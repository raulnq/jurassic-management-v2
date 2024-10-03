using AsyncAwaitBestPractices;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure
{
    public class AzureBlobStorage
    {
        protected readonly BlobContainerClient _container;

        protected AzureBlobStorage(string connectionString, string container)
        {
            _container = new BlobContainerClient(connectionString, container);

            _container.CreateIfNotExistsAsync(PublicAccessType.Blob).SafeFireAndForget();
        }

        public async Task<string> Upload(string name, Stream stream, string contentType)
        {
            var blob = _container.GetBlobClient(name);

            var headers = new BlobHttpHeaders()
            {
                ContentType = contentType,
            };

            await blob.UploadAsync(stream, headers);

            return blob.Uri.AbsoluteUri;
        }

        public async Task<Stream> Download(string name)
        {
            var blob = _container.GetBlobClient(name);

            var memoryStream = new MemoryStream();

            await blob.DownloadToAsync(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
