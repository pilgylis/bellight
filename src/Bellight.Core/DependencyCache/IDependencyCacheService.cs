namespace Bellight.Core.DependencyCache
{
    public interface IDependencyCacheService
    {
        DependencyCacheModel Load();
        void Save(DependencyCacheModel item);
    }
}
