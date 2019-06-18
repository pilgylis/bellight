namespace Bellight.Core.DependencyCache
{
    public interface IDependencyCacheService
    {
        bool Load();
        void Save(DependencyCacheModel item);
    }
}
