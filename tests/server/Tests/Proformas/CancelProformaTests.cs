using Tests.Infrastructure;

namespace Tests.Proformas;

public class CancelProformaTests : BaseTest
{
    [Fact]
    public async Task cancel_should_be_ok()
    {
        var (proformaResult, _, _, _) = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.Proformas.Cancel(proformaResult.ProformaId);
    }

    [Fact]
    public async Task cancel_should_throw_an_error_when_proforma_is_not_pending()
    {
        var (proformaResult, _, _, _) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.Proformas.Cancel(proformaResult.ProformaId, errorDetail: "code: proforma-status-not-pending");
    }
}