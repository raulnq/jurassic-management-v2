
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.ProformaToInvoiceProcesses;

namespace Tests.ProformaToInvoiceProcesses;

public class ProformaToInvoiceProcessDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "proforma-to-invoice-processes";
    public ProformaToInvoiceProcessDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(StartProformaToInvoiceProcess.Command, StartProformaToInvoiceProcess.Result?)> Start(Action<StartProformaToInvoiceProcess.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<StartProformaToInvoiceProcess.Command>()
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<StartProformaToInvoiceProcess.Command, StartProformaToInvoiceProcess.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.InvoiceId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}