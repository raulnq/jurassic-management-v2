
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Collections;
using WebAPI.Infrastructure.SqlKata;

namespace Tests.Collections;

public class CollectionDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "collections";
    public CollectionDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }
    public async Task<ConfirmCollection.Command> Confirm(Guid collectionId, Action<ConfirmCollection.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<ConfirmCollection.Command>()
            .RuleFor(command => command.Number, faker => faker.Random.Guid().ToString())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{collectionId}/confirm", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListCollections.Query, ListResults<ListCollections.Result>?)> List(Action<ListCollections.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListCollections.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListCollections.Query, ListResults<ListCollections.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetCollection.Query, GetCollection.Result?)> Get(Action<GetCollection.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetCollection.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetCollection.Query, GetCollection.Result>($"{_uri}/{request.CollectionId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.CollectionId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}