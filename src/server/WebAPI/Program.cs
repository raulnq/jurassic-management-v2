using Coravel;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using WebAPI.BankBalance;
using WebAPI.ClientContacts;
using WebAPI.Clients;
using WebAPI.CollaboratorBalance;
using WebAPI.CollaboratorPayments;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Collections;
using WebAPI.Companies;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.OpenApi;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Invoices;
using WebAPI.InvoiceToCollectionProcesses;
using WebAPI.JiraProfiles;
using WebAPI.MoneyExchanges;
using WebAPI.PayrollPayments;
using WebAPI.ProformaDocuments;
using WebAPI.Proformas;
using WebAPI.ProformaToCollaboratorPaymentProcesses;
using WebAPI.ProformaToInvoiceProcesses;
using WebAPI.Projects;
using WebAPI.ScheduleTasks;
using WebAPI.TaxPayments;
using WebAPI.Transactions;
using WebAPI.Users;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DescribeAllParametersInCamelCase();
    options.CustomSchemaIds(CustomSchemaIdProvider.Get);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.MaxAge = options.ExpireTimeSpan;
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents()
        {
            OnRedirectToLogin = (options) =>
            {
                options.HttpContext.Response.Headers.Append("HX-Redirect", WebAPI.Users.Endpoints.Login);
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();
builder.Services.AddEntityFramework(builder.Configuration);
builder.Services.AddSqlKata(builder.Configuration);
builder.Services.AddRebus(builder.Configuration, onCreated: bus =>
{
    return bus.SubscribeToProforma();
});
builder.Services.AddProblemDetails();

builder.Services.AddInvoices(builder.Configuration);
builder.Services.AddCollaboratorPayments(builder.Configuration);
builder.Services.AddPayrollPayments(builder.Configuration);
builder.Services.AddProformaDocuments(builder.Configuration);
builder.Services.AddTransactions(builder.Configuration);
builder.Services.AddMoneyExchanges(builder.Configuration);
builder.Services.AddCollections(builder.Configuration);
builder.Services.AddTaxPayments(builder.Configuration);
builder.Services.AddUsers();
builder.Services.AddCompanies(builder.Configuration);
builder.Services.AddJiraProfiles(builder.Configuration);
builder.Services.AddSingleton<IClock>(new SystemClock());
builder.Services.AddExceptionHandler<DefaultExceptionHandler>();
builder.Services.AddRazorComponents();
builder.Services.AddScheduler();
builder.Services.AddTransient<SendMonthlyDocuments>();
var app = builder.Build();

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<SendMonthlyDocuments>()
    .Cron(SendMonthlyDocuments.CRON);
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.RegisterCollaboratorRoleEndpoints();
app.RegisterCollaboratorEndpoints();
app.RegisterProjectEndpoints();
app.RegisterClientEndpoints();
app.RegisterProformaEndpoints();
app.RegisterProformaToInvoiceProcessEndpoints();
app.RegisterInvoiceEndpoints();
app.RegisterInvoiceToCollectionProcessEndpoints();
app.RegisterCollectionEndpoints();
app.RegisterProformaToColaboratorPaymentProcessEndpoints();
app.RegisterCollaboratorPaymentEndpoints();
app.RegisterPayrollPaymentEndpoints();
app.RegisterMoneyExchangeEndpoints();
app.RegisterTransactionEndpoints();
app.RegisterUserEndpoints();
app.RegisterBankBalanceEndpoints();
app.RegisterCollaboratorBalanceEndpoints();
app.RegisterClientContactEndpoints();
app.RegisterTaxPaymentEndpoints();
app.Run();