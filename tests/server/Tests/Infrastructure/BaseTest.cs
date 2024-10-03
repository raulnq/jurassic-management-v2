namespace Tests.Infrastructure;

public class BaseTest : IAsyncLifetime
{
    public readonly AppDsl _appDsl;

    public BaseTest()
    {
        _appDsl = new AppDsl();
    }

    public Task DisposeAsync()
    {
        return _appDsl.DisposeAsync().AsTask();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
