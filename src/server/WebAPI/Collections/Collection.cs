using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.Collections;

public enum CollectionStatus
{
    Pending = 0,
    Confirmed = 1,
    Canceled = 2
}

public class Collection
{
    public Guid CollectionId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public decimal Total { get; private set; }
    public string? Number { get; private set; }
    public decimal Commission { get; private set; }
    public CollectionStatus Status { get; private set; }
    public decimal ITF { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public string? DocumentUrl { get; private set; }

    private Collection() { }

    public Collection(Guid collectionId, Guid clientId, decimal total, Currency currency, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        Total = total;
        CreatedAt = createdAt;
        Status = CollectionStatus.Pending;
        Currency = currency;
        ClientId = clientId;
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

    public void Confirm(decimal total, decimal commission, string number, DateTime confirmedAt)
    {
        EnsureDocument();
        EnsureStatus(CollectionStatus.Pending);
        Total = total;
        Number = number;
        Commission = commission;
        Status = CollectionStatus.Confirmed;
        ConfirmedAt = confirmedAt;
        Refresh();
    }

    public void EnsureDocument()
    {
        if (string.IsNullOrEmpty(DocumentUrl))
        {
            throw new DomainException("collection-document-not-uploaded");
        }
    }

    public void EnsureStatus(CollectionStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"collection-status-not-{status.ToString().ToLower()}");
        }
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(CollectionStatus.Pending);
        Status = CollectionStatus.Canceled;
        CanceledAt = canceledAt;
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder
            .ToTable(Tables.Collections);

        builder
            .HasKey(c => c.CollectionId);

        builder
            .Property(c => c.Total)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<CollectionStatus>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}