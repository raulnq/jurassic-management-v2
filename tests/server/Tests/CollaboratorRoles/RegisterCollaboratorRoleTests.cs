using Tests.Infrastructure;

namespace Tests.CollaboratorRoles;

public class RegisterCollaboratorRoleTests : BaseTest
{
    [Fact]
    public Task register_should_be_ok()
    {
        return _appDsl.CollaboratorRole.Register();
    }
}