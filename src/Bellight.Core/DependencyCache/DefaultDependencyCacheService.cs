using Bellight.Core.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bellight.Core.DependencyCache
{
    public class DefaultDependencyCacheService: IDependencyCacheService
    {
        private readonly BellightCoreOptions _options;
        private readonly ISerializer _serializer;
        private readonly IServiceProvider _serviceProvider;

        public DefaultDependencyCacheService(BellightCoreOptions options, ISerializer serializer, IServiceProvider serviceProvider) {
            _options = options;
            _serializer = serializer;
            _serviceProvider = serviceProvider;
        }

        public DependencyCacheModel Load()
        {
            if (_options.DependencyCacheOptions == null || !_options.DependencyCacheOptions.Enabled)
            {
                return null;
            }

            var filePath = string.IsNullOrEmpty(_options.DependencyCacheOptions.FileLocation) ? _options.DependencyCacheOptions.FileName :
                Path.Combine(_options.DependencyCacheOptions.FileLocation, _options.DependencyCacheOptions.FileName);

            var content = string.Empty;
            if (!SafeExecute.Sync(() => content = LoadCache(filePath)) || string.IsNullOrEmpty(content))
            {
                return null;
            }

            var model = _serializer.TryDeserializeObject<DependencyCacheModel>(content);

            if (!VerifyAssembles(model.Assemblies))
            {
                return null;
            }

            if (model.TypeHandlers?.Any() != true)
            {
                return null;
            }

            var loadSuccess = SafeExecute.Sync(() => {
                foreach (var item in model.TypeHandlers)
                {
                    var type = Type.GetType(item.Name);
                    var handler = _serviceProvider.GetService(type) as ITypeHandler;
                    handler.LoadCache(item.Sections);
                }
            });

            if (!loadSuccess)
            {
                return null;
            }
            

            return null; // _serializer.TryDeserializeObject<DependencyCacheModel>(content);
            // TODO: verify assemblies
            // TODO: load dependencies
        }

        public void Save(DependencyCacheModel item)
        {
            if (item == null || _options.DependencyCacheOptions == null || !_options.DependencyCacheOptions.Enabled)
            {
                return;
            }

            if (_options.DependencyCacheOptions.PrettyPrint)
            {
                _serializer.Settings.Formatting = Formatting.Indented;
            }

            var serializedContent = _serializer.TrySerializeObject(item);
            if (string.IsNullOrEmpty(serializedContent))
            {
                return;
            }

            var filePath = string.IsNullOrEmpty(_options.DependencyCacheOptions.FileLocation) ? _options.DependencyCacheOptions.FileName :
                Path.Combine(_options.DependencyCacheOptions.FileLocation, _options.DependencyCacheOptions.FileName);

            SafeExecute.Sync(() => File.WriteAllText(filePath, serializedContent));
        }

        private bool VerifyAssembles(IEnumerable<string> assemblyNames)
        {
            return assemblyNames?.Any() == true;
        }

        private string LoadCache(string fileName)
        {
            return File.ReadAllText(fileName);
        }
    }
}
