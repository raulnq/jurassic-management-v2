using Tests.Infrastructure;

namespace Tests.Proformas;

public class RegisterProformaTests : BaseTest
{
    [Fact]
    public async Task register_should_be_ok()
    {
        var (_, client) = await _appDsl.Client.Register();

        var (_, project) = await _appDsl.Project.Add(client!.ClientId);

        var (_, proforma) = await _appDsl.Proformas.Register(c =>
        {
            c.ProjectId = project!.ProjectId;
            c.Start = _appDsl.Clock.Now.DateTime;
            c.End = c.Start.AddDays(6);
        });
    }

    [Fact]
    public async Task register_should_be_ok_with_multiple_week()
    {
        var (_, client) = await _appDsl.Client.Register();

        var (_, project) = await _appDsl.Project.Add(client!.ClientId);

        var (_, proforma) = await _appDsl.Proformas.Register(c =>
        {
            c.ProjectId = project!.ProjectId;
            c.Start = _appDsl.Clock.Now.DateTime;
            c.End = c.Start.AddDays(28 - 1);
        });
    }

    [Fact]
    public async Task register_should_be_ok_with_same_start_end()
    {
        var (_, client) = await _appDsl.Client.Register();

        var (_, project) = await _appDsl.Project.Add(client!.ClientId);

        var (_, proforma) = await _appDsl.Proformas.Register(c =>
        {
            c.ProjectId = project!.ProjectId;
            c.Start = _appDsl.Clock.Now.DateTime;
            c.End = c.Start;
        });
    }

    [Fact]
    public async Task register_should_be_ok_with_not_complete_week()
    {
        var (_, client) = await _appDsl.Client.Register();

        var (_, project) = await _appDsl.Project.Add(client!.ClientId);

        var (_, proforma) = await _appDsl.Proformas.Register(c =>
        {
            c.ProjectId = project!.ProjectId;
            c.Start = _appDsl.Clock.Now.DateTime;
            c.End = c.Start.AddDays(10);
        });
    }
}
