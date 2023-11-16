using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IEntityUpdateDefinition<TObject> where TObject : IEntity {
    IEntityUpdateDefinition<TObject> Set<TField>(
        Expression<Func<TObject, TField>> field,
        TField fieldValue);
}