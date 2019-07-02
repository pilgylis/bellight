using Bellight.Core;
using System;
using System.Reflection;
using AutoMapper;
using Bellight.Core.DependencyCache;
using System.Collections.Generic;
using System.Linq;

namespace Bellight.AutoMapper
{
    public class ModelMappingTypeHandler : ITypeHandler
    {
        private readonly IModelRegistrationService _modelRegistrationService;

        private const string MappingSectionName = "Mappings";
        private const string ProfileSectionName = "Profiles";

        public ModelMappingTypeHandler(IModelRegistrationService modelRegistrationService)
        {
            _modelRegistrationService = modelRegistrationService;
        }

        public void Process(Type type)
        {
            foreach (var attribute in type.GetCustomAttributes<MappedModelAttribute>())
            {
                if (attribute == null || attribute.TargetType == null)
                {
                    continue;
                }

                var targetType = attribute.TargetType;
                if (targetType.BaseType == typeof(Profile))
                {
                    _modelRegistrationService.AddProfile(targetType);
                }
                else
                {
                    _modelRegistrationService.AddMapping(type, targetType);

                }
            }
        }

        public void LoadCache(IEnumerable<TypeHandlerCacheSection> sections)
        {
            foreach (var section in sections)
            {
                if (ProfileSectionName.Equals(section.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var line in section.Lines)
                    {
                        var type = Type.GetType(line);
                        _modelRegistrationService.AddProfile(type);
                    }
                } else
                {
                    foreach(var line in section.Lines)
                    {
                        if (line.IndexOf(':') < 0)
                        {
                            continue;
                        }

                        var parts = line.Split(':');
                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        var sourceTypeName = parts[0].Trim();
                        var destinationTypeName = parts[1].Trim();

                        var sourceType = Type.GetType(sourceTypeName);
                        var destinationType = Type.GetType(destinationTypeName);

                        _modelRegistrationService.AddMapping(sourceType, destinationType);
                    }
                }
            }
        }

        public IEnumerable<TypeHandlerCacheSection> SaveCache()
        {
            var profiles = _modelRegistrationService.GetAllProfiles();
            var mappings = _modelRegistrationService.GetAllMappings();
            return new List<TypeHandlerCacheSection> {
                new TypeHandlerCacheSection
                {
                    Name = MappingSectionName,
                    Lines = mappings.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName))
                },
                new TypeHandlerCacheSection
                {
                    Name = ProfileSectionName,
                    Lines = profiles.Select(profile => profile.AssemblyQualifiedName)
                }

            };
        }
    }
}
