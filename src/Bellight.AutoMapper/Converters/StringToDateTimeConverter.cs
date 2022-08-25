using AutoMapper;
using System.Globalization;

namespace Bellight.AutoMapper.Converters
{
    public class StringToDateTimeConverter : ITypeConverter<string, DateTime>
    {
        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            // TODO: convert to UTC using timezone
            return DateTime.ParseExact(source, Constants.StandardDatetimeFormat, CultureInfo.InvariantCulture);
        }
    }
}