using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.Core.Defaults
{
    public class DependencyTypeHandler : ITypeHandler
    {
        private readonly Type _dependencyType = typeof(ITransientDependency);
        private readonly Type _scopedDependencyType = typeof(IScopedDependency);
        private readonly Type _singletonType = typeof(ISingletonDependency);
        private readonly Type _keyedType = typeof(IKeyedDependency);

        private readonly IServiceCollection _builder;
        private readonly IDictionary<string, Type> _keyedTypeDictionary;

        public DependencyTypeHandler(IServiceCollection builder, IDictionary<string, Type> keyedTypeDictionary)
        {
            _builder = builder;
            _keyedTypeDictionary = keyedTypeDictionary;
        }

        public void Process(Type type)
        {
            if (!_dependencyType.IsAssignableFrom(type))
            {
                return;
            }

            foreach (var interfaceType in type
                .GetInterfaces()
                .Where(itf => _dependencyType.IsAssignableFrom(itf)))
            {
                if (_singletonType.IsAssignableFrom(interfaceType))
                {
                    _builder.AddSingleton(interfaceType, type);
                    _builder.AddSingleton(type);
                }
                else if (_keyedType.IsAssignableFrom(interfaceType))
                {
                    var key = GetKey(type);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    _builder.AddTransient(interfaceType, type);
                    _builder.AddTransient(type);
                    if (_keyedTypeDictionary.ContainsKey(key))
                    {
                        _keyedTypeDictionary.Add(key, type);
                    }
                }
                else if (_scopedDependencyType.IsAssignableFrom(interfaceType))
                {
                    _builder.AddScoped(interfaceType, type);
                    _builder.AddScoped(type);
                }
                else
                {
                    _builder.AddTransient(interfaceType, type);
                    _builder.AddTransient(type);
                }
            }
        }

        private static string GetKey(Type type)
        {
            var keyAttribute = type.GetCustomAttribute<KeyedServiceAttribute>();
            if (keyAttribute == null)
            {
                return string.Empty;
            }

            return !string.IsNullOrEmpty(keyAttribute?.Name) ? keyAttribute.Name : type.FullName;
        }
    }
}
