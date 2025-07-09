using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IEntitySortDefinition<TObject, TKey> 
  where TObject : IEntity<TKey>
  where TKey: IEquatable<TKey>
{
  IEntitySortDefinition<TObject, TKey> Ascending(Expression<Func<TObject, object>> field);
  IEntitySortDefinition<TObject, TKey> Descending(Expression<Func<TObject, object>> field);
}