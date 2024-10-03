using Infrastructure;

namespace Collections
{
    public class CollectionStorage : AzureBlobStorage
    {
        public CollectionStorage(string connectionString) : base(connectionString, "collections")
        {
        }
    }
}
