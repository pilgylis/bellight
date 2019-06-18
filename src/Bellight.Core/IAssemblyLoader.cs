using System.Reflection;

namespace Bellight.Core
{
    public interface IAssemblyLoader
    {
        Assembly[] Load();
    }

    public static class AssemblyLoaderExtensions
    {
        internal static string ExtractAssemblyShortName(this string fullName)
        {
            fullName = fullName.ToLowerInvariant();
            var index = fullName.IndexOf(',');
            return index < 0 ? fullName : fullName.Substring(0, index);
        }

        internal static string ExtractAssemblyName(this string moduleName)
        {
            var index = moduleName.LastIndexOf('.');
            return index < 0 ? moduleName : moduleName.Substring(0, index);
        }

        internal static string GetShortName(this Assembly assembly)
        {
            return ExtractAssemblyShortName(assembly.FullName);
        }

        internal static string GetQualifiedName(this Assembly assembly)
        {
            return ExtractAssemblyName(assembly.ManifestModule.Name);
        }

        internal static string GetAssemblyNameFromFileName(this string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".dll")
                ? fileName : fileName.Substring(0, fileName.Length - 4);
        }
    }
}
