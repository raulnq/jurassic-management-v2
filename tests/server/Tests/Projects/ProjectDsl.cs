
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Projects;

namespace Tests.Projects;

public class ProjectDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "clients";
    public ProjectDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(AddProject.Command, AddProject.Result?)> Add(Guid clientId, Action<AddProject.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<AddProject.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<AddProject.Command, AddProject.Result>($"{_uri}/{clientId}/projects", request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.ProjectId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<EditProject.Command> Edit(Guid projectId, Action<EditProject.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditProject.Command>()
            .RuleFor(command => command.Name, faker => faker.Random.Guid().ToString())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{Guid.NewGuid()}/projects/{projectId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListProjects.Query, ListResults<ListProjects.Result>?)> List(Action<ListProjects.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListProjects.Query>()
            .RuleFor(command => command.Name, faker => faker.Lorem.Word());

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListProjects.Query, ListResults<ListProjects.Result>>($"{_uri}/{Guid.NewGuid()}/projects", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetProject.Query, GetProject.Result?)> Get(Action<GetProject.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetProject.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetProject.Query, GetProject.Result>($"{_uri}/{Guid.NewGuid()}/projects/{request.ProjectId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.ProjectId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}