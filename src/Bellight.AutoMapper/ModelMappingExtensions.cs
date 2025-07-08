using AutoMapper;
using System.Reflection;

namespace Bellight.AutoMapper;

public static class ModelMappingExtensions
{
    public static IMappingExpression IgnoreAllNonExisting
        (this IMappingExpression expression, Type sourceType, Type destinationType)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        foreach (var property in destinationType.GetProperties(flags)
            .Where(property => sourceType.GetProperty(property.Name, flags) == null))
        {
            expression.ForMember(property.Name, opt => opt.Ignore());
        }

        return expression;
    }
}