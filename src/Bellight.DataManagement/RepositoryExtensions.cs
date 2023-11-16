using System.Linq.Expressions;

namespace Bellight.DataManagement;

public static class RepositoryExtensions
{
    public static Task UpdateAsync<T, Tid>(
        this IRepository<T, Tid> repository,
        Tid id,
        Func<IEntityUpdateDefinition<T>, IEntityUpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default
    ) where T : class, IEntity<Tid> {
        var update = repository.UpdateDefinition;
        updateFunc.Invoke(update);
        return repository.UpdateAsync(id, update, cancellationToken);
    }
    
    public static Task<long> UpdateManyAsync<T, Tid>(
        this IRepository<T, Tid> repository,
        Expression<Func<T, bool>> filter,
        Func<IEntityUpdateDefinition<T>, IEntityUpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default
    ) where T : class, IEntity<Tid> {
        var update = repository.UpdateDefinition;
        updateFunc.Invoke(update);
        return repository.UpdateManyAsync(filter, update, cancellationToken);
    }
}