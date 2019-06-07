using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bellight.Core
{
    public interface IAssemblyLoader
    {
        Assembly[] Load();
    }

    public static class AssemblyLoaderExtensions
    {
        public static string ExtractAssemblyShortName(string fullName)
        {
            fullName = fullName.ToLowerInvariant();
            var index = fullName.IndexOf(',');
            return index < 0 ? fullName : fullName.Substring(0, index);
        }

        public static string ExtractAssemblyName(string moduleName)
        {
            var index = moduleName.LastIndexOf('.');
            return index < 0 ? moduleName : moduleName.Substring(0, index);
        }

        public static string GetShortName(this Assembly assembly)
        {
            return ExtractAssemblyShortName(assembly.FullName);
        }

        public static string GetQualifiedName(this Assembly assembly)
        {
            return ExtractAssemblyName(assembly.ManifestModule.Name);
        }

        public static string GetAssemblyNameFromFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".dll") 
                ? fileName : fileName.Substring(0, fileName.Length - 4);
        }
    }
}
