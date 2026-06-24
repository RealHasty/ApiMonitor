using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Api> Apis => Set<Api>();
    public DbSet<ApiEndpoint> ApiEndpoints => Set<ApiEndpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Api
        modelBuilder.Entity<Api>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired().HasMaxLength(200);
            e.Property(a => a.Url).IsRequired().HasMaxLength(2000);
            e.Property(a => a.YamlSource).HasMaxLength(500);
        });

        // ApiEndpoint
        modelBuilder.Entity<ApiEndpoint>(e =>
        {
            e.HasKey(ep => ep.Id);
            e.Property(ep => ep.Method).IsRequired().HasMaxLength(10);
            e.Property(ep => ep.Path).IsRequired().HasMaxLength(500);
            e.Property(ep => ep.Description).HasMaxLength(1000);
            e.Property(ep => ep.SchemaFile).HasMaxLength(500);

            e.HasOne(ep => ep.Api)
             .WithMany(a => a.Endpoints)
             .HasForeignKey(ep => ep.ApiId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
