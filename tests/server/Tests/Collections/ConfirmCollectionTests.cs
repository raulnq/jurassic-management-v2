using Tests.Infrastructure;

namespace Tests.Collections;

public class ConfirmCollectionTests : BaseTest
{
    [Fact]
    public async Task confirm_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var (proformaResult, proformaCommand, clientResult, _) = await _appDsl.IssueProforma(today);

        var invoice = await _appDsl.IssueInvoice(proformaResult.ProformaId, clientResult.ClientId, proformaCommand.Currency, today);

        var (_, start) = await _appDsl.InvoiceToCollectionProcess.Start(c =>
        {
            c.ClientId = clientResult.ClientId;
            c.Currency = proformaCommand.Currency;
            c.InvoiceId = new[] { invoice.InvoiceId };
        });

        var (_, collection) = await _appDsl.Collection.Get(q => q.CollectionId = start!.CollectionId);

        await _appDsl.Collection.Confirm(start!.CollectionId, c =>
        {
            c.ConfirmedAt = today.AddDays(1);
            c.Total = collection!.Total;
            c.Commission = collection!.Commission;
        });

    }
}