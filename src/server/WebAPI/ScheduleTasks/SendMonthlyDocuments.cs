using Collections;
using Coravel.Invocable;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using MoneyExchanges;
using PostmarkDotNet;
using System.Net.Mime;
using Transactions;
using WebAPI.Collections;
using WebAPI.Companies;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.MoneyExchanges;
using WebAPI.PayrollPayments;
using WebAPI.Transactions;

namespace WebAPI.ScheduleTasks
{
    public class SendMonthlyDocuments : IInvocable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IClock _clock;
        private readonly Company _company;
        private readonly MoneyExchangeStorage _mestorage;
        private readonly CollectionStorage _cstorage;
        private readonly TransactionStorage _tstorage;
        private readonly PostmarkClient _client;
        public static string CRON = "0 0 1 * *";
        public SendMonthlyDocuments(ApplicationDbContext dbContext,
            IClock clock,
            Company company,
            MoneyExchangeStorage moneyExchangeStorage,
            CollectionStorage collectionStorage,
            TransactionStorage transactionStorage,
            PostmarkClient client)
        {
            _dbContext = dbContext;
            _clock = clock;
            _company = company;
            _mestorage = moneyExchangeStorage;
            _cstorage = collectionStorage;
            _tstorage = transactionStorage;
            _client = client;
        }
        public async Task Invoke()
        {
            var priorMonth = _clock.Now.DateTime.AddMonths(-1);
            var start = new DateTime(priorMonth.Year, priorMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var moneyExchanges = await _dbContext.Set<MoneyExchange>()
                .AsNoTracking().Where(me => me.IssuedAt < end && me.IssuedAt >= start && me.DocumentUrl != null).ToListAsync();

            var transactions = await _dbContext.Set<Transaction>()
                .AsNoTracking().Where(t => t.IssuedAt < end && t.IssuedAt >= start && t.Type == TransactionType.Expenses && t.DocumentUrl != null).ToListAsync();

            var collections = await _dbContext.Set<Collection>()
                .AsNoTracking().Where(c => c.ConfirmedAt < end && c.ConfirmedAt >= start && c.DocumentUrl != null).ToListAsync();

            var payrollPayments = await _dbContext.Set<PayrollPayment>()
                .AsNoTracking().Where(c => c.PaidAt < end && c.PaidAt >= start && c.DocumentUrl != null).ToListAsync();

            if (!string.IsNullOrEmpty(_company.AccountantEmail))
            {
                var message = new TemplatedPostmarkMessage
                {
                    From = _company.FromEmail,
                    To = _company.AccountantEmail,
                    Cc = string.Join(",", _company.CcEmails),
                    TemplateAlias = "monthly-documents",
                    TemplateModel = new Dictionary<string, object> {
                        { "project_name", _company.Name! },
                        { "month", priorMonth.ToString("MMMM",System.Globalization.CultureInfo.GetCultureInfo("es-ES")) },
                        { "year", priorMonth.Year },
                        { "company_name", _company.Name! },
                        { "company_address", _company.Address! },
                    },
                };

                foreach (var p in payrollPayments)
                {
                    var file = p.DocumentUrl!.Split("/").Last();

                    using (var stream = await _mestorage.Download(file))
                    {
                        message.AddAttachment(stream, $"payroll_payment_{p.PaidAt?.ToString("ddMMyyyy")}_{p.PayrollPaymentId}.pdf", MediaTypeNames.Application.Pdf);
                    }
                }

                foreach (var me in moneyExchanges)
                {
                    var file = me.DocumentUrl!.Split("/").Last();

                    using (var stream = await _mestorage.Download(file))
                    {
                        message.AddAttachment(stream, $"money_exchange_{me.IssuedAt?.ToString("ddMMyyyy")}_{me.MoneyExchangeId}.pdf", MediaTypeNames.Application.Pdf);
                    }
                }

                foreach (var t in transactions)
                {
                    var file = t.DocumentUrl!.Split("/").Last();

                    using (var stream = await _tstorage.Download(file))
                    {
                        message.AddAttachment(stream, $"transaction_{t.IssuedAt?.ToString("ddMMyyyy")}_{t.TransactionId}.pdf", MediaTypeNames.Application.Pdf);
                    }
                }

                foreach (var c in collections)
                {
                    var file = c.DocumentUrl!.Split("/").Last();

                    using (var stream = await _cstorage.Download(file))
                    {
                        message.AddAttachment(stream, $"collection_{c.ConfirmedAt?.ToString("ddMMyyyy")}_{c.CollectionId}.pdf", MediaTypeNames.Application.Pdf);
                    }
                }

                var response = await _client.SendMessageAsync(message);

                if (response.Status != PostmarkStatus.Success)
                {
                    throw new InfrastructureException(response.ErrorCode.ToString(), response.Message);
                }
            }
        }
    }
}
