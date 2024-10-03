
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.ProformaToCollaboratorPaymentProcesses;

namespace Tests.ProformaToCollaboratorPaymentProcesses;

public class ProformaToCollaboratorPaymentProcessDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "proforma-to-collaborator-payment-processes";
    public ProformaToCollaboratorPaymentProcessDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(StartProformaToCollaboratorPaymentProcess.Command, StartProformaToCollaboratorPaymentProcess.Result?)> Start(Action<StartProformaToCollaboratorPaymentProcess.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<StartProformaToCollaboratorPaymentProcess.Command>()
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<StartProformaToCollaboratorPaymentProcess.Command, StartProformaToCollaboratorPaymentProcess.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.CollaboratorPaymentId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}