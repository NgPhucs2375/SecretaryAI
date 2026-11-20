using Microsoft.EntityFrameworkCore;
using Qeq.Application.Interfaces;
using Qeq.Domain.Entities;

namespace Qeq.Infrastructure.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<PlanTask> Tasks => Set<PlanTask>();
    public DbSet<IdeaNote> IdeaNotes => Set<IdeaNote>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<DailyBriefing> Briefings => Set<DailyBriefing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}