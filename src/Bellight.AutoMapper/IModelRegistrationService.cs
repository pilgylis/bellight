namespace Bellight.AutoMapper
{
    public interface IModelRegistrationService
    {
        void AddProfile(Type profileType);

        void AddMapping(Type sourceType, Type destinationType);

        IEnumerable<Type> GetAllProfiles();

        IEnumerable<Tuple<Type, Type>> GetAllMappings();
    }
}