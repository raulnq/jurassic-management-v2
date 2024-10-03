using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostmarkDotNet;
using WebAPI.Companies;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.ProformaToCollaboratorPaymentProcesses;

namespace WebAPI.CollaboratorPayments;

public static class SendCollaboratorPayment
{
    public static async Task<Ok> Handle(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] PostmarkClient client,
        [FromServices] Company company,
        [FromRoute] Guid collaboratorPaymentId)
    {

        var collaboratorPayment = await dbContext.Set<CollaboratorPayment>().AsNoTracking().FirstAsync(cp => cp.CollaboratorPaymentId == collaboratorPaymentId);

        var result = await new ListProformaToCollaboratorPaymentProcessItems.Runner(runner)
            .Run(new ListProformaToCollaboratorPaymentProcessItems.Query() { CollaboratorPaymentId = collaboratorPaymentId, PageSize = 100 });

        if (result.Items.Any())
        {
            var first = result.Items.First();

            if (string.IsNullOrEmpty(first.CollaboratorEmail))
            {
                throw new DomainException("no-collaborator-email");
            }

            if (string.IsNullOrEmpty(first.ProformaNote))
            {
                throw new DomainException("no-note");
            }

            var message = new TemplatedPostmarkMessage
            {
                From = company.FromEmail,
                To = first.CollaboratorEmail,
                Cc = string.Join(",", company.CcEmails),
                TemplateAlias = "collaboratorpayment",
                TemplateModel = new Dictionary<string, object> {
            { "project_name", first!.ProjectName! },
            { "document_number", company.DocumentNumber! },
            { "company_name", company.Name! },
            { "payAt", collaboratorPayment.PaidAt.ToListFormat() },
            { "note", first.ProformaNote! },
            { "gross_amount", collaboratorPayment.GrossSalary.ToMoneyFormat() },
            { "currency", collaboratorPayment.Currency.ToString() },
            { "withholding_amount", collaboratorPayment.Withholding.ToMoneyFormat() },
            { "net_amount",  collaboratorPayment.NetSalary.ToMoneyFormat() },
            { "company_address", company.Address! },
          },
            };

            var response = await client.SendMessageAsync(message);

            if (response.Status != PostmarkStatus.Success)
            {
                throw new InfrastructureException(response.ErrorCode.ToString(), response.Message);
            }
        }
        else
        {
            throw new DomainException("no-proforma-link");
        }

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] PostmarkClient client,
        [FromServices] Company company,
        HttpContext httpContext,
        Guid collaboratorPaymentId)
    {
        await Handle(dbContext, runner, client, company, collaboratorPaymentId);

        httpContext.Response.Headers.TriggerShowSuccessMessage($"collaborator payment", "sent", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}

