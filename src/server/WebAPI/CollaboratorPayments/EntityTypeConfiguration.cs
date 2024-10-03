using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.CollaboratorPayments;

public class EntityTypeConfiguration : IEntityTypeConfiguration<CollaboratorPayment>
{
    public void Configure(EntityTypeBuilder<CollaboratorPayment> builder)
    {
        builder
            .ToTable(Tables.CollaboratorPayments);

        builder
            .HasKey(c => c.CollaboratorPaymentId);

        builder
            .Property(c => c.NetSalary)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.GrossSalary)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Withholding)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ExchangeRate)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<CollaboratorPaymentStatus>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}