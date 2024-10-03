using Infrastructure;
using System.Net.Mime;

namespace CollaboratorPayments;

public class CollaboratorPaymentStorage : AzureBlobStorage
{
    public CollaboratorPaymentStorage(string connectionString) : base(connectionString, "payment")
    {
    }

    public Task<string> Upload(string name, Stream stream)
    {
        return Upload(name, stream, MediaTypeNames.Application.Pdf);
    }
}
