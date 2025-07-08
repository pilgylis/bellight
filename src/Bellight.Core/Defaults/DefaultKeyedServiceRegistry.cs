namespace Bellight.Core.Defaults;

public class DefaultKeyedServiceRegistry : IKeyedServiceRegistry
{
    private readonly IDictionary<string, (Type, ServiceLifetime)> _dictionary = new Dictionary<string, (Type, ServiceLifetime)>();

    public void Add(string key, Type type, ServiceLifetime lifetime)
    {
        if (_dictionary.ContainsKey(key))
        {
            return;
        }

        _dictionary.Add(key, (type, lifetime));
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public IDictionary<string, (Type, ServiceLifetime)> GetDictionary()
    {
        return _dictionary;
    }
}