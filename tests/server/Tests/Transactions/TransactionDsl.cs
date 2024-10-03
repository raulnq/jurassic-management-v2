
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Transactions;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace Tests.Transactions;

public class TransactionDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "transactions";
    public TransactionDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterTransaction.Command, RegisterTransaction.Result?)> Register(Action<RegisterTransaction.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterTransaction.Command>()
            .RuleFor(command => command.Description, faker => faker.Lorem.Sentence())
            .RuleFor(command => command.SubTotal, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.Taxes, faker => faker.Random.Number(0, 50))
            .RuleFor(command => command.Number, faker => faker.Random.AlphaNumeric(5))
            .RuleFor(command => command.Currency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.Type, faker => faker.Random.Enum<TransactionType>())
            .RuleFor(command => command.IssuedAt, faker => faker.Date.Future())
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterTransaction.Command, RegisterTransaction.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.TransactionId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditTransaction.Command> Edit(Guid transactionId, Action<EditTransaction.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditTransaction.Command>()
            .RuleFor(command => command.Description, faker => faker.Lorem.Sentence())
            .RuleFor(command => command.SubTotal, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.Taxes, faker => faker.Random.Number(0, 50))
            .RuleFor(command => command.Number, faker => faker.Random.AlphaNumeric(5))
            .RuleFor(command => command.Currency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.Type, faker => faker.Random.Enum<TransactionType>())
            .RuleFor(command => command.IssuedAt, faker => faker.Date.Future())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{transactionId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListTransactions.Query, ListResults<ListTransactions.Result>?)> List(Action<ListTransactions.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListTransactions.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListTransactions.Query, ListResults<ListTransactions.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetTransaction.Query, GetTransaction.Result?)> Get(Action<GetTransaction.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetTransaction.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetTransaction.Query, GetTransaction.Result>($"{_uri}/{request.TransactionId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TransactionId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}