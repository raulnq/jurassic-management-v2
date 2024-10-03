
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.CollaboratorRoles;
using WebAPI.Infrastructure.SqlKata;

namespace Tests.CollaboratorRoles;

public class CollaboratorRoleDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "collaborator-roles";
    public CollaboratorRoleDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterCollaboratorRole.Command, RegisterCollaboratorRole.Result?)> Register(Action<RegisterCollaboratorRole.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterCollaboratorRole.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            .RuleFor(command => command.FeeAmount, faker => faker.Random.Number(20, 40))
            .RuleFor(command => command.ProfitPercentage, faker => faker.Random.Number(1, 5))
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterCollaboratorRole.Command, RegisterCollaboratorRole.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.CollaboratorRoleId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditCollaboratorRole.Command> Edit(Guid collaboratorRoleId, Action<EditCollaboratorRole.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditCollaboratorRole.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            .RuleFor(command => command.FeeAmount, faker => faker.Random.Number(20, 40))
            .RuleFor(command => command.ProfitPercentage, faker => faker.Random.Number(1, 5))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{collaboratorRoleId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListCollaboratorRoles.Query, ListResults<ListCollaboratorRoles.Result>?)> List(Action<ListCollaboratorRoles.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListCollaboratorRoles.Query>()
            .RuleFor(command => command.Name, faker => faker.Lorem.Word());

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListCollaboratorRoles.Query, ListResults<ListCollaboratorRoles.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetCollaboratorRole.Query, GetCollaboratorRole.Result?)> Get(Action<GetCollaboratorRole.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetCollaboratorRole.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetCollaboratorRole.Query, GetCollaboratorRole.Result>($"{_uri}/{request.CollaboratorRoleId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.CollaboratorRoleId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}