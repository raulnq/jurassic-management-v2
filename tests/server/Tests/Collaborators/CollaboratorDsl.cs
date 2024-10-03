
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.SqlKata;

namespace Tests.Collaborators;

public class CollaboratorDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "collaborators";
    public CollaboratorDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterCollaborator.Command, RegisterCollaborator.Result?)> Register(Action<RegisterCollaborator.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterCollaborator.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterCollaborator.Command, RegisterCollaborator.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.CollaboratorId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditCollaborator.Command> Edit(Guid collaboratorId, Action<EditCollaborator.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditCollaborator.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{collaboratorId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListCollaborators.Query, ListResults<ListCollaborators.Result>?)> List(Action<ListCollaborators.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListCollaborators.Query>()
            .RuleFor(command => command.Name, faker => faker.Lorem.Word());

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListCollaborators.Query, ListResults<ListCollaborators.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetCollaborator.Query, GetCollaborator.Result?)> Get(Action<GetCollaborator.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetCollaborator.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetCollaborator.Query, GetCollaborator.Result>($"{_uri}/{request.CollaboratorId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.CollaboratorId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}