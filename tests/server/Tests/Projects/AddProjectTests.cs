using Tests.Infrastructure;

namespace Tests.Projects;

public class AddProjectTests : BaseTest
{
    [Fact]
    public async Task add_should_be_ok()
    {
        var (_, client) = await _appDsl.Client.Register();

        await _appDsl.Project.Add(client!.ClientId);
    }
}