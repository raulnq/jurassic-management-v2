
using Bogus;
using Shouldly;
using Tests.Infrastructure;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace Tests.Proformas;

public class ProformasDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "proformas";
    public ProformasDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<(RegisterProforma.Command, RegisterProforma.Result?)> Register(Action<RegisterProforma.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterProforma.Command>()
            .RuleFor(command => command.Discount, faker => faker.Random.Number(0, 5))
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterProforma.Command, RegisterProforma.Result>(_uri, request);

        (status, result, error).Check(errorDetail, errors: errors, successAssert: result =>
        {
            result.ProformaId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task<AddWorkItem.Command> AddWorkItem(Action<AddWorkItem.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<AddWorkItem.Command>()
            .RuleFor(command => command.Hours, faker => faker.Random.Number(1, 10))
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{request.ProformaId}/weeks/{request.Week}/work-items", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<EditWorkItem.Command> EditWorkItem(Guid proformaId, int week, Guid collaboratorId, Action<EditWorkItem.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditWorkItem.Command>()
            .RuleFor(command => command.Hours, faker => faker.Random.Number(1, 10))
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{proformaId}/weeks/{week}/work-items/{collaboratorId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<RemoveWorkItem.Command> RemoveWorkItem(Action<RemoveWorkItem.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RemoveWorkItem.Command>()
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Delete($"{_uri}/{request.ProformaId}/weeks/{request.Week}/work-items/{request.CollaboratorId}");

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<IssueProforma.Command> Issue(Guid proformaId, Action<IssueProforma.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<IssueProforma.Command>()
            ;

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{proformaId}/issue", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<CancelProforma.Command> Cancel(Guid proformaId, Action<CancelProforma.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<CancelProforma.Command>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{proformaId}/cancel", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }


    public async Task<(ListProformas.Query, ListResults<ListProformas.Result>?)> List(Action<ListProformas.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListProformas.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListProformas.Query, ListResults<ListProformas.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(ListProformaWeeks.Query, ListResults<ListProformaWeeks.Result>?)> ListWeeks(Action<ListProformaWeeks.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListProformaWeeks.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListProformaWeeks.Query, ListResults<ListProformaWeeks.Result>>($"{_uri}/{request.ProformaId}/weeks", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetProformaWeekWorkItem.Query, GetProformaWeekWorkItem.Result?)> GetWorkItem(Action<GetProformaWeekWorkItem.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetProformaWeekWorkItem.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetProformaWeekWorkItem.Query, GetProformaWeekWorkItem.Result>($"{_uri}/{request.ProformaId}/weeks/{request.Week}/work-items/{request.CollaboratorId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.CollaboratorId.ShouldNotBe(Guid.Empty);
            result.ProformaId.ShouldNotBe(Guid.Empty);
            result.Week.ShouldNotBe(0);
        });

        return (request, result);
    }

    public async Task<(GetProformaWeek.Query, GetProformaWeek.Result?)> GetWeek(Action<GetProformaWeek.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetProformaWeek.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetProformaWeek.Query, GetProformaWeek.Result>($"{_uri}/{request.ProformaId}/weeks/{request.Week}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.ProformaId.ShouldNotBe(Guid.Empty);
            result.Week.ShouldNotBe(0);
        });

        return (request, result);
    }

    public async Task<(GetProforma.Query, GetProforma.Result?)> Get(Action<GetProforma.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetProforma.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetProforma.Query, GetProforma.Result>($"{_uri}/{request.ProformaId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.ProformaId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }

    public async Task WorkItemShouldHaveRightAmounts(Guid proformaId, int week, Guid collaboratorId, decimal subtotal, decimal profitAmount)
    {
        var (_, workItem) = await GetWorkItem(q =>
        {
            q.ProformaId = proformaId;
            q.Week = week;
            q.CollaboratorId = collaboratorId;
        });

        workItem!.SubTotal.ShouldBe(subtotal);

        workItem!.ProfitAmount.ShouldBe(profitAmount);
    }

    public async Task WeekShouldHaveRightAmounts(Guid proformaId, int week, decimal penalty, decimal subtotal)
    {
        var (_, weekItem) = await GetWeek(q =>
        {
            q.ProformaId = proformaId;
            q.Week = week;
        });

        weekItem!.Penalty.ShouldBe(penalty);

        weekItem!.SubTotal.ShouldBe(subtotal);
    }

    public async Task ShouldHaveRightAmounts(Guid proformaId,
        decimal subTotal,
        decimal taxesExpensesAmount,
        decimal administrativeExpensesAmount,
        decimal bankingExpensesAmount,
        decimal commission,
        decimal total)
    {
        var (_, proforma) = await Get(q =>
        {
            q.ProformaId = proformaId;
        });

        proforma!.SubTotal.ShouldBe(subTotal);

        proforma.TaxesExpensesAmount.ShouldBe(taxesExpensesAmount);

        proforma.AdministrativeExpensesAmount.ShouldBe(administrativeExpensesAmount);

        proforma.BankingExpensesAmount.ShouldBe(bankingExpensesAmount);

        proforma.Commission.ShouldBe(commission);

        proforma.Total.ShouldBe(total);
    }

    public async Task ShouldBe(Guid proformaId, ProformaStatus status)
    {
        var (_, proforma) = await Get(q =>
        {
            q.ProformaId = proformaId;
        });

        proforma!.Status.ShouldBe(status.ToString());
    }
}