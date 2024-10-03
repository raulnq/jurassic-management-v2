using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.ProformaDocuments;

namespace WebAPI.ProformaDocuments
{
    public class ProformaDocument
    {
        public Guid ProformaId { get; private set; }
        public string Url { get; private set; } = default!;

        private ProformaDocument()
        {

        }

        public ProformaDocument(Guid proformaId, string url)
        {
            ProformaId = proformaId;
            Url = url;
        }
    }
}

public class EntityTypeConfiguration : IEntityTypeConfiguration<ProformaDocument>
{
    public void Configure(EntityTypeBuilder<ProformaDocument> builder)
    {
        builder
            .ToTable(Tables.ProformaDocuments);

        builder
            .HasKey(p => p.ProformaId);
    }
}
