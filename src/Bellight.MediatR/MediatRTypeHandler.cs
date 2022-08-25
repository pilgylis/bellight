using Bellight.Core;
using Bellight.Core.DependencyCache;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bellight.MediatR
{
    public class MediatRTypeHandler : ITypeHandler
    {
        private readonly Type[] multiOpenInterfaces = new[]
        {
            typeof(INotificationHandler<>),
            typeof(IRequestPreProcessor<>),
            typeof(IRequestPostProcessor<,>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>)
        };

        private readonly Type[] handlerInterfaceTypes = new[]
        {
            typeof(IRequestHandler<,>),
            typeof(INotificationHandler<>),
            typeof(IRequestPreProcessor<>),
            typeof(IRequestPostProcessor<,>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>)
        };

        private readonly List<Tuple<Type, Type>> typeMap = new List<Tuple<Type, Type>>();

        private readonly List<Tuple<Type, Type>> possibleCloseMap = new List<Tuple<Type, Type>>();

        private readonly IServiceCollection services;

        public MediatRTypeHandler(IServiceCollection services)
        {
            this.services = services;
        }

        public void Process(Type type)
        {
            if (!type.IsConcrete())
            {
                return;
            }

            if (type.IsOpenGeneric())
            {
                foreach (var multiOpenInterface in multiOpenInterfaces)
                {
                    if (type.FindInterfacesThatClose(multiOpenInterface).Any())
                    {
                        typeMap.Add(new Tuple<Type, Type>(multiOpenInterface, type));
                        services.AddTransient(multiOpenInterface, type);
                    }
                }

                return;
            }

            foreach (var handlerInterfaceType in handlerInterfaceTypes)
            {
                var closeInterfaces = type.FindInterfacesThatClose(handlerInterfaceType);

                foreach (var interfaceType in closeInterfaces)
                {
                    if (type.CanBeCastTo(interfaceType))
                    {
                        typeMap.Add(new Tuple<Type, Type>(interfaceType, type));
                        services.AddTransient(interfaceType, type);
                    }

                    if (!interfaceType.IsOpenGeneric() && type.CouldCloseTo(interfaceType))
                    {
                        possibleCloseMap.Add(new Tuple<Type, Type>(interfaceType, type));

                        try
                        {
                            services.TryAddTransient(interfaceType, type.MakeGenericType(interfaceType.GenericTypeArguments));
                        }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                        catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                        { }
                    }
                }
            }
        }

        public void LoadCache(IEnumerable<TypeHandlerCacheSection> sections)
        {
            foreach (var section in sections)
            {
                var isCloseServicesSection = !"Services".Equals(section.Name, StringComparison.OrdinalIgnoreCase);

                foreach (var line in section.Lines!)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex < 0)
                    {
                        continue;
                    }

                    var interfaceTypeName = line.Substring(0, colonIndex).Trim();
                    var implementationTypeName = line.Substring(colonIndex + 1);

                    var interfaceType = Type.GetType(interfaceTypeName);
                    var implementationType = Type.GetType(implementationTypeName);

                    if (!isCloseServicesSection && interfaceType != null && implementationType != null)
                    {
                        services.AddTransient(interfaceType, implementationType);
                        continue;
                    }

                    if (interfaceType == null || implementationType == null)
                    {
                        continue;
                    }

                    try
                    {
                        services.TryAddTransient(interfaceType!, implementationType.MakeGenericType(interfaceType.GenericTypeArguments));
                    }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                    catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                    { }
                }
            }
        }

        public IEnumerable<TypeHandlerCacheSection> SaveCache()
        {
            yield return new TypeHandlerCacheSection
            {
                Name = "Services",
                Lines = typeMap.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName))
            };
            yield return new TypeHandlerCacheSection
            {
                Name = "PossibleCloseServices",
                Lines = possibleCloseMap.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName))
            };
        }
    }
}