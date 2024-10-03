using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.CollaboratorPayments;

public enum CollaboratorPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Confirmed = 2,
    Canceled = 3
}

public class CollaboratorPayment
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }
    public decimal Withholding { get; private set; }
    public CollaboratorPaymentStatus Status { get; private set; }
    public string? DocumentUrl { get; private set; }
    public string? Number { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public Guid? TaxPaymentId { get; private set; }

    private CollaboratorPayment() { }

    public void AddInTaxPayment(Guid taxPaymentId)
    {
        TaxPaymentId = taxPaymentId;
    }

    public void RemoveFromTaxPayment()
    {
        TaxPaymentId = null;
    }

    public CollaboratorPayment(Guid collaboratorPaymentId, Guid collaboratoId, decimal grossSalary, decimal withholdingPercentage, Currency currency, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        CollaboratorId = collaboratoId;
        GrossSalary = grossSalary;
        Withholding = Math.Round((grossSalary * withholdingPercentage) / 100, 2, MidpointRounding.AwayFromZero);
        CreatedAt = createdAt;
        NetSalary = Math.Round(GrossSalary - Withholding, 2, MidpointRounding.AwayFromZero);
        Status = CollaboratorPaymentStatus.Pending;
        Currency = currency;
        ExchangeRate = 1;
        Refresh();
    }

    public void Edit(decimal grossSalary, Currency currency, decimal withholdingPercentage)
    {
        GrossSalary = grossSalary;
        Withholding = (grossSalary * withholdingPercentage) / 100;
        NetSalary = GrossSalary - Withholding;
        Currency = currency;
        Refresh();
    }
    private void Refresh()
    {
        if (NetSalary >= 1000)
        {
            ITF = 0.00005m * NetSalary * 20m;
            ITF = Math.Round(ITF) / 20m;
        }
        else
        {
            ITF = 0;
        }
    }
    public void Pay(DateTime paidAt)
    {
        EnsureStatus(CollaboratorPaymentStatus.Pending);
        Status = CollaboratorPaymentStatus.Paid;
        PaidAt = paidAt;
    }

    public void Upload(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public decimal GeTaxes()
    {
        return Withholding * ExchangeRate;
    }

    public void Confirm(DateTime confirmedAt, string number, decimal exchangeRate)
    {
        EnsureStatus(CollaboratorPaymentStatus.Paid);
        Number = number;
        ExchangeRate = exchangeRate;
        Status = CollaboratorPaymentStatus.Confirmed;
        ConfirmedAt = confirmedAt;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(CollaboratorPaymentStatus.Pending);
        Status = CollaboratorPaymentStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void EnsureStatus(CollaboratorPaymentStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"collaborator-payment-status-not-{status.ToString().ToLower()}");
        }
    }
}
