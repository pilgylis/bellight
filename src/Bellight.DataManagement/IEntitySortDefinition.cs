using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IEntitySortDefinition<TObject> where TObject : IEntity {
  IEntitySortDefinition<TObject> Ascending(Expression<Func<TObject, object>> field);
  IEntitySortDefinition<TObject> Descending(Expression<Func<TObject, object>> field);
}