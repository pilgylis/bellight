using System.Linq.Expressions;
using Bellight.DataManagement;
using Microsoft.EntityFrameworkCore.Query;

namespace Bellight.EntityFrameworkCore;

public class EntityUpdateDefinition<TObject, TKey> : IEntityUpdateDefinition<TObject, TKey> 
    where TObject : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    private readonly List<Action<UpdateSettersBuilder<TObject>>> _updates = [];

    public Action<UpdateSettersBuilder<TObject>> SetPropertyExpression => b =>
    {
        foreach (var update in _updates)
        {
            update(b);
        }
    };

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
        _updates.Add(b => b.SetProperty(propertyExpression, valueExpression));
    }
}
