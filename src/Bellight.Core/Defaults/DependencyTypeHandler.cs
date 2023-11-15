using Bellight.Core.DependencyCache;
using Bellight.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bellight.Core.Defaults;

public class DependencyTypeHandler : ITypeHandler
{
    private readonly Type _dependencyType = typeof(ITransientDependency);
    private readonly Type _scopedDependencyType = typeof(IScopedDependency);
    private readonly Type _singletonType = typeof(ISingletonDependency);
    private readonly Type _keyedType = typeof(IKeyedDependency);

    private readonly IServiceCollection _services;
    private readonly IKeyedServiceRegistry _keyedServiceRegistry;

    private readonly IList<Type> _singletonTypes = new List<Type>();
    private readonly IList<Type> _scopedTypes = new List<Type>();
    private readonly IList<Type> _transientTypes = new List<Type>();
    private readonly IList<Tuple<Type, Type>> _singletonMaps = new List<Tuple<Type, Type>>();
    private readonly IList<Tuple<Type, Type>> _scopedMaps = new List<Tuple<Type, Type>>();
    private readonly IList<Tuple<Type, Type>> _transientMaps = new List<Tuple<Type, Type>>();
    private readonly IList<Tuple<string, Type, Type>> _keyedMaps = new List<Tuple<string, Type, Type>>();

    public DependencyTypeHandler(IServiceCollection services, IKeyedServiceRegistry keyedServiceRegistry)
    {
        _services = services;
        _keyedServiceRegistry = keyedServiceRegistry;
        _keyedServiceRegistry.Clear();
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
                _services.AddSingleton(type);
                _singletonTypes.Add(type);

                if (interfaceType != _singletonType)
                {
                    _services.AddSingleton(interfaceType, type);
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

                _services.AddTransient(type);
                _transientTypes.Add(type);

                if (interfaceType != _keyedType)
                {
                    _services.AddTransient(interfaceType, type);
                }

                if (!_keyedServiceRegistry.ContainsKey(key))
                {
                    _keyedServiceRegistry.Add(key, type);
                    _keyedMaps.Add(new Tuple<string, Type, Type>(key, interfaceType, type));
                }
            }
            else if (_scopedDependencyType.IsAssignableFrom(interfaceType))
            {
                _services.AddScoped(type);
                _scopedTypes.Add(type);

                if (interfaceType != _scopedDependencyType)
                {
                    _scopedMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                    _services.AddScoped(interfaceType, type);
                }
            }
            else
            {
                _services.AddTransient(type);
                _transientTypes.Add(type);

                if (interfaceType != _dependencyType)
                {
                    _transientMaps.Add(new Tuple<Type, Type>(interfaceType, type));
                    _services.AddTransient(interfaceType, type);
                }
            }
        }
    }

    #region Load Cache

    public void LoadCache(IEnumerable<TypeHandlerCacheSection> sections)
    {
        Action<Type>? typeAction = null;
        Action<Type, Type>? mapAction = null;
        foreach (var section in sections)
        {
            if ("SingletonTypes".Equals(section.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                typeAction = type => _services.AddSingleton(type);
                mapAction = (interfaceType, implementationType) => _services.AddSingleton(interfaceType, implementationType);
            }
            else if ("ScopeTypes".Equals(section.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                typeAction = type => _services.AddScoped(type);
                mapAction = (interfaceType, implementationType) => _services.AddScoped(interfaceType, implementationType);
            }
            else if ("KeyedTypes".Equals(section.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                typeAction = type => _services.AddTransient(type);
                mapAction = (interfaceType, implementationType) => _services.AddTransient(interfaceType, implementationType);

                foreach (var line in section.Lines!)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex < 0)
                    {
                        var type = Type.GetType(line);
                        _services.AddTransient(type!);
                        continue;
                    }

                    var parts = line.Split(':');
                    if (parts.Length != 3)
                    {
                        throw new BellightStartupException($"Error parsing keyed dependency: {line}");
                    }

                    var key = parts[0].Trim();
                    var interfaceTypeName = parts[1].Trim();
                    var implementationTypeName = parts[2].Trim();

                    var interfaceType = Type.GetType(interfaceTypeName);
                    var implementationType = Type.GetType(implementationTypeName);

                    _keyedServiceRegistry.Add(key, implementationType!);
                    _services.AddTransient(interfaceType!, implementationType!);
                }

                continue;
            }
            else // TransientTypes
            {
                typeAction = type => _services.AddTransient(type);
                mapAction = (interfaceType, implementationType) => _services.AddTransient(interfaceType, implementationType);
            }

            foreach (var line in section.Lines!)
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex < 0)
                {
                    var type = Type.GetType(line);
                    typeAction(type!);
                    continue;
                }

                var interfaceTypeName = line[..colonIndex].Trim();
                var typeName = line[(colonIndex + 1)..];

                var interfaceType = Type.GetType(interfaceTypeName);
                var implementationType = Type.GetType(typeName);
                mapAction(interfaceType!, implementationType!);
            }
        }
    }

    #endregion Load Cache

    #region Save Cache

    public IEnumerable<TypeHandlerCacheSection> SaveCache()
    {
        var sections = new List<TypeHandlerCacheSection>
        {
            CreateSection("SingletonTypes", _singletonTypes, _singletonMaps),
            CreateSection("ScopeTypes", _scopedTypes, _scopedMaps),
            CreateSection("TransientTypes", _transientTypes, _transientMaps),
            // keyed
            new() {
                Name = "KeyedTypes",
                Lines = _keyedMaps.Select(tuple => string.Format("{0}: {1}: {2}",
                    tuple.Item1,
                    tuple.Item2.AssemblyQualifiedName,
                    tuple.Item3.AssemblyQualifiedName))
            }
        };

        return sections;
    }

    private static TypeHandlerCacheSection CreateSection(string name, IEnumerable<Type> types, IEnumerable<Tuple<Type, Type>> maps)
    {
        var section = new TypeHandlerCacheSection
        {
            Name = name
        };

        var lines = new List<string>();

        lines.AddRange(types.Distinct().Select(t => t.AssemblyQualifiedName)!);
        lines.AddRange(maps.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName)));

        section.Lines = lines;

        return section;
    }

    #endregion Save Cache

    private static string GetKey(Type type)
    {
        var keyAttribute = type.GetCustomAttribute<KeyedServiceAttribute>();
        if (keyAttribute == null)
        {
            return type.FullName!;
        }

        return !string.IsNullOrEmpty(keyAttribute?.Name) ? keyAttribute.Name : type.FullName!;
    }
}