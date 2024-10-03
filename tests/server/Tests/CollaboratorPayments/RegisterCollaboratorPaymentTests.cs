using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class RegisterCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task register_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        await _appDsl.CollaboratorPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);
    }
}

