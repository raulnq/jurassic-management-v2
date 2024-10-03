using Tests.Infrastructure;

namespace Tests.Proformas;

public class IssueProformaTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, _, _, _) = await _appDsl.RegisterProformaReadyToIssue(today);

        await _appDsl.Proformas.Issue(proformaResult.ProformaId, c =>
        {
            c.IssuedAt = today.AddDays(7);
        });
    }
}