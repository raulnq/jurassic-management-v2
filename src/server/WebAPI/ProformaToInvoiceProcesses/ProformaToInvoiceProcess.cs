using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.ProformaToInvoiceProcesses;

public class ProformaToInvoiceProcess
{
    public Guid InvoiceId { get; private set; }
    public Guid ClientId { get; private set; }
    public Currency Currency { get; private set; }
    public List<ProformaToInvoiceProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ProformaToInvoiceProcess() { Items = []; }

    public ProformaToInvoiceProcess(Guid invoiceId, Guid clientId, Currency currency, IEnumerable<Proforma> proformas, DateTimeOffset createdAt)
    {
        InvoiceId = invoiceId;
        CreatedAt = createdAt;
        ClientId = clientId;
        Currency = currency;
        Items = [];
        foreach (var proforma in proformas)
        {
            if (proforma.Status != ProformaStatus.Issued)
            {
                throw new DomainException("proforma-is-not-issued");
            }
            Items.Add(new ProformaToInvoiceProcessItem(InvoiceId, proforma.ProformaId));
        }
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaToInvoiceProcess>
{
    public void Configure(EntityTypeBuilder<ProformaToInvoiceProcess> builder)
    {
        builder
            .ToTable(Tables.ProformaToInvoiceProcesses);

        builder
            .HasKey(field => field.InvoiceId);

        builder
        .Property(c => c.Currency)
        .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(pti => pti.InvoiceId);
    }
}

public class ProformaToInvoiceProcessItem
{
    public Guid InvoiceId { get; private set; }
    public Guid ProformaId { get; private set; }
    private ProformaToInvoiceProcessItem()
    {

    }

    public ProformaToInvoiceProcessItem(Guid invoiceId, Guid proformaId)
    {
        InvoiceId = invoiceId;
        ProformaId = proformaId;
    }
}


public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaToInvoiceProcessItem>
{
    public void Configure(EntityTypeBuilder<ProformaToInvoiceProcessItem> builder)
    {
        builder
            .ToTable(Tables.ProformaToInvoiceProcessItems);

        builder
            .HasKey(field => new { field.InvoiceId, field.ProformaId });
    }
}