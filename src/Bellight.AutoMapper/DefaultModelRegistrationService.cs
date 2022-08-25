namespace Bellight.AutoMapper
{
    public class DefaultModelRegistrationService : IModelRegistrationService
    {
        private readonly IList<Tuple<Type, Type>> _mappings = new List<Tuple<Type, Type>>();
        private readonly IList<Type> _mappingProfiles = new List<Type>();

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
}