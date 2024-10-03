using Infrastructure;
using System.Net.Mime;

namespace WebAPI.ProformaDocuments
{
    public class ProformaDocumentStorage : AzureBlobStorage
    {
        public ProformaDocumentStorage(string connectionString) : base(connectionString, "proformas")
        {
        }

        public Task<string> Upload(string name, Stream stream)
        {
            return Upload(name, stream, MediaTypeNames.Application.Pdf);
        }
    }
}
