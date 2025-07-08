namespace Bellight.Core.Defaults;

public class KeyedServiceFactory(IDictionary<string, (Type, ServiceLifetime)> keyedTypeDictionary, IServiceProvider serviceProvider) : IKeyedServiceFactory
{
    private readonly IDictionary<string, (Type, ServiceLifetime)> _keyedTypeDictionary = keyedTypeDictionary;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public T Resolve<T>(string name)
    {
        if (!_keyedTypeDictionary.ContainsKey(name))
        {
            return default!;
        }

        var (type, _) = _keyedTypeDictionary[name];
        return (T)_serviceProvider.GetService(type)!;
    }
}