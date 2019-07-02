using System;

namespace Bellight.AutoMapper
{
    public interface IModelMappingService
    {
        T Map<T>(object source) where T : class;
        object Map(object source, Type sourceType, Type destinationType);
    }
}
