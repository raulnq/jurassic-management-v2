using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostmarkDotNet;
using System.Net.Mime;
using WebAPI.ClientContacts;
using WebAPI.Companies;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.ProformaDocuments;

public static class SendProformaDcument
{
    public static async Task<Ok> Handle(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] PostmarkClient client,
        [FromServices] Company company,
        [FromRoute] Guid proformaId,
        [FromServices] ProformaDocumentStorage storage)
    {
        var proforma = await new GetProforma.Runner(runner).Run(new GetProforma.Query() { ProformaId = proformaId });

        var contacts = await dbContext.Set<ClientContact>().AsNoTracking().Where(cc => cc.ClientId == proforma.ClientId).ToListAsync();

        if (contacts.Any())
        {
            var message = new TemplatedPostmarkMessage
            {
                From = company.FromEmail,
                To = string.Join(",", contacts.Select(c => c.Email)),
                Cc = string.Join(",", company.CcEmails),
                TemplateAlias = "proforma",
                TemplateModel = new Dictionary<string, object> {
            { "project_name", proforma!.ProjectName! },
            { "month", proforma.Start.ToString("MMMM",System.Globalization.CultureInfo.GetCultureInfo("es-ES")) },
            { "year", proforma.Start.Year },
            { "company_name", company.Name! },
            { "company_address", company.Address! },
          },
            };

            using (var stream = await storage.Download($"{proformaId}.pdf"))
            {
                message.AddAttachment(stream, $"proforma{proforma.Start.ToString("MMyyyy")}.pdf", MediaTypeNames.Application.Pdf);

                var response = await client.SendMessageAsync(message);

                if (response.Status != PostmarkStatus.Success)
                {
                    throw new InfrastructureException(response.ErrorCode.ToString(), response.Message);
                }
            }
        }
        else
        {
            throw new DomainException("no-contacts");
        }

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ProformaDocumentStorage storage,
        [FromServices] PostmarkClient client,
        [FromServices] Company company,
        HttpContext httpContext,
        Guid proformaId)
    {
        await Handle(dbContext, runner, client, company, proformaId, storage);

        httpContext.Response.Headers.TriggerShowSuccessMessage($"proforma", "sent", proformaId);

        return await GetProforma.HandlePage(runner, dbContext, proformaId);
    }
}
