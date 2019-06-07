using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bellight.Core.Defaults
{
    public class DefaultAssemblyScanner: IAssemblyScanner
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IAssemblyHandler _assemblyHandler;
        public DefaultAssemblyScanner(IAssemblyLoader assemblyLoader, IAssemblyHandler assemblyHandler)
        {
            _assemblyLoader = assemblyLoader;
            _assemblyHandler = assemblyHandler;
        }

        public void Scan(IEnumerable<Assembly> additionalAssemblies)
        {
            var thisAssembly = GetType().GetTypeInfo().Assembly;

            var thisAssemblyName = thisAssembly.GetName().Name;

            var assemblies = _assemblyLoader.Load()
                .Where(a => excludeThisAssemblyPredicate(a, thisAssemblyName));

            _assemblyHandler.Process(thisAssembly);

            if (assemblies?.Any() == true)
            {
                foreach (var assembly in assemblies)
                {
                    _assemblyHandler.Process(assembly);
                }
            }

            if (additionalAssemblies?.Any() == true)
            {
                foreach (var assembly in additionalAssemblies)
                {
                    _assemblyHandler.Process(assembly);
                }
            }
        }

        private Func<Assembly, string, bool> excludeThisAssemblyPredicate = (a, b) => a.GetReferencedAssemblies().Any(asb => string.CompareOrdinal(asb.Name, b) == 0);
    }
}
