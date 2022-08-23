namespace Bellight.Core.DependencyCache;

public class DependencyCacheOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The name of the cache file
    /// </summary>
    public string FileName { get; set; } = "bellight-cache.json";

    /// <summary>
    /// Location of the cache file. Default: current directory
    /// </summary>
    public string? FileLocation { get; set; }

    public bool PrettyPrint { get; set; } = false;
}
