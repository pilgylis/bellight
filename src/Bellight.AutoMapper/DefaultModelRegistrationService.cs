namespace Bellight.AutoMapper;

public class DefaultModelRegistrationService : IModelRegistrationService
{
    private readonly List<Tuple<Type, Type>> _mappings = [];
    private readonly List<Type> _mappingProfiles = [];

    public void AddMapping(Type sourceType, Type destinationType)
    {
        _mappings.Add(new Tuple<Type, Type>(sourceType, destinationType));
    }

    public void AddProfile(Type profileType)
    {
        _mappingProfiles.Add(profileType);
    }

    public IEnumerable<Tuple<Type, Type>> GetAllMappings()
    {
        return _mappings;
    }

    public IEnumerable<Type> GetAllProfiles()
    {
        return _mappingProfiles;
    }
}