namespace Bellight.Core.DependencyCache;

public class DependencyCacheModel
{
    public IEnumerable<string>? Assemblies { get; set; }
    public IEnumerable<TypeHandlerCacheModel>? TypeHandlers { get; set; }
}

public class TypeHandlerCacheModel
{
    public string? Name { get; set; }
    public IEnumerable<TypeHandlerCacheSection>? Sections { get; set; }
}

public class TypeHandlerCacheSection
{
    public string? Name { get; set; }
    public IEnumerable<string>? Lines { get; set; }
}