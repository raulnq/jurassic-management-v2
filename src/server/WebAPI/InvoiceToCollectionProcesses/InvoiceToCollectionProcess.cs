using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Invoices;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.InvoiceToCollectionProcesses;

public class InvoiceToCollectionProcess
{
    public Guid CollectionId { get; private set; }
    public List<InvoiceToCollectionProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid ClientId { get; private set; }
    public Currency Currency { get; private set; }
    private InvoiceToCollectionProcess() { Items = []; }

    public InvoiceToCollectionProcess(Guid collectionId, Guid clientId, Currency currency, IEnumerable<Invoice> invoices, DateTimeOffset createdAt)
    {
        CollectionId = collectionId;
        ClientId = clientId;
        Currency = currency;
        CreatedAt = createdAt;
        Items = [];
        foreach (var invoice in invoices)
        {
            if (invoice.Status != InvoiceStatus.Issued)
            {
                throw new DomainException("invoice-is-not-issued");
            }
            Items.Add(new InvoiceToCollectionProcessItem(invoice.InvoiceId, CollectionId));
        }
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<InvoiceToCollectionProcess>
{
    public void Configure(EntityTypeBuilder<InvoiceToCollectionProcess> builder)
    {
        builder
            .ToTable(Tables.InvoiceToCollectionProcesses);

        builder
            .HasKey(field => field.CollectionId);

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(p => p.CollectionId);
    }
}

public class InvoiceToCollectionProcessItem
{
    public Guid InvoiceId { get; private set; }
    public Guid CollectionId { get; private set; }
    private InvoiceToCollectionProcessItem()
    {

    }

    public InvoiceToCollectionProcessItem(Guid invoiceId, Guid collectionId)
    {
        InvoiceId = invoiceId;
        CollectionId = collectionId;
    }
}

public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<InvoiceToCollectionProcessItem>
{
    public void Configure(EntityTypeBuilder<InvoiceToCollectionProcessItem> builder)
    {
        builder
            .ToTable(Tables.InvoiceToCollectionProcessItems);

        builder
            .HasKey(i => new { i.CollectionId, i.InvoiceId });
    }
}