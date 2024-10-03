using Tests.Infrastructure;

namespace Tests.PayrollPayments;

public class RegisterPayrollPaymentTests : BaseTest
{
    [Fact]
    public async Task register_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        await _appDsl.PayrollPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);
    }
}

