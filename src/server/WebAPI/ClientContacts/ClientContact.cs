using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.ClientContacts;

public class ClientContact
{
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Guid ClientContactId { get; private set; }
    private ClientContact() { }

    public ClientContact(Guid clientContactId, Guid clientId, string name, string email)
    {
        ClientContactId = clientContactId;
        Name = name;
        ClientId = clientId;
        Email = email;
    }

    public void Edit(string name, string email)
    {
        Name = name;
        Email = email;
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<ClientContact>
{
    public void Configure(EntityTypeBuilder<ClientContact> builder)
    {
        builder
            .ToTable(Tables.ClientContacts);

        builder
            .HasKey(p => p.ClientContactId);
    }
}