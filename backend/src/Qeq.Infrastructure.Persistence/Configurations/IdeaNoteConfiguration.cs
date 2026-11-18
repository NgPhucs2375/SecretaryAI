using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Qeq.Domain.Notes;

namespace Qeq.Infrastructure.Persistence.Configurations;

public class IdeaNoteConfiguration : IEntityTypeConfiguration<IdeaNote>
{
    public void Configure(EntityTypeBuilder<IdeaNote> builder)
    {
        builder.Property(x => x.Content).HasMaxLength(4000);
        builder.Property(x => x.Embedding)
            .HasConversion(v => new Vector(v!), v => v.ToArray())
            .HasColumnType("vector(768)");
    }
}