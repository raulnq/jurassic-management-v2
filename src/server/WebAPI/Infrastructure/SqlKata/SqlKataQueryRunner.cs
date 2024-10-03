using SqlKata.Execution;
using WebAPI.Infrastructure.ExceptionHandling;
using SqlKata;

namespace WebAPI.Infrastructure.SqlKata;

public class SqlKataQueryRunner
{
    private readonly QueryFactory _queryFactory;

    public SqlKataQueryRunner(QueryFactory queryFactory)
    {
        _queryFactory = queryFactory;
    }

    public async Task<TResult> Get<TResult>(Func<QueryFactory, Query> statementBuilder)
    {
        var result = await statementBuilder(_queryFactory).FirstOrDefaultAsync<TResult>();

        if (result == null)
        {
            throw new NotFoundException();
        }
        return result;
    }

    public async Task<TResult> GetOrDefault<TResult>(Func<QueryFactory, Query> statementBuilder)
    {
        var result = await statementBuilder(_queryFactory).FirstOrDefaultAsync<TResult>();

        return result;
    }

    public async Task<ListResults<TResult>> List<TQuery, TResult>(Func<QueryFactory, Query> statementBuilder, TQuery query)
        where TQuery : ListQuery
    {
        var statement = statementBuilder(_queryFactory);

        int count = await Count(statement);

        if (query.OrderBy?.Length > 0)
        {
            if (query.Ascending)
            {
                statement = statement.OrderBy(query.OrderBy);
            }
            else
            {
                statement = statement.OrderByDesc(query.OrderBy);
            }
        }

        var result = await statement.ForPage(query.Page, query.PageSize).GetAsync<TResult>();

        return new ListResults<TResult>(query, count, result);
    }

    public async Task<List<TResult>> List<TResult>(Func<QueryFactory, Query> statementBuilder)
    {
        var statement = statementBuilder(_queryFactory);

        var result = await statement.GetAsync<TResult>();

        return result.ToList();
    }

    public Task<bool> Any(Func<QueryFactory, Query> statementBuilder)
    {
        var statement = statementBuilder(_queryFactory);

        return statement.ExistsAsync();
    }

    public Task<int> Count(Func<QueryFactory, Query> statementBuilder)
    {
        var statement = statementBuilder(_queryFactory);

        return statement.CountAsync<int>();
    }

    private Task<int> Count(Query statement)
    {
        return statement.Clone().CountAsync<int>();
    }
}

public class ListResults<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; }
    public bool HasPreviousPage
    {
        get
        {
            return Page > 1;
        }
    }

    public bool HasNextPage
    {
        get
        {
            return Page < TotalPages;
        }
    }

    public ListResults()
    {

    }

    public ListResults(ListQuery query, int totalCount, IEnumerable<T> source)
    {
        Page = query.Page;
        TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        PageSize = query.PageSize;
        TotalCount = totalCount;
        Items = source;
    }
}

public class ListQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string[]? OrderBy { get; set; }

    public bool Ascending { get; set; } = true;
}