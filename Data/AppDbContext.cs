using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Api> Apis { get; set; }
    public DbSet<ApiEndpoint> ApiEndpoints { get; set; }
    }
}
