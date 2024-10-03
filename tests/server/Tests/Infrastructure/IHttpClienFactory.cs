namespace Tests.Infrastructure;

public interface IHttpClienFactory
{
    HttpClient CreateClient();
}