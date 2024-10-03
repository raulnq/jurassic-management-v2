using Tests.Infrastructure;

namespace Tests.MoneyExchanges;

public class RegisterMoneyExchangeTests : BaseTest
{
    [Fact]
    public Task register_should_be_ok()
    {
        return _appDsl.MoneyExchange.Register();
    }
}