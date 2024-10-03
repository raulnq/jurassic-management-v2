using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.CollaboratorRoles;

public class CollaboratorRole
{
    public Guid CollaboratorRoleId { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal FeeAmount { get; private set; }
    public decimal ProfitPercentage { get; private set; }

    private CollaboratorRole() { }

    public CollaboratorRole(Guid collaboratorRoleId, string name, decimal feeAmount, decimal profitPercentage)
    {
        CollaboratorRoleId = collaboratorRoleId;
        Name = name;
        FeeAmount = feeAmount;
        ProfitPercentage = profitPercentage;
    }

    public void Edit(string name, decimal feeAmount, decimal profitPercentage)
    {
        Name = name;
        FeeAmount = feeAmount;
        ProfitPercentage = profitPercentage;
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<CollaboratorRole>
{
    public void Configure(EntityTypeBuilder<CollaboratorRole> builder)
    {
        builder
            .ToTable(Tables.CollaboratorRoles);

        builder
            .HasKey(cr => cr.CollaboratorRoleId);

        builder
            .Property(c => c.ProfitPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FeeAmount)
            .HasColumnType("decimal(19, 4)");
    }
}