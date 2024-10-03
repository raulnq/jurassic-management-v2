using WebAPI.ProformaDocuments;

namespace WebAPI.Proformas;

public static class Endpoints
{
    public const string Title = "Proformas";

    public const string WeekTitle = "Weeks";

    public const string WorkItemTitle = "Work Items";

    public const string List = "/ui/proformas/list";

    public const string ListTitle = "List proformas";

    public const string Register = "/ui/proformas/register";

    public const string RegisterTitle = "Register proformas";

    public const string View = "/ui/proformas/{proformaId}/view";

    public const string ViewTitle = "View proforma";

    public const string Issue = "/ui/proformas/{proformaId}/issue";

    public const string IssueTitle = "Issue";

    public const string Cancel = "/ui/proformas/{proformaId}/cancel";

    public const string Open = "/ui/proformas/{proformaId}/open";

    public const string Send = "/ui/proformas/{proformaId}/send";

    public const string TrackingTitle = "Tracking";

    public const string Track = "/ui/proformas/{proformaId}/tracking";

    public const string ListWeeks = "/ui/proformas/{proformaId}/weeks/list";

    public const string AddWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/add";

    public const string LoadWorkItemsTitle = "Load work items";

    public const string LoadWorkItems = "/ui/proformas/{proformaId}/weeks/{week}/work-items/load";

    public const string AddWorkItemTitle = "Add work item";

    public const string EditWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/edit";

    public const string EditWorkItemTitle = "Edit work item";

    public const string RemoveWorkItem = "/ui/proformas/{proformaId}/weeks/{week}/work-items/{collaboratorId}/remove";

    public const string ListWorkItems = "/ui/proformas/{proformaId}/weeks/{week}/work-items/list";

    public const string SearchNotAddedToInvoice = "/ui/proformas/search-not-added-to-invoice";

    public const string SearchNotAddedToCollaboratorPayment = "/ui/proformas/search-not-added-to-collaborator-payment";

    public static string GetLoadWorkItems(Guid proformaId, int week)
    {
        return LoadWorkItems.Replace("{proformaId}", proformaId.ToString()).Replace("{week}", week.ToString());
    }

    public static void RegisterProformaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/proformas")
        .WithTags("proformas");

        group.MapGet("/", ListProformas.Handle);

        group.MapGet("/{proformaId:guid}/weeks", ListProformaWeeks.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items", ListProformaWeekWorkItems.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", GetProformaWeekWorkItem.Handle);

        group.MapGet("/{proformaId:guid}/weeks/{week:int}", GetProformaWeek.Handle);

        group.MapGet("/{proformaId:guid}", GetProforma.Handle);

        group.MapPost("/", RegisterProforma.Handle);

        group.MapPost("/{proformaId:guid}/issue", IssueProforma.Handle);

        group.MapPost("/{proformaId:guid}/cancel", CancelProforma.Handle);

        group.MapPost("/{proformaId:guid}/open", OpenProforma.Handle);

        group.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items", Proformas.AddWorkItem.Handle);

        group.MapPut("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.EditWorkItem.Handle);

        group.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}", Proformas.RemoveWorkItem.Handle);

        var uigroup = app.MapGroup("/ui/proformas")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListProformas.HandlePage);

        uigroup.MapGet("/register", RegisterProforma.HandlePage);

        uigroup.MapPost("/register", RegisterProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/view", GetProforma.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/issue", IssueProforma.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/issue", IssueProforma.HandleAction);

        uigroup.MapPost("/{proformaId:guid}/cancel", CancelProforma.HandleAction);

        uigroup.MapPost("/{proformaId:guid}/send", SendProformaDcument.HandleAction);

        uigroup.MapPost("/{proformaId:guid}/open", OpenProforma.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/tracking", GetProformaTracking.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/list", ListProformaWeeks.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/list", ListProformaWeekWorkItems.HandlePage);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/add", Proformas.AddWorkItem.HandleAction);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/load", JiraProfiles.LoadJiraWorklogs.HandleAction);

        uigroup.MapGet("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandlePage);

        uigroup.MapPost("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/edit", Proformas.EditWorkItem.HandleAction);

        uigroup.MapDelete("/{proformaId:guid}/weeks/{week:int}/work-items/{collaboratorId:guid}/remove", Proformas.RemoveWorkItem.HandleAction);

        uigroup.MapGet("/search-not-added-to-invoice", SearchProformasNotAddedToInvoice.HandlePage);

        uigroup.MapGet("/search-not-added-to-collaborator-payment", SearchProformasNotAddedToCollaboratorPayment.HandlePage);
    }
}