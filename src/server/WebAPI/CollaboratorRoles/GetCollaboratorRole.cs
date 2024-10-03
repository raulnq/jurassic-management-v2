using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorRoles;

public static class GetCollaboratorRole
{
    public class Query
    {
        public Guid CollaboratorRoleId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorRoleId { get; set; }
        public string? Name { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid collaboratorRoleId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.CollaboratorRoles)
                .Where(Tables.CollaboratorRoles.Field(nameof(CollaboratorRole.CollaboratorRoleId)), collaboratorRoleId));

        return TypedResults.Ok(result);
    }
}
