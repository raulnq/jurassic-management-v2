using Tests.Infrastructure;

namespace Tests.Collaborators;

public class RegisterCollaboratorTests : BaseTest
{
    [Fact]
    public Task register_should_be_ok()
    {
        return _appDsl.Collaborator.Register();
    }

    [Fact]
    public Task register_should_throw_an_error_when_properties_are_empty()
    {
        return _appDsl.Collaborator.Register(command =>
        {
            command.Name = null;
        }, errorDetail: "validation-error", new Dictionary<string, string[]> {
            { "Name", new string[] { "'Name' must not be empty." } },
        });
    }

    [Fact]
    public Task register_should_throw_an_error_when_properties_are_too_long()
    {
        return _appDsl.Collaborator.Register(command =>
        {
            command.Name = new string('*', 101);
        }, errorDetail: "validation-error", new Dictionary<string, string[]> {
            { "Name", new string[] { "The length of 'Name' must be 100 characters or fewer. You entered 101 characters." } },
        });
    }

    //[Fact]
    //public async Task register_should_throw_an_error_when_name_is_duplicated()
    //{
    //    var (command, result) = await _appDsl.Collaborator.Register();

    //    await _appDsl.Collaborator.Register(c => { c.Name = command.Name; }, errorDetail: "El beneficiario se encuentra duplicado.");
    //}
}