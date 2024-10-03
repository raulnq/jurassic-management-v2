using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;

namespace WebAPI.ProformaDocuments;

public static class GetProformaDocument
{
    public class Query
    {
        public Guid ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public string Url { get; set; } = default!;
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid proformaId)
    {
        var result = await runner.GetOrDefault<Result>((qf) => qf
                .Query(Tables.ProformaDocuments)
                .Where(Tables.ProformaDocuments.Field(nameof(Proforma.ProformaId)), proformaId));

        return TypedResults.Ok(result);
    }
}
