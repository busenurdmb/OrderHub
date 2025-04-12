
using Microsoft.EntityFrameworkCore;
using OrderHub.Shared.Models;
using System.Collections.Generic;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
