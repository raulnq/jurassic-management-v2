using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.ScheduleTasks;

public static class ListScheduleTasks
{
    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<ListScheduleTasksPage>(new { }));
    }
}
