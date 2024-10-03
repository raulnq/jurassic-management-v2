using Tests.Infrastructure;

namespace Tests.Invoices;

public class UploadDocumentTests : BaseTest
{
    [Fact]
    public async Task upload_should_be_ok()
    {
        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(_appDsl.Clock.Now.DateTime);

        var start = await _appDsl.RegisterInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency);

        await _appDsl.Invoice.Upload("blank.pdf", c => c.InvoiceId = start!.InvoiceId);
    }
}
