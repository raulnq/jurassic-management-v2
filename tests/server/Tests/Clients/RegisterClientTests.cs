using Tests.Infrastructure;

namespace Tests.Clients;

public class RegisterClientTests : BaseTest
{
    [Fact]
    public Task register_should_be_ok()
    {
        return _appDsl.Client.Register();
    }
}