namespace ApiMonitor.Models;

// -------------------------------------------------------
// These classes mirror exactly what's in your .yaml files.
// YamlDotNet deserializes the file into YamlApiDefinition.
// -------------------------------------------------------

public class YamlApiDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<YamlEndpoint> Endpoints { get; set; } = new();
}

public class YamlEndpoint
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SchemaFile { get; set; }
}
