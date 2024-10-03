using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;

namespace WebAPI.PayrollPayments;

public enum PayrollPaymentStatus
{
    Pending = 0,
    Paid = 1,
    AfpPaid = 2,
    Canceled = 3
}

public class PayrollPayment
{
    public const decimal TAX = 0.09m;
    public const decimal MINIMALSALARY = 1025m;
    public Guid PayrollPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? AfpPaidAt { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }
    public decimal Afp { get; private set; }
    public decimal Commission { get; private set; }
    public PayrollPaymentStatus Status { get; private set; }
    public string? DocumentUrl { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public Guid? MoneyExchangeId { get; private set; }
    public Guid? TaxPaymentId { get; private set; }
    public bool ExcludeFromTaxes { get; private set; }

    private PayrollPayment() { }

    public PayrollPayment(Guid payrollPaymentId, Guid collaboratoId, decimal netSalary, decimal afp, decimal commission, Currency currency, DateTimeOffset createdAt, Guid? moneyExchangeId)
    {
        PayrollPaymentId = payrollPaymentId;
        CollaboratorId = collaboratoId;
        NetSalary = netSalary;
        Afp = afp;
        GrossSalary = netSalary + afp;
        CreatedAt = createdAt;
        Status = PayrollPaymentStatus.Pending;
        Currency = currency;
        Commission = commission;
        Refresh();
        MoneyExchangeId = moneyExchangeId;
        ExcludeFromTaxes = false;
    }

    public void AddInTaxPayment(Guid taxPaymentId)
    {
        TaxPaymentId = taxPaymentId;
    }

    public void RemoveFromTaxPayment()
    {
        TaxPaymentId = null;
    }

    //La tasa aplicable es el 9% sobre tu remuneración. La remuneración mínima asegurable mensual no podrá ser inferior a la Remuneración Mínima Vital (1025).
    public decimal GetTaxes()
    {
        if (ExcludeFromTaxes)
        {
            return 0m;
        }
        if (GrossSalary < MINIMALSALARY)
        {
            return MINIMALSALARY * TAX;
        }

        return GrossSalary * TAX;
    }


    public void Edit(decimal netSalary, Currency currency, decimal afp, decimal commission, Guid? moneyExchangeId)
    {
        NetSalary = netSalary;
        Afp = afp;
        GrossSalary = netSalary + afp;
        Commission = commission;
        Currency = currency;
        MoneyExchangeId = moneyExchangeId;
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
        EnsureStatus(PayrollPaymentStatus.Pending);
        Status = PayrollPaymentStatus.Paid;
        PaidAt = paidAt;
    }

    public void PayAfp(DateTime paidAt, decimal afp)
    {
        EnsureStatus(PayrollPaymentStatus.Paid);
        Status = PayrollPaymentStatus.AfpPaid;
        Afp = afp;
        GrossSalary = NetSalary + afp;
        AfpPaidAt = paidAt;
    }

    public void Upload(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(PayrollPaymentStatus.Pending);
        Status = PayrollPaymentStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void Exclude()
    {
        ExcludeFromTaxes = true;
    }

    public void Include()
    {
        ExcludeFromTaxes = false;
    }

    public void EnsureStatus(PayrollPaymentStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"payroll-payment-status-not-{status.ToString().ToLower()}");
        }
    }
}
