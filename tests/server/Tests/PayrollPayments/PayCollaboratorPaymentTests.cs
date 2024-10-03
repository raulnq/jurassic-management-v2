using Tests.Infrastructure;

namespace Tests.PayrollPayments;

public class PayPayrollPaymentTests : BaseTest
{
    [Fact]
    public async Task pay_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        var (_, payrollPaymentResult) = await _appDsl.PayrollPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);

        await _appDsl.PayrollPayment.Pay(payrollPaymentResult!.PayrollPaymentId);
    }
}
