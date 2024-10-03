using System.Net.Http.Headers;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.JiraProfiles;

public class TempoService
{
    public class Request
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ProjectId { get; set; } = default!;
        public string Token { get; set; } = default!;
    }

    public class Response
    {
        public IEnumerable<Result> Results { get; set; } = default!;
    }

    public class Result
    {
        public int BillableSeconds { get; set; }
        public DateTime StartDate { get; set; }
        public Author Author { get; set; } = default!;
    }

    public class Author
    {
        public string AccountId { get; set; } = default!;
    }

    public class Settings
    {
        public string Uri { get; set; } = default!;
        public TimeSpan Timeout { get; set; } = default!;
    }

    public readonly HttpClient _httpClient;

    public TempoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Response> Get(Request request)
    {
        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get,
            $"/4/worklogs?projectId={request.ProjectId}&from={request.Start:yyyy-MM-dd}&to={request.End:yyyy-MM-dd}&limit=100"))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

            var httpResponse = await _httpClient.SendAsync(requestMessage);

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.Content != null)
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();

                    throw new InfrastructureException($"{httpResponse.StatusCode} {content}");
                }

                throw new InfrastructureException(httpResponse.StatusCode.ToString());
            }

            var response = await httpResponse.Content!.ReadFromJsonAsync<Response>();

            return response!;
        }
    }
}