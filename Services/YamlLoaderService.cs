using ApiMonitor.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ApiMonitor.Services;

/// <summary>
/// Scans the APIData folder, deserializes every .yaml/.yml file it finds,
/// and returns the parsed definitions. Call this on startup or on-demand
/// to refresh what's in the database.
/// </summary>
public class YamlLoaderService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<YamlLoaderService> _logger;

    // YamlDotNet deserializer — camelCase keys match C# PascalCase properties
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public YamlLoaderService(IWebHostEnvironment env, ILogger<YamlLoaderService> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Returns a list of (relativeFilePath, parsed definition) tuples
    /// for every YAML file found recursively under /APIData.
    /// </summary>
    public List<(string RelativePath, YamlApiDefinition Definition)> LoadAll()
    {
        // APIData lives alongside the project root (ContentRootPath)
        var apiDataPath = Path.Combine(_env.ContentRootPath, "APIData");

        if (!Directory.Exists(apiDataPath))
        {
            _logger.LogWarning("APIData directory not found at {Path}", apiDataPath);
            return new();
        }

        var results = new List<(string, YamlApiDefinition)>();

        // Walk every .yaml / .yml file recursively
        var files = Directory.GetFiles(apiDataPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
                                      || f.EndsWith(".yml",  StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            try
            {
                var yaml = File.ReadAllText(file);
                var definition = _deserializer.Deserialize<YamlApiDefinition>(yaml);

                // Store path relative to APIData/ for display purposes
                var relativePath = Path.GetRelativePath(apiDataPath, file);
                results.Add((relativePath, definition));

                _logger.LogInformation("Loaded YAML: {File}", relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse YAML file: {File}", file);
            }
        }

        return results;
    }
}
