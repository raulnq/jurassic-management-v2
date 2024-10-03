using Infrastructure;
using System.Net.Mime;

namespace Invoices
{
    public class InvoiceStorage : AzureBlobStorage
    {
        public InvoiceStorage(string connectionString) : base(connectionString, "invoices")
        {
        }

        public Task<string> Upload(string name, Stream stream)
        {
            return Upload(name, stream, MediaTypeNames.Application.Pdf);
        }
    }
}
