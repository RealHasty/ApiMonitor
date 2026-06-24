namespace ApiMonitor.Models;

public class Api
{
    public int Id { get; set; }

    // Loaded from YAML
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    // Populated by connection test
    public int? StatusCode { get; set; }
    public long? ResponseTimeMs { get; set; }
    public bool IsRunning { get; set; }

    // Which YAML file this came from (relative path inside APIData/)
    public string YamlSource { get; set; } = string.Empty;

    // Navigation to endpoints defined in the YAML
    public List<ApiEndpoint> Endpoints { get; set; } = new();
}

public class ApiEndpoint
{
    public int Id { get; set; }
    public int ApiId { get; set; }

    public string Method { get; set; } = string.Empty;   // GET / POST / PUT / DELETE
    public string Path { get; set; } = string.Empty;     // e.g. /users/{id}
    public string? Description { get; set; }
    public string? SchemaFile { get; set; }              // path to schema json/yaml

    // Last test result for this specific endpoint
    public int? StatusCode { get; set; }
    public long? ResponseTimeMs { get; set; }
    public bool IsRunning { get; set; }

    public Api Api { get; set; } = null!;
}
