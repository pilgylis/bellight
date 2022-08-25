using Bellight.Core.DependencyCache;

namespace Bellight.Core;

public interface ITypeHandler
{
    void Process(Type type);

    void LoadCache(IEnumerable<TypeHandlerCacheSection> sections);

    IEnumerable<TypeHandlerCacheSection> SaveCache();
}