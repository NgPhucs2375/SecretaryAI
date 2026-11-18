using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qeq.Domain.Entities;

namespace Qeq.Infrastructure.Persistence.Configurations;

public class DailyBriefingConfiguration : IEntityTypeConfiguration<DailyBriefing>
{
    public void Configure(EntityTypeBuilder<DailyBriefing> builder)
    {
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
        builder.HasIndex(x => new { x.Date, x.Kind }).IsUnique();
    }
}