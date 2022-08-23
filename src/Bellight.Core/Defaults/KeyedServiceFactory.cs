namespace Bellight.Core.Defaults;

public class KeyedServiceFactory : IKeyedServiceFactory
{
    private readonly IDictionary<string, Type> _keyedTypeDictionary;
    private readonly IServiceProvider _serviceProvider;

    public KeyedServiceFactory(IDictionary<string, Type> keyedTypeDictionary, IServiceProvider serviceProvider)
    {
        _keyedTypeDictionary = keyedTypeDictionary;
        _serviceProvider = serviceProvider;
    }

    public T Resolve<T>(string name)
    {
        if (!_keyedTypeDictionary.ContainsKey(name))
        {
            return default!;
        }

        var type = _keyedTypeDictionary[name];
        return (T)_serviceProvider.GetService(type)!;
    }
}
