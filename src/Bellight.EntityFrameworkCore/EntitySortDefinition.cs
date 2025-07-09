using System.Linq.Expressions;
using Bellight.DataManagement;

namespace Bellight.EntityFrameworkCore;

public class EntitySortDefinition<TObject, TKey> : IEntitySortDefinition<TObject, TKey> 
    where TObject : IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    public List<Tuple<bool, Expression<Func<TObject, object>>>> Expressions { get; private set; } = [];
    public IEntitySortDefinition<TObject, TKey> Ascending(Expression<Func<TObject, object>> field)
    {
        Expressions.Add(new Tuple<bool, Expression<Func<TObject, object>>>(true, field));
        return this;
    }

    public IEntitySortDefinition<TObject, TKey> Descending(Expression<Func<TObject, object>> field)
    {
        Expressions.Add(new Tuple<bool, Expression<Func<TObject, object>>>(false, field));
        return this;
    }

    public IQueryable<TObject> Apply(IQueryable<TObject> queryable)
    {
        foreach (var item in Expressions)
        {
            queryable = item.Item1 ? queryable.OrderBy(item.Item2) : queryable.OrderByDescending(item.Item2);
        }

        return queryable;
    }
}
