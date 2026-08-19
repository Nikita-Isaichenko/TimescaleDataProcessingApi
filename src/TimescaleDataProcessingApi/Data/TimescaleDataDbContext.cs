using Microsoft.EntityFrameworkCore;
using TimescaleDataProcessingApi.Models;

public class TimescaleDataDbContext : DbContext
{
    public TimescaleDataDbContext(DbContextOptions<TimescaleDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<Result> Results { get; set; }

    public DbSet<ValueEntry> Values { get; set; }
}