using System;

namespace Bellight.Core.Misc
{
    public static class FileNameHelper
    {
        public static string ExtractExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var indexOfLastDot = fileName.LastIndexOf('.');
            if (indexOfLastDot == -1 || indexOfLastDot >= fileName.Length)
            {
                return string.Empty;
            }
            return fileName.Substring(indexOfLastDot);
        }

        public static string ExtractFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = path.TrimEnd('/');
            var indexOfLastSlash = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            return indexOfLastSlash < 0 ? path : path.Substring(indexOfLastSlash + 1);
        }
    }
}