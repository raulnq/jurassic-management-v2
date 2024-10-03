using Tests.Infrastructure;

namespace Tests.Transactions;

public class RegisterTransactionsTests : BaseTest
{
    [Fact]
    public Task register_should_be_ok()
    {
        return _appDsl.Transaction.Register();
    }
}