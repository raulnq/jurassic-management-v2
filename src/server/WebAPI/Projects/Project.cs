using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.Projects;

public class Project
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    private Project() { }

    public Project(Guid projectId, Guid clientId, string name)
    {
        ProjectId = projectId;
        Name = name;
        ClientId = clientId;
    }

    public void Edit(string name)
    {
        Name = name;
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder
            .ToTable(Tables.Projects);

        builder
            .HasKey(p => p.ProjectId);
    }
}