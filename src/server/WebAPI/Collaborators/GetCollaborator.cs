using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collaborators;

public static class GetCollaborator
{
    public class Query
    {
        public Guid CollaboratorId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
        public string? Name { get; set; }
        public decimal WithholdingPercentage { get; set; }
        public string Email { get; set; } = default!;
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid collaboratorId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.Collaborators)
                .Where(Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)), collaboratorId));

        return TypedResults.Ok(result);
    }
}
