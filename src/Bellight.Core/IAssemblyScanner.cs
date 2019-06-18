using Bellight.Core.DependencyCache;

namespace Bellight.Core
{
    public interface IAssemblyScanner
    {
        DependencyCacheModel Scan();
    }
}
