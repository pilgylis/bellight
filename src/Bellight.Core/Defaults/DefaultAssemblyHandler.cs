using System.Reflection;

namespace Bellight.Core.Defaults;

public class DefaultAssemblyHandler(IEnumerable<ITypeHandler> typeHandlers) : IAssemblyHandler
{
    public void Process(Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes().Where(TypeCondition))
        {
            foreach (var typeHandler in typeHandlers)
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