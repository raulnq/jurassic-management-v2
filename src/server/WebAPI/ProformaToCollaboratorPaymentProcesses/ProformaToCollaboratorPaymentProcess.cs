using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Proformas;
using Infrastructure;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public class ProformaToCollaboratorPaymentProcess
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public List<ProformaToCollaboratorPaymentProcessItem> Items { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Currency Currency { get; private set; }
    private ProformaToCollaboratorPaymentProcess() { Items = []; }

    public ProformaToCollaboratorPaymentProcess(Guid collaboratorPaymentId, Guid collaboratorId, Currency currency, IEnumerable<Proforma> proformas, DateTimeOffset createdAt)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        CreatedAt = createdAt;
        CollaboratorId = collaboratorId;
        Items = [];
        Currency = currency;
        foreach (var proforma in proformas)
        {
            if (proforma.Status != ProformaStatus.Issued)
            {
                throw new DomainException("proforma-is-issued");
            }

            foreach (var week in proforma.Weeks)
            {
                foreach (var item in week.WorkItems.Where(w => w.CollaboratorId == collaboratorId))
                {
                    Items.Add(new ProformaToCollaboratorPaymentProcessItem(CollaboratorPaymentId, item.ProformaId, item.Week, item.CollaboratorId));
                }
            }
        }
    }
}

public class ProformaToCollaboratorPaymentProcessItem
{
    public Guid CollaboratorPaymentId { get; private set; }
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public Guid CollaboratorId { get; private set; }
    private ProformaToCollaboratorPaymentProcessItem()
    {

    }

    public ProformaToCollaboratorPaymentProcessItem(Guid collaboratorPaymentId, Guid proformaId, int week, Guid collaboratorId)
    {
        CollaboratorPaymentId = collaboratorPaymentId;
        ProformaId = proformaId;
        Week = week;
        CollaboratorId = collaboratorId;
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaToCollaboratorPaymentProcess>
{
    public void Configure(EntityTypeBuilder<ProformaToCollaboratorPaymentProcess> builder)
    {
        builder
            .ToTable(Tables.ProformaToCollaboratorPaymentProcesses);

        builder
            .HasKey(field => field.CollaboratorPaymentId);

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(p => p.CollaboratorPaymentId);
    }
}

public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaToCollaboratorPaymentProcessItem>
{
    public void Configure(EntityTypeBuilder<ProformaToCollaboratorPaymentProcessItem> builder)
    {
        builder
            .ToTable(Tables.ProformaToCollaboratorPaymentProcessItems);

        builder
            .HasKey(i => new { i.CollaboratorPaymentId, i.ProformaId, i.Week, i.CollaboratorId });
    }
}