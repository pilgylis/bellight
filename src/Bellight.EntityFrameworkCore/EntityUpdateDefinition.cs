using System.Linq.Expressions;
using Bellight.DataManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Bellight.EntityFrameworkCore;

public class EntityUpdateDefinition<TObject, TKey> : IEntityUpdateDefinition<TObject, TKey> 
    where TObject : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    public Action<UpdateSettersBuilder<TObject>> SetPropertyExpression { get; private set; } = new UpdateSettersBuilder<TObject>();
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
        SetPropertyExpression = b => b.SetProperty<TProperty>(propertyExpression, valueExpression);
        SetPropertyExpression = SetPropertyExpression.Update(propertyExpression, valueExpression);
            body: Expression.Call(
                instance: SetPropertyExpression.Body,
                methodName: nameof(SetPropertyCalls<TProperty>.SetProperty),
                typeArguments: [typeof(TProperty)],
                propertyExpression,
                valueExpression
            ),
            parameters: SetPropertyExpression.Parameters
        );
    }
}
