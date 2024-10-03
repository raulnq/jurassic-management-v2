
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.MoneyExchanges;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace Tests.MoneyExchanges;

public class MoneyExchangeDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "money-exchanges";
    public MoneyExchangeDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterMoneyExchange.Command, RegisterMoneyExchange.Result?)> Register(Action<RegisterMoneyExchange.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterMoneyExchange.Command>()
            .RuleFor(command => command.ToCurrency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.FromAmount, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.Rate, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.ToAmount, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.FromCurrency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.IssuedAt, faker => faker.Date.Future())
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterMoneyExchange.Command, RegisterMoneyExchange.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.MoneyExchangeId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditMoneyExchange.Command> Edit(Guid moneyExchangeId, Action<EditMoneyExchange.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditMoneyExchange.Command>()
            .RuleFor(command => command.ToCurrency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.FromAmount, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.Rate, faker => faker.Random.Number(1, 5))
            .RuleFor(command => command.ToAmount, faker => faker.Random.Number(1, 400))
            .RuleFor(command => command.FromCurrency, faker => faker.Random.Enum<Currency>())
            .RuleFor(command => command.IssuedAt, faker => faker.Date.Future())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{moneyExchangeId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListMoneyExchanges.Query, ListResults<ListMoneyExchanges.Result>?)> List(Action<ListMoneyExchanges.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListMoneyExchanges.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListMoneyExchanges.Query, ListResults<ListMoneyExchanges.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetMoneyExchange.Query, GetMoneyExchange.Result?)> Get(Action<GetMoneyExchange.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetMoneyExchange.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetMoneyExchange.Query, GetMoneyExchange.Result>($"{_uri}/{request.MoneyExchangeId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.MoneyExchangeId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}