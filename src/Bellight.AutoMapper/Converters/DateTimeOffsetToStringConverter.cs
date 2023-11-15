using AutoMapper;
using System.Globalization;

namespace Bellight.AutoMapper.Converters;

public class DateTimeOffsetToStringConverter : ITypeConverter<DateTimeOffset, string>
{
    public string Convert(DateTimeOffset source, string destination, ResolutionContext context)
    {
        return source.ToString(Constants.StandardDatetimeFormat, CultureInfo.InvariantCulture);
    }
}