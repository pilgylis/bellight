namespace Bellight.Core;

public interface IKeyedServiceRegistry
{
    void Add(string key, Type type, ServiceLifetime lifetime);

    bool ContainsKey(string key);

    void Clear();

    IDictionary<string, (Type, ServiceLifetime)> GetDictionary();
}