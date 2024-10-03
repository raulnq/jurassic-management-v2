using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.Transactions;

public enum TransactionType
{
    Incomes = 0,
    Expenses = 1
}

public class Transaction
{
    public Guid TransactionId { get; private set; }
    public string Description { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Taxes { get; private set; }
    public decimal Total { get; private set; }
    public decimal ITF { get; private set; }
    public string? Number { get; private set; }
    public string? DocumentUrl { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public TransactionType Type { get; private set; }

    private Transaction() { }

    public Transaction(Guid transactionId,
        TransactionType type,
        string description,
        decimal subTotal,
        decimal taxes,
        Currency currency,
        string? number,
        DateTime issuedAt,
        DateTimeOffset createdAt)
    {
        TransactionId = transactionId;
        Type = type;
        Description = description;
        SubTotal = subTotal;
        Taxes = taxes;
        Total = subTotal + taxes;
        CreatedAt = createdAt;
        Number = number;
        Currency = currency;
        IssuedAt = issuedAt;
        Refresh();
    }

    public void Edit(
        TransactionType type,
        string description,
        decimal subTotal,
        decimal taxes,
        Currency currency,
        string? number,
        DateTime issuedAt)
    {
        Type = type;
        Description = description;
        SubTotal = subTotal;
        Taxes = taxes;
        Total = subTotal + taxes;
        Number = number;
        Currency = currency;
        IssuedAt = issuedAt;
        Refresh();
    }

    public void UploadDocument(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    private void Refresh()
    {
        if (Total >= 1000)
        {
            ITF = 0.00005m * Total * 20m;
            ITF = Math.Round(ITF) / 20m;
        }
        else
        {
            ITF = 0;
        }
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .ToTable(Tables.Transactions);

        builder
            .HasKey(cr => cr.TransactionId);

        builder
            .Property(c => c.Total)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Taxes)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Type)
            .HasConversion(s => s.ToString(), value => value.ToEnum<TransactionType>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}