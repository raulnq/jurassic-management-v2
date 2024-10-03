using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Proformas;

namespace WebAPI.PayrollPayments;

public class EntityTypeConfiguration : IEntityTypeConfiguration<PayrollPayment>
{
    public void Configure(EntityTypeBuilder<PayrollPayment> builder)
    {
        builder
            .ToTable(Tables.PayrollPayments);

        builder
            .HasKey(c => c.PayrollPaymentId);

        builder
            .Property(c => c.NetSalary)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.GrossSalary)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Commission)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ITF)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Afp)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<PayrollPaymentStatus>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());
    }
}