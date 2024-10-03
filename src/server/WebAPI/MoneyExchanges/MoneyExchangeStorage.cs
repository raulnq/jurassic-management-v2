using Infrastructure;

namespace MoneyExchanges
{
    public class MoneyExchangeStorage : AzureBlobStorage
    {
        public MoneyExchangeStorage(string connectionString) : base(connectionString, "moneyexchanges")
        {
        }
    }
}
