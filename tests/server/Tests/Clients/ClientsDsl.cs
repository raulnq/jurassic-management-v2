
using Bogus;
using Bogus.Extensions.UnitedStates;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Clients;
using WebAPI.Infrastructure.SqlKata;

namespace Tests.Clients;

public class ClientDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "clients";
    public ClientDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterClient.Command, RegisterClient.Result?)> Register(Action<RegisterClient.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterClient.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            .RuleFor(command => command.PhoneNumber, faker => faker.Phone.PhoneNumber(("##########")))
            .RuleFor(command => command.DocumentNumber, faker => faker.Person.Ssn())
            .RuleFor(command => command.Address, faker => faker.Address.FullAddress())
            .RuleFor(command => command.PenaltyMinimumHours, faker => faker.Random.Number(10, 15))
            .RuleFor(command => command.PenaltyAmount, faker => faker.Random.Number(20, 40))
            .RuleFor(command => command.TaxesExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.AdministrativeExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.BankingExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.MinimumBankingExpenses, faker => faker.Random.Number(20, 30))
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterClient.Command, RegisterClient.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.ClientId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditClient.Command> Edit(Guid clientId, Action<EditClient.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditClient.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            .RuleFor(command => command.PhoneNumber, faker => faker.Phone.PhoneNumber(("##########")))
            .RuleFor(command => command.DocumentNumber, faker => faker.Person.Ssn())
            .RuleFor(command => command.Address, faker => faker.Address.FullAddress())
            .RuleFor(command => command.PenaltyMinimumHours, faker => faker.Random.Number(10, 15))
            .RuleFor(command => command.PenaltyAmount, faker => faker.Random.Number(20, 40))
            .RuleFor(command => command.TaxesExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.AdministrativeExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.BankingExpensesPercentage, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.MinimumBankingExpenses, faker => faker.Random.Number(20, 30))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{clientId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListClients.Query, ListResults<ListClients.Result>?)> List(Action<ListClients.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListClients.Query>()
            .RuleFor(command => command.Name, faker => faker.Lorem.Word());

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListClients.Query, ListResults<ListClients.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetClient.Query, GetClient.Result?)> Get(Action<GetClient.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetClient.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetClient.Query, GetClient.Result>($"{_uri}/{request.ClientId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.ClientId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}