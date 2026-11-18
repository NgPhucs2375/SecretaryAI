using Microsoft.EntityFrameworkCore;
using Qeq.Domain.Entities;

namespace Qeq.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Transaction> Transactions { get; }
    DbSet<PlanTask> Tasks { get; }
    DbSet<IdeaNote> IdeaNotes { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<DailyBriefing> Briefings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}