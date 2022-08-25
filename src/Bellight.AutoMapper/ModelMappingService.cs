using AutoMapper;
using Bellight.AutoMapper.Converters;

namespace Bellight.AutoMapper
{
    public class ModelMappingService : IModelMappingService
    {
        private IMapper _mapper;
        private readonly IModelRegistrationService _modelRegistrationService;

        public ModelMappingService(IModelRegistrationService modelRegistrationService, Action<IMapperConfigurationExpression> configAction)
        {
            _modelRegistrationService = modelRegistrationService;

            _mapper = Init(configAction);
        }

        public T Map<T>(object source)
            where T : class
        {
            return _mapper.Map<T>(source);
        }

        public object Map(object source, Type sourceType, Type destinationType)
        {
            return _mapper.Map(source, sourceType, destinationType);
        }

        private IMapper Init(Action<IMapperConfigurationExpression> configAction)
        {
            var profiles = _modelRegistrationService.GetAllProfiles();
            var mappings = _modelRegistrationService.GetAllMappings();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, DateTime>().ConvertUsing<StringToDateTimeConverter>();
                cfg.CreateMap<string, DateTime?>().ConvertUsing<StringToNullableDateTimeConverter>();
                cfg.CreateMap<DateTime, string>().ConvertUsing<DateTimeToStringConverter>();
                cfg.CreateMap<DateTimeOffset, string>().ConvertUsing<DateTimeOffsetToStringConverter>();

                if (configAction != null)
                {
                    configAction.Invoke(cfg);
                }

                if (profiles?.Any() == true)
                {
                    profiles.ForEach(cfg.AddProfile);
                }

                if (mappings?.Any() == true)
                {
                    foreach (var tuple in mappings)
                    {
                        cfg.CreateMap(tuple.Item1, tuple.Item2).IgnoreAllNonExisting(tuple.Item1, tuple.Item2);
                        cfg.CreateMap(tuple.Item2, tuple.Item1).IgnoreAllNonExisting(tuple.Item2, tuple.Item1);
                    }
                }
            });

            return config.CreateMapper();
        }
    }
}