using Infrastructure;
using System.Net.Mime;

namespace WebAPI.TaxPayments;

public class TaxPaymentStorage : AzureBlobStorage
{
    public TaxPaymentStorage(string connectionString) : base(connectionString, "tax")
    {
    }

    public Task<string> Upload(string name, Stream stream)
    {
        return Upload(name, stream, MediaTypeNames.Application.Pdf);
    }
}
