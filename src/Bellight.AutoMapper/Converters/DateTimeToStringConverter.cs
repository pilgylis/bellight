using AutoMapper;
using System.Globalization;

namespace Bellight.AutoMapper.Converters;

public class DateTimeToStringConverter : ITypeConverter<DateTime, string>
{
    public string Convert(DateTime source, string destination, ResolutionContext context)
    {
        // TODO: convert to UTC using timezone
        return source.ToString(Constants.StandardDatetimeFormat, CultureInfo.InvariantCulture);
    }
}