using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Collections;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Invoices;
using WebAPI.InvoiceToCollectionProcesses;
using WebAPI.ProformaToInvoiceProcesses;

namespace WebAPI.Proformas;

public static class GetProformaTracking
{
    public class Query
    {
        public Guid ProformaId { get; set; }
    }

    public class Result
    {
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Commission { get; set; }
        public decimal Discount { get; set; }
        public Invoice[] Invoices { get; set; }
        public decimal TotalInvoices { get; set; }
        public Collection[] Collections { get; set; }
        public decimal TotalCollections { get; set; }
        public class Invoice
        {
            public decimal Total { get; set; }
            public Guid InvoiceId { get; set; }

        }
        public class Collection
        {
            public decimal Total { get; set; }
            public Guid CollectionId { get; set; }
        }
    }





    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid proformaId,
    HttpContext context)
    {
        var proforma = await dbContext.Set<Proforma>().AsNoTracking().FirstAsync(x => x.ProformaId == proformaId);

        var proformaInvoices = await (from proformas in dbContext.Set<Proforma>().AsNoTracking()
                                      join items in dbContext.Set<ProformaToInvoiceProcessItem>().AsNoTracking() on proformas.ProformaId equals items.ProformaId
                                      join invoices in dbContext.Set<Invoice>().AsNoTracking() on items.InvoiceId equals invoices.InvoiceId
                                      where proformas.ProformaId == proformaId
                                      select new Result.Invoice() { InvoiceId = invoices.InvoiceId, Total = invoices.Total }).ToArrayAsync();

        var proformaCollections = await (from invoices in dbContext.Set<Invoice>().AsNoTracking()
                                         join items in dbContext.Set<InvoiceToCollectionProcessItem>().AsNoTracking() on invoices.InvoiceId equals items.InvoiceId
                                         join collections in dbContext.Set<Collection>().AsNoTracking() on items.CollectionId equals collections.CollectionId
                                         where proformaInvoices.Select(x => x.InvoiceId).Contains(invoices.InvoiceId)
                                         select new Result.Collection() { CollectionId = collections.CollectionId, Total = collections.Total }).ToArrayAsync();

        var result = new Result()
        {
            Total = proforma.Total,
            Commission = proforma.Commission,
            Discount = proforma.Discount,
            SubTotal = proforma.SubTotal,
            Invoices = proformaInvoices,
            TotalInvoices = proformaInvoices.Sum(x => x.Total),
            Collections = proformaCollections,
            TotalCollections = proformaCollections.Sum(X => X.Total)
        };

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<GetProformaTrackingPage>(new
        {
            Result = result,
        });
    }
}
