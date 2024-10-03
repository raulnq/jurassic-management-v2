using Tests.Infrastructure;

namespace Tests.Invoices;

public class IssueInvoiceTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(today);

        var start = await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        await _appDsl.Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);

        await _appDsl.Invoice.Issue(start!.InvoiceId, c =>
        {
            c.IssuedAt = today.AddDays(1);
        });

    }
}
