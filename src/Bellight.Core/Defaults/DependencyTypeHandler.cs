using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bellight.Core.DependencyCache;
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

        private readonly IList<Type> _singletonTypes = new List<Type>();
        private readonly IList<Type> _scopedTypes = new List<Type>();
        private readonly IList<Type> _transientTypes = new List<Type>();
        private readonly IList<Tuple<Type, Type>> _singletonMaps = new List<Tuple<Type, Type>>();
        private readonly IList<Tuple<Type, Type>> _scopedMaps = new List<Tuple<Type, Type>>();
        private readonly IList<Tuple<Type, Type>> _transientMaps = new List<Tuple<Type, Type>>();
        private readonly IList<Tuple<string, Type>> _keyedMaps = new List<Tuple<string, Type>>();

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
                    _builder.AddSingleton(type);
                    _singletonTypes.Add(type);

                    if (interfaceType != _singletonType)
                    {
                        _builder.AddSingleton(interfaceType, type);
                        _singletonMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                    }
                }
                else if (_keyedType.IsAssignableFrom(interfaceType))
                {
                    var key = GetKey(type);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    _builder.AddTransient(type);
                    _transientTypes.Add(type);

                    if (interfaceType != _keyedType)
                    {
                        _transientMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                        _builder.AddTransient(interfaceType, type);
                    }

                    if (!_keyedTypeDictionary.ContainsKey(key))
                    {
                        _keyedTypeDictionary.Add(key, type);
                        _keyedMaps.Add(new Tuple<string, Type>(key, type));
                    }
                }
                else if (_scopedDependencyType.IsAssignableFrom(interfaceType))
                {
                    _builder.AddScoped(type);
                    _scopedTypes.Add(type);

                    if (interfaceType != _scopedDependencyType)
                    {
                        _scopedMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                        _builder.AddScoped(interfaceType, type);
                    }
                }
                else
                {
                    _builder.AddTransient(type);
                    _transientTypes.Add(type);

                    if (interfaceType != _dependencyType)
                    {
                        _transientMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                        _builder.AddTransient(interfaceType, type);
                    }
                }
            }
        }

        public void LoadCache(IEnumerable<TypeHandlerCacheSection> sections)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TypeHandlerCacheSection> SaveCache()
        {
            var sections = new List<TypeHandlerCacheSection>
            {
                CreateSection("SingletonTypes", _singletonTypes, _singletonMaps),
                CreateSection("ScopeTypes", _scopedTypes, _scopedMaps),
                CreateSection("TransientTypes", _transientTypes, _transientMaps)
            };

            // keyed
            sections.Add(new TypeHandlerCacheSection {
                Name = "KeyedTypes",
                Lines = _keyedMaps.Select(tuple => string.Format("{0}: {1}", tuple.Item1, tuple.Item2.AssemblyQualifiedName))
            });

            return sections;
        }

        private TypeHandlerCacheSection CreateSection(string name, IEnumerable<Type> types, IEnumerable<Tuple<Type, Type>> maps)
        {
            var section = new TypeHandlerCacheSection
            {
                Name = name
            };

            var lines = new List<string>();

            lines.AddRange(types.Distinct().Select(t => t.AssemblyQualifiedName));
            lines.AddRange(maps.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName)));

            section.Lines = lines;

            return section;
        }

        private static string GetKey(Type type)
        {
            var keyAttribute = type.GetCustomAttribute<KeyedServiceAttribute>();
            if (keyAttribute == null)
            {
                return type.FullName;
            }

            return !string.IsNullOrEmpty(keyAttribute?.Name) ? keyAttribute.Name : type.FullName;
        }
    }
}
