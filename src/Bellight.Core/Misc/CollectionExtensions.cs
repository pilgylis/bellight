namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null || action == null)
            {
                return enumerable!;
            }

            foreach (var item in enumerable)
            {
                action(item);
            }
            return enumerable!;
        }
    }
}
