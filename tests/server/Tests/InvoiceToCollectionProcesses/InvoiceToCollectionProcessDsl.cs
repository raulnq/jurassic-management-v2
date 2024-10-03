
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.InvoiceToCollectionProcesses;

namespace Tests.InvoiceToCollectionProcesses;

public class InvoiceToCollectionProcessDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "invoice-to-collection-processes";
    public InvoiceToCollectionProcessDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(StartInvoiceToCollectionProcess.Command, StartInvoiceToCollectionProcess.Result?)> Start(Action<StartInvoiceToCollectionProcess.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<StartInvoiceToCollectionProcess.Command>()
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<StartInvoiceToCollectionProcess.Command, StartInvoiceToCollectionProcess.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.CollectionId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}