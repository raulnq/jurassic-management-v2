namespace WebAPI.ScheduleTasks;

public static class Endpoints
{
    public const string Title = "Schedule tasks";

    public const string List = "/ui/schedule-tasks/list";

    public const string ListTitle = "List schedule tasks";

    public const string TriggerMonthlyDocuments = "/ui/schedule-tasks/monthly-documents";

    public const string TriggerMonthlyDocumentsTitle = "Trigger monthly documents";


    public static void RegisterScheduleTasksEndpoints(this WebApplication app)
    {
        var uigroup = app.MapGroup("/ui/schedule-tasks")
        .ExcludeFromDescription()
        .RequireAuthorization();

        uigroup.MapGet("/list", ListScheduleTasks.HandlePage);

        uigroup.MapPost("/monthly-documents", SendMonthlyDocuments.Trigger);
    }
}
