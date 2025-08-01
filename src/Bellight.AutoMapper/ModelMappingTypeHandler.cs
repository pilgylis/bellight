﻿using AutoMapper;
using Bellight.Core;
using Bellight.Core.DependencyCache;
using System.Reflection;

namespace Bellight.AutoMapper;

public class ModelMappingTypeHandler(IModelRegistrationService modelRegistrationService) : ITypeHandler
{
    private readonly IModelRegistrationService _modelRegistrationService = modelRegistrationService;

    private const string MappingSectionName = "Mappings";
    private const string ProfileSectionName = "Profiles";

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
                foreach (var line in section.Lines!)
                {
                    var type = Type.GetType(line);
                    _modelRegistrationService.AddProfile(type!);
                }
                continue;
            }

            foreach (var line in section.Lines!)
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

                _modelRegistrationService.AddMapping(sourceType!, destinationType!);
            }
        }
    }

    public IEnumerable<TypeHandlerCacheSection> SaveCache()
    {
        var mappings = _modelRegistrationService.GetAllMappings();
        yield return new TypeHandlerCacheSection
        {
            Name = MappingSectionName,
            Lines = mappings.Select(tuple => string.Format("{0}: {1}", tuple.Item1.AssemblyQualifiedName, tuple.Item2.AssemblyQualifiedName))
        };

        var profiles = _modelRegistrationService.GetAllProfiles();
        yield return new TypeHandlerCacheSection
        {
            Name = ProfileSectionName,
            Lines = profiles.Select(profile => profile.AssemblyQualifiedName)!
        };
    }
}