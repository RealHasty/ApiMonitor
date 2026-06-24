using ApiMonitor.Data;
using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Services;

/// <summary>
/// Reads all YAML files via YamlLoaderService, then upserts them into
/// the database. Existing records are updated; new ones are inserted.
/// </summary>
public class ApiSyncService
{
    private readonly AppDbContext _db;
    private readonly YamlLoaderService _yamlLoader;
    private readonly ApiConnectionService _connectionService;
    private readonly ILogger<ApiSyncService> _logger;

    public ApiSyncService(AppDbContext db,
                          YamlLoaderService yamlLoader,
                          ApiConnectionService connectionService,
                          ILogger<ApiSyncService> logger)
    {
        _db                = db;
        _yamlLoader        = yamlLoader;
        _connectionService = connectionService;
        _logger            = logger;
    }

    /// <summary>
    /// Full sync: load all YAML → upsert DB → test connections.
    /// Call on app startup and whenever the user triggers a refresh.
    /// </summary>
    public async Task SyncAllAsync()
    {
        var yamlApis = _yamlLoader.LoadAll();

        foreach (var (relativePath, definition) in yamlApis)
        {
            // Find existing record by source file path, or create new
            var existing = await _db.Apis
                .Include(a => a.Endpoints)
                .FirstOrDefaultAsync(a => a.YamlSource == relativePath);

            if (existing == null)
            {
                existing = new Api { YamlSource = relativePath };
                _db.Apis.Add(existing);
            }

            // Update scalar fields from YAML
            existing.Name = definition.Name;
            existing.Url  = definition.Url;

            // Sync endpoints — remove old, add fresh from YAML
            existing.Endpoints.Clear();
            foreach (var ep in definition.Endpoints)
            {
                existing.Endpoints.Add(new ApiEndpoint
                {
                    Method      = ep.Method.ToUpper(),
                    Path        = ep.Path,
                    Description = ep.Description,
                    SchemaFile  = ep.SchemaFile
                });
            }

            await _db.SaveChangesAsync();

            // Test connection right after sync (matches your requirement)
            await _connectionService.TestAllEndpointsAsync(existing);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Synced and tested: {Name} ({Source})",
                existing.Name, relativePath);
        }
    }
}
