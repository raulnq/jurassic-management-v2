using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using Infrastructure;

namespace WebAPI.Proformas;

public enum ProformaStatus
{
    Pending = 0,
    Issued = 1,
    Canceled = 2
}

public enum Currency
{
    PEN = 0,
    USD = 1
}

public class Proforma
{
    public Guid ProformaId { get; private set; }
    public Guid ProjectId { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }
    public string Number { get; private set; } = default!;
    public string? Note { get; private set; } = string.Empty;
    public List<ProformaWeek> Weeks { get; private set; }
    public decimal Total { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Commission { get; private set; }
    public decimal Discount { get; private set; }
    public decimal MinimumHours { get; private set; }
    public decimal PenaltyMinimumHours { get; private set; }
    public decimal TaxesExpensesPercentage { get; private set; }
    public decimal AdministrativeExpensesPercentage { get; private set; }
    public decimal BankingExpensesPercentage { get; private set; }
    public decimal MinimumBankingExpenses { get; private set; }
    public decimal TaxesExpensesAmount { get; private set; }
    public decimal AdministrativeExpensesAmount { get; private set; }
    public decimal BankingExpensesAmount { get; private set; }
    public ProformaStatus Status { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Currency Currency { get; private set; }

    private Proforma() { Weeks = []; }

    public Proforma(Guid proformaId, DateTime start, DateTime end, Guid projectId, Client client, DateTimeOffset createdAt, decimal discount, Currency currency, int count, string note)
    {
        ProformaId = proformaId;
        CreatedAt = createdAt;
        Status = ProformaStatus.Pending;
        Start = start.Date;
        End = end.Date;
        Number = $"{End.ToString("yyyyMMdd")}-{count + 1}";
        ProjectId = projectId;
        Currency = currency;
        Weeks = [];
        EnsureValidPeriod();
        MinimumHours = client.PenaltyMinimumHours;
        PenaltyMinimumHours = client.PenaltyAmount;
        TaxesExpensesPercentage = client.TaxesExpensesPercentage;
        AdministrativeExpensesPercentage = client.AdministrativeExpensesPercentage;
        BankingExpensesPercentage = client.BankingExpensesPercentage;
        MinimumBankingExpenses = client.MinimumBankingExpenses;
        Discount = discount;
        Note = note;
        FillWeeks();
        Refresh();
    }

    public bool CanAddWorkItems()
    {
        return Status == ProformaStatus.Pending;
    }

    public bool CanEditWorkItems()
    {
        return Status == ProformaStatus.Pending;
    }

    public bool CanRemoveWorkItems()
    {
        return Status == ProformaStatus.Pending;
    }

    private void EnsureValidPeriod()
    {
        if (End < Start)
        {
            throw new DomainException("proforma-invalid-period");
        }
    }

    private void FillWeeks()
    {
        var flag = true;
        var start = Start;
        var i = 1;
        do
        {
            var end = start.AddDays(6);

            if (end < End)
            {
                var week = new ProformaWeek(ProformaId, i, start, end, MinimumHours, PenaltyMinimumHours);
                Weeks.Add(week);
                start = end.AddDays(1);
                i++;
            }
            else
            {
                var week = new ProformaWeek(ProformaId, i, start, End, MinimumHours, PenaltyMinimumHours);
                Weeks.Add(week);
                flag = false;
            }

        } while (flag);
    }

    public void Issue(DateTime issuedAt)
    {
        EnsureTotalGreaterThenZero();
        //TODO: Validate empty weeks/work items?
        EnsureIssueAtGreaterOrEqualThanEnd(issuedAt);
        EnsureStatus(ProformaStatus.Pending);
        Status = ProformaStatus.Issued;
        IssuedAt = issuedAt;
    }

    public void Cancel(DateTimeOffset canceledAt)
    {
        EnsureStatus(ProformaStatus.Pending);
        Status = ProformaStatus.Canceled;
        CanceledAt = canceledAt;
    }

    public void Open()
    {
        EnsureStatus(ProformaStatus.Issued);
        Status = ProformaStatus.Pending;
        IssuedAt = null;
    }

    private void EnsureIssueAtGreaterOrEqualThanEnd(DateTime issuedAt)
    {
        if (End > issuedAt)
        {
            throw new DomainException("proforma-issue-date-lower-than-end-date");
        }
    }

    public void EnsureTotalGreaterThenZero()
    {
        if (Total <= 0)
        {
            throw new DomainException("proforma-total-lower-than-zero");
        }
    }

    public void EnsureStatus(ProformaStatus status)
    {
        if (status != Status)
        {
            throw new DomainException($"proforma-status-not-{status.ToString().ToLower()}");
        }
    }

    public void AddWorkItem(int week, Collaborator collaborator, CollaboratorRole collaboratorRole, decimal hours, decimal freeHours)
    {
        EnsureStatus(ProformaStatus.Pending);

        var weekItem = Weeks.FirstOrDefault(w => w.Week == week);

        if (weekItem == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        weekItem.Add(collaborator, collaboratorRole, hours, freeHours, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    public void EditWorkItem(int week, Guid collaboratorId, decimal hours, decimal freeHours)
    {
        EnsureStatus(ProformaStatus.Pending);

        var item = Weeks.FirstOrDefault(w => w.Week == week);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        item.Edit(collaboratorId, hours, freeHours, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    public void RemoveWorkItem(int week, Guid collaboratorId)
    {
        EnsureStatus(ProformaStatus.Pending);

        var item = Weeks.FirstOrDefault(w => w.Week == week);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeek>();
        }

        item.Remove(collaboratorId, MinimumHours, PenaltyMinimumHours);

        Refresh();
    }

    private void Refresh()
    {
        SubTotal = Weeks.Sum(i => i.SubTotal);

        TaxesExpensesAmount = TaxesExpensesPercentage * SubTotal / 100;

        AdministrativeExpensesAmount = AdministrativeExpensesPercentage * SubTotal / 100;

        BankingExpensesAmount = AdministrativeExpensesPercentage * SubTotal / 100;

        if (BankingExpensesAmount < MinimumBankingExpenses)
        {
            BankingExpensesAmount = MinimumBankingExpenses;
        }

        Commission = TaxesExpensesAmount + AdministrativeExpensesAmount + BankingExpensesAmount;

        Commission = Math.Round(Commission, 2, MidpointRounding.AwayFromZero);

        Total = SubTotal + Commission - Discount;
    }
}

public class ProformaWeek
{
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }
    public decimal Penalty { get; private set; }
    public decimal SubTotal { get; private set; }
    public List<ProformaWeekWorkItem> WorkItems { get; private set; }

    private ProformaWeek()
    {
        WorkItems = [];
    }

    public ProformaWeek(Guid platformId, int week, DateTime start, DateTime end, decimal minimumHours, decimal penaltyMinimumHours)
    {
        ProformaId = platformId;
        Week = week;
        Start = start;
        End = end;
        WorkItems = new List<ProformaWeekWorkItem>();
        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Add(Collaborator collaborator, CollaboratorRole collaboratorRole, decimal hours, decimal freeHours, decimal minimumHours, decimal penaltyMinimumHours)
    {
        if (WorkItems.Any(i => i.CollaboratorId == collaborator.CollaboratorId))
        {
            throw new DomainException(ExceptionCodes.Duplicated);
        }

        WorkItems.Add(new ProformaWeekWorkItem(ProformaId, Week, hours, freeHours, collaboratorRole, collaborator));


        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Edit(Guid collaboratorId, decimal hours, decimal freeHours, decimal minimumHours, decimal penaltyMinimumHours)
    {
        var item = WorkItems.FirstOrDefault(i => i.CollaboratorId == collaboratorId);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeekWorkItem>();
        }

        item.Edit(hours, freeHours);

        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Remove(Guid collaboratorId, decimal minimumHours, decimal penaltyMinimumHours)
    {
        var item = WorkItems.FirstOrDefault(i => i.CollaboratorId == collaboratorId);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeekWorkItem>();
        }

        WorkItems.Remove(item);

        Refresh(minimumHours, penaltyMinimumHours);
    }

    private void Refresh(decimal minimumHours, decimal penaltyMinimumHours)
    {
        var hours = WorkItems.Sum(i => i.Hours);

        if (IsCompleteWeek())
        {
            if (minimumHours > hours)
            {
                Penalty = (minimumHours - hours) * penaltyMinimumHours;
            }
            else
            {
                Penalty = 0;
            }
        }
        else
        {
            Penalty = 0;
        }

        SubTotal = WorkItems.Sum(i => i.SubTotal) + Penalty;
    }

    private bool IsCompleteWeek()
    {
        return (End - Start).TotalDays == 6;
    }
}

public class ProformaWeekWorkItem
{
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public Guid CollaboratorId { get; private set; }
    public Guid CollaboratorRoleId { get; private set; }
    public decimal Hours { get; private set; }
    public decimal FreeHours { get; private set; }
    public decimal FeeAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal ProfitAmount { get; private set; }
    public decimal ProfitPercentage { get; private set; }
    public decimal Withholding { get; private set; }
    public decimal WithholdingPercentage { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal NetSalary { get; private set; }

    private ProformaWeekWorkItem()
    {

    }
    public ProformaWeekWorkItem(Guid proformId, int week, decimal hours, decimal freeHours, CollaboratorRole collaboratorRole, Collaborator collaborator)
    {
        ProformaId = proformId;
        Week = week;
        CollaboratorId = collaborator.CollaboratorId;
        CollaboratorRoleId = collaboratorRole.CollaboratorRoleId;
        Hours = hours;
        FreeHours = freeHours;
        FeeAmount = collaboratorRole.FeeAmount;
        ProfitPercentage = collaboratorRole.ProfitPercentage;
        WithholdingPercentage = collaborator.WithholdingPercentage;
        Refresh();
    }

    private void Refresh()
    {
        SubTotal = (Hours - FreeHours) * FeeAmount;
        ProfitAmount = (SubTotal * ProfitPercentage) / 100;
        GrossSalary = SubTotal - ProfitAmount;
        Withholding = Math.Round((GrossSalary * WithholdingPercentage) / 100, 2, MidpointRounding.AwayFromZero);
        NetSalary = Math.Round(GrossSalary - Withholding, 2, MidpointRounding.AwayFromZero);
    }

    public void Edit(decimal hours, decimal freeHours)
    {
        Hours = hours;
        FreeHours = freeHours;
        Refresh();
    }
}

public class ProformaEntityTypeConfiguration : IEntityTypeConfiguration<Proforma>
{
    public void Configure(EntityTypeBuilder<Proforma> builder)
    {
        builder
            .ToTable(Tables.Proformas);

        builder
            .HasKey(p => p.ProformaId);

        builder
            .Property(c => c.Total)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Commission)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Discount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.MinimumHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.PenaltyMinimumHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.TaxesExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.AdministrativeExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.BankingExpensesPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.MinimumBankingExpenses)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.TaxesExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.AdministrativeExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.BankingExpensesAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Status)
            .HasConversion(s => s.ToString(), value => value.ToEnum<ProformaStatus>());

        builder
            .Property(c => c.Currency)
            .HasConversion(s => s.ToString(), value => value.ToEnum<Currency>());

        builder
            .HasMany(p => p.Weeks)
            .WithOne()
            .HasForeignKey(pw => pw.ProformaId);
    }
}

public class WeekEntityTypeConfiguration : IEntityTypeConfiguration<ProformaWeek>
{
    public void Configure(EntityTypeBuilder<ProformaWeek> builder)
    {
        builder
            .ToTable(Tables.ProformaWeeks);

        builder
            .HasKey(field => new { field.ProformaId, field.Week });

        builder
            .Property(c => c.Penalty)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");


        builder
            .HasMany(p => p.WorkItems)
            .WithOne()
            .HasForeignKey(wi => new { wi.ProformaId, wi.Week });
    }
}

public class WorkItemEntityTypeConfiguration : IEntityTypeConfiguration<ProformaWeekWorkItem>
{
    public void Configure(EntityTypeBuilder<ProformaWeekWorkItem> builder)
    {
        builder
            .ToTable(Tables.ProformaWeekWorkItems);

        builder
            .HasKey(field => new { field.ProformaId, field.Week, field.CollaboratorId });

        builder
            .Property(c => c.Hours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FreeHours)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.FeeAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ProfitAmount)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.ProfitPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.WithholdingPercentage)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.Withholding)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.GrossSalary)
            .HasColumnType("decimal(19, 4)");

        builder
            .Property(c => c.NetSalary)
            .HasColumnType("decimal(19, 4)");
    }
}