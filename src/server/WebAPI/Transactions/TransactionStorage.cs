using Infrastructure;

namespace Transactions
{
    public class TransactionStorage : AzureBlobStorage
    {
        public TransactionStorage(string connectionString) : base(connectionString, "transactions")
        {
        }
    }
}
