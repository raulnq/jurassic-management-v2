using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class EditCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task edit_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        var (_, collaboratorPaymentResult) = await _appDsl.CollaboratorPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);

        await _appDsl.CollaboratorPayment.Edit(collaboratorPaymentResult.CollaboratorPaymentId);
    }
}

