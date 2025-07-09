using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IEntityUpdateDefinition<TObject, TKey> 
  where TObject : IEntity<TKey>
  where TKey: IEquatable<TKey>
{
    IEntityUpdateDefinition<TObject, TKey> Set<TField>(
        Expression<Func<TObject, TField>> field,
        TField fieldValue);
}