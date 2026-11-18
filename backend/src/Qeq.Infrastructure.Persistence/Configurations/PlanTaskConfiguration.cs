using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qeq.Domain.Planning;

namespace Qeq.Infrastructure.Persistence.Configurations;

public class PlanTaskConfiguration : IEntityTypeConfiguration<PlanTask>
{
    public void Configure(EntityTypeBuilder<PlanTask> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(300);
        builder.Property(x => x.Importance).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Ignore(x => x.IsOverdue);
        builder.HasIndex(x => x.DueAt);
    }
}