using AutoMapper;
using System;
using System.Reflection;

namespace Bellight.AutoMapper
{
    public static class ModelMappingExtensions
    {
        public static IMappingExpression IgnoreAllNonExisting
            (this IMappingExpression expression, Type sourceType, Type destinationType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo property in destinationType.GetProperties(flags))
            {
                if (sourceType.GetProperty(property.Name, flags) == null)
                {
                    expression.ForMember(property.Name, opt => opt.Ignore());
                }
            }
            return expression;
        }
    }
}
