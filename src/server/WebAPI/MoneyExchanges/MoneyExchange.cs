using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.MoneyExchanges;

public class MoneyExchange
{
    public Guid MoneyExchangeId { get; private set; }
    public decimal Rate { get; private set; }
    public decimal ToAmount { get; private set; }
    public decimal FromAmount { get; private set; }
    public decimal ToITF { get; private set; }
    public decimal FromITF { get; private set; }
    public Currency ToCurrency { get; private set; }
    public Currency FromCurrency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public string? DocumentUrl { get; private set; }

    private MoneyExchange() { }

    public MoneyExchange(Guid moneyExchangeId,
        Currency fromCurrency,
        decimal fromAmount,
        Currency toCurrency,
        decimal toAmount,
        decimal rate,
        DateTime issuedAt,
        DateTimeOffset createdAt)
    {
        Rate = rate;
        MoneyExchangeId = moneyExchangeId;
        FromCurrency = fromCurrency;
        FromAmount = fromAmount;
        ToCurrency = toCurrency;
        ToAmount = toAmount;
        IssuedAt = issuedAt;
        CreatedAt = createdAt;
        Refresh();
    }

    public void Edit(Currency fromCurrency,
        decimal fromAmount,
        Currency toCurrency,
        decimal toAmount,
        decimal rate,
        DateTime issuedAt)
    {
        Rate = rate;
        FromCurrency = fromCurrency;
        FromAmount = fromAmount;
        ToCurrency = toCurrency;
        ToAmount = toAmount;
        IssuedAt = issuedAt;
        Refresh();
    }

    public void UploadDocument(string documentUrl)
    {
        DocumentUrl = documentUrl;
    }

    private void Refresh()
    {
        if (FromAmount >= 1000)
        {
            FromITF = 0.00005m * FromAmount * 20m;
            FromITF = Math.Round(FromITF) / 20m;
        }
        else
        {
            FromITF = 0;
        }

        if (ToAmount >= 1000)
        {
            ToITF = 0.00005m * ToAmount * 20m;
            ToITF = Math.Round(ToITF) / 20m;
        }
        else
        {
            ToITF = 0;
        }
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<MoneyExchange>
{
    public void Configure(EntityTypeBuilder<MoneyExchange> builder)
    {
        builder
            .ToTable(Tables.MoneyExchanges);

        builder
            .HasKey(cr => cr.MoneyExchangeId);

        builder
            .Property(c => c.FromITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FromAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Rate)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ToITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ToAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ToCurrency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .Property(c => c.FromCurrency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}