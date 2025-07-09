using System.Linq.Expressions;
using Bellight.DataManagement;
using Microsoft.EntityFrameworkCore.Query;

namespace Bellight.EntityFrameworkCore;

public class EntityUpdateDefinition<TObject, TKey> : IEntityUpdateDefinition<TObject, TKey> 
    where TObject : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    public Expression<Func<SetPropertyCalls<TObject>, SetPropertyCalls<TObject>>> SetPropertyExpression { get; private set; } = b => b;
    public IEntityUpdateDefinition<TObject, TKey> Set<TField>(Expression<Func<TObject, TField>> field, TField fieldValue)
    {
        SetProperty(field, _ => fieldValue);
        return this;
    }

    private void SetProperty<TProperty>(
        Expression<Func<TObject, TProperty>> propertyExpression,
        Expression<Func<TObject, TProperty>> valueExpression
    )
    {
        SetPropertyExpression = SetPropertyExpression.Update(
            body: Expression.Call(
                instance: SetPropertyExpression.Body,
                methodName: nameof(SetPropertyCalls<TObject>.SetProperty),
                typeArguments: [typeof(TProperty)],
                propertyExpression,
                valueExpression
            ),
            parameters: SetPropertyExpression.Parameters
        );
    }
}
