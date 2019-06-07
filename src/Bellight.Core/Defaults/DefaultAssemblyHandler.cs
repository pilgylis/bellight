using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bellight.Core.Defaults
{
    public class DefaultAssemblyHandler : IAssemblyHandler
    {
        private readonly IEnumerable<ITypeHandler> _typeHandlers;

        public DefaultAssemblyHandler(IEnumerable<ITypeHandler> typeHandlers)
        {
            _typeHandlers = typeHandlers;
        }

        public void Process(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes().Where(TypeCondition))
            {
                foreach (var typeHandler in _typeHandlers)
                {
                    typeHandler.Process(type);
                }
            }
        }

        private static bool TypeCondition(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return typeInfo.IsClass
                && typeInfo.IsPublic
                && !typeInfo.IsAbstract;
        }
    }
}
