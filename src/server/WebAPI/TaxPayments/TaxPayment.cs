
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using Infrastructure;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.TaxPayments
{
    public enum TaxPaymentStatus
    {
        Pending = 0,
        Paid = 1,
    }

    public enum TaxType
    {
        ESSALUD = 0,
        REGIMENMYPE = 1,
        CUARTACATEGORIA = 2,
    }

    public class TaxPayment
    {
        public Guid TaxPaymentId { get; private set; }
        public decimal Total { get; private set; }
        public decimal ITF { get; private set; }
        public string? Month { get; private set; }
        public int Year { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public string? DocumentUrl { get; private set; }
        public TaxPaymentStatus Status { get; private set; }
        public List<TaxPaymentItem> Items { get; private set; }
        private TaxPayment() { Items = []; }

        public TaxPayment(Guid taxPaymentId, DateTimeOffset createdAt, string month, int year)
        {
            Items = [];
            TaxPaymentId = taxPaymentId;
            CreatedAt = createdAt;
            Status = TaxPaymentStatus.Pending;
            Total = 0;
            ITF = 0;
            Year = year;
            Month = month;
        }

        public void Upload(string documentUrl)
        {
            DocumentUrl = documentUrl;
        }

        public int Add(decimal amount, TaxType type)
        {
            var id = 1;

            if (Items.Any())
            {
                id = Items.Max(x => x.TaxPaymentItemId) + 1;
            }

            if (type == TaxType.ESSALUD)
            {
                amount = Math.Round(amount, 0);
            }
            else
            {
                amount = Math.Round(amount, 0, MidpointRounding.ToPositiveInfinity);
            }

            Items.Add(new TaxPaymentItem(TaxPaymentId, id, amount, type));

            Total = Total + amount;

            return id;
        }

        public TaxPaymentItem? Get(int taxPaymentItemId)
        {
            var item = Items.FirstOrDefault(i => i.TaxPaymentItemId == taxPaymentItemId);

            if (item != null)
            {
                return item;
            }

            return null;
        }

        public void Remove(int taxPaymentItemId)
        {
            var item = Items.FirstOrDefault(i => i.TaxPaymentItemId == taxPaymentItemId);

            if (item != null)
            {
                Items.Remove(item);

                Total = Total - item.Amount;
            }
        }

        public void Edit(int taxPaymentItemId, decimal amount)
        {
            var item = Items.FirstOrDefault(i => i.TaxPaymentItemId == taxPaymentItemId);

            if (item != null)
            {
                Total = Total - item.Amount;

                item.Edit(amount);

                Total = Total + item.Amount;
            }
        }

        public void EnsureStatus(TaxPaymentStatus status)
        {
            if (status != Status)
            {
                throw new DomainException($"tax-payment-status-not-{status.ToString().ToLower()}");
            }
        }

        public void Pay(DateTime paidAt)
        {
            EnsureStatus(TaxPaymentStatus.Pending);
            Status = TaxPaymentStatus.Paid;
            PaidAt = paidAt;
        }
    }

    public class TaxPaymentItem
    {
        public Guid TaxPaymentId { get; private set; }
        public int TaxPaymentItemId { get; private set; }
        public TaxType Type { get; private set; }
        public decimal Amount { get; private set; }

        public TaxPaymentItem(Guid taxPaymentId, int taxPaymentItemId, decimal amount, TaxType type)
        {
            TaxPaymentId = taxPaymentId;
            TaxPaymentItemId = taxPaymentItemId;
            Amount = amount;
            Type = type;
        }

        public void Edit(decimal amount)
        {
            Amount = amount;
        }
    }

    public class EntityTypeConfiguration : IEntityTypeConfiguration<TaxPayment>
    {
        public void Configure(EntityTypeBuilder<TaxPayment> builder)
        {
            builder
                .ToTable(Tables.TaxPayments);

            builder
                .HasKey(field => field.TaxPaymentId);

            builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<TaxPaymentStatus>());

            builder
                .Property(c => c.Total)
                .HasColumnType("decimal(19, 4)");

            builder
                .Property(c => c.ITF)
                .HasColumnType("decimal(19, 4)");

            builder
                .HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(pti => pti.TaxPaymentId);
        }
    }

    public class ItemEntityTypeConfiguration : IEntityTypeConfiguration<TaxPaymentItem>
    {
        public void Configure(EntityTypeBuilder<TaxPaymentItem> builder)
        {
            builder
                .ToTable(Tables.TaxPaymentItems);

            builder
                .HasKey(field => new { field.TaxPaymentId, field.TaxPaymentItemId });

            builder
            .Property(c => c.Type)
            .HasConversion(s => s.ToString(), value => value.ToEnum<TaxType>());

            builder
                .Property(c => c.Amount)
                .HasColumnType("decimal(19, 4)");
        }
    }
}
