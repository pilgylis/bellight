using AutoMapper;
using System.Globalization;

namespace Bellight.AutoMapper.Converters;

public class StringToNullableDateTimeConverter : ITypeConverter<string, DateTime?>
{
    public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        return DateTime.ParseExact(source, Constants.StandardDatetimeFormat, CultureInfo.InvariantCulture);
    }
}