using Tests.Infrastructure;

namespace Tests.Proformas;

public class RemoveWorkItemTests : BaseTest
{
    [Fact]
    public async Task remove_should_be_ok()
    {
        var (result, _, _, proformaResult) = await _appDsl.RegisterProforma(_appDsl.Clock.Now.DateTime, clientSetup: c =>
        {
            c.AdministrativeExpensesPercentage = 1;
            c.TaxesExpensesPercentage = 1;
            c.BankingExpensesPercentage = 1;
            c.MinimumBankingExpenses = 25;
            c.PenaltyMinimumHours = 15;
            c.PenaltyAmount = 30;
        });

        var (_, collaborator) = await _appDsl.Collaborator.Register();

        var (_, collaboratorRole) = await _appDsl.CollaboratorRole.Register();

        await _appDsl.Proformas.AddWorkItem(c =>
        {
            c.ProformaId = result!.ProformaId;
            c.Week = 1;
            c.CollaboratorId = collaborator!.CollaboratorId;
            c.CollaboratorRoleId = collaboratorRole!.CollaboratorRoleId;
            c.Hours = 15;
            c.FreeHours = 0;
        });

        await _appDsl.Proformas.RemoveWorkItem(c =>
        {
            c.ProformaId = result!.ProformaId;
            c.Week = 1;
            c.CollaboratorId = collaborator!.CollaboratorId;
        });

        await _appDsl.Proformas.WeekShouldHaveRightAmounts(result!.ProformaId, 1, 450, 450);

        await _appDsl.Proformas.ShouldHaveRightAmounts(result!.ProformaId, 450, 4.5m, 4.5m, 25, 34, 484);
    }
}
