using System.Linq.Expressions;

namespace Bellight.DataManagement;

public abstract class EntityUpdateDefinition<TObject> where TObject : IEntity
{

    public abstract EntityUpdateDefinition<TObject> Set<TField>(
        Expression<Func<TObject, TField>> field,
        TField fieldValue);
}
