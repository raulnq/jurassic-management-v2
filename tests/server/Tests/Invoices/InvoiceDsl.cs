
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Net.Mime;
using Tests.Infrastructure;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Invoices;

namespace Tests.Invoices;

public class InvoiceDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "invoices";
    public InvoiceDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<UploadDocument.Command?> Upload(string file, Action<UploadDocument.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<UploadDocument.Command>()
           ;
        var request = faker.Generate();

        setup?.Invoke(request);

        using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var fileName = Path.GetFileName(file);

            var (status, result, error) = await _httpDriver.Post<EmptyResult>($"{_uri}/{request.InvoiceId}/upload-document", fs, fileName, MediaTypeNames.Application.Pdf);

            (status, error).Check(errorDetail, errors: errors);

            return request;
        }
    }

    public async Task<IssueInvoice.Command> Issue(Guid invoiceId, Action<IssueInvoice.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<IssueInvoice.Command>()
             .RuleFor(command => command.Number, faker => faker.Random.Guid().ToString())
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{invoiceId}/issue", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<CancelInvoice.Command> Cancel(Guid invoiceId, Action<CancelInvoice.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<CancelInvoice.Command>();
        ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{invoiceId}/cancel", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<(ListInvoices.Query, ListResults<ListInvoices.Result>?)> List(Action<ListInvoices.Query>? setup = null, string? errorDetail = null)
    {
        var faker = new Faker<ListInvoices.Query>();

        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<ListInvoices.Query, ListResults<ListInvoices.Result>>(_uri, request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.TotalCount.ShouldBeGreaterThan(0);
        });

        return (request, result);
    }

    public async Task<(GetInvoice.Query, GetInvoice.Result?)> Get(Action<GetInvoice.Query>? setup = null, string? errorDetail = null)
    {
        var request = new GetInvoice.Query();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Get<GetInvoice.Query, GetInvoice.Result>($"{_uri}/{request.InvoiceId}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.InvoiceId.ShouldNotBe(Guid.Empty);
        });

        return (request, result);
    }
}