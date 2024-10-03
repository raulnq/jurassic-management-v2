
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Net.Mime;
using Tests.Infrastructure;
using WebAPI.PayrollPayments;

namespace Tests.PayrollPayments;

public class PayrollPaymentDsl
{
    private readonly HttpDriver _httpDriver;
    private static readonly string _uri = "payroll-payments";
    public PayrollPaymentDsl(HttpDriver httpDriver)
    {
        _httpDriver = httpDriver;
    }

    public async Task<UploadDocument.Command?> Upload(string file, Guid payrollPaymentId, Action<UploadDocument.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<UploadDocument.Command>()
           ;
        var request = faker.Generate();

        setup?.Invoke(request);

        using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var fileName = Path.GetFileName(file);

            var (status, result, error) = await _httpDriver.Post<EmptyResult>($"{_uri}/{payrollPaymentId}/upload-document", fs, fileName, MediaTypeNames.Application.Pdf);

            (status, error).Check(errorDetail, errors: errors);

            return request;
        }
    }

    public async Task<(RegisterPayrollPayment.Command, RegisterPayrollPayment.Result)> Register(Action<RegisterPayrollPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<RegisterPayrollPayment.Command>()
            .RuleFor(command => command.NetSalary, faker => faker.Random.Number(100, 1000))
            .RuleFor(command => command.Afp, faker => faker.Random.Number(1, 100))
            .RuleFor(command => command.Commission, faker => faker.Random.Number(0, 100))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, result, error) = await _httpDriver.Post<RegisterPayrollPayment.Command, RegisterPayrollPayment.Result>($"{_uri}", request);

        (status, result, error).Check(errorDetail, successAssert: result =>
        {
            result.PayrollPaymentId.ShouldNotBe(Guid.Empty);
        });

        return (request, result!);
    }

    public async Task<PayPayrollPayment.Command> Pay(Guid payrollPaymentId, Action<PayPayrollPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<PayPayrollPayment.Command>()
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Post($"{_uri}/{payrollPaymentId}/pay", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }

    public async Task<EditPayrollPayment.Command> Edit(Guid payrollPaymentId, Action<EditPayrollPayment.Command>? setup = null, string? errorDetail = null, IDictionary<string, string[]>? errors = null)
    {
        var faker = new Faker<EditPayrollPayment.Command>()
            .RuleFor(command => command.NetSalary, faker => faker.Random.Number(100, 1000))
            .RuleFor(command => command.Afp, faker => faker.Random.Number(1, 100))
            .RuleFor(command => command.Commission, faker => faker.Random.Number(0, 100))
            ;
        var request = faker.Generate();

        setup?.Invoke(request);

        var (status, error) = await _httpDriver.Put($"{_uri}/{payrollPaymentId}", request);

        (status, error).Check(errorDetail, errors: errors);

        return request;
    }
}