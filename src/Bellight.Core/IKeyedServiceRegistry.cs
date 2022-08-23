namespace Bellight.Core;

public interface IKeyedServiceRegistry
{
    void Add(string key, Type type);
    bool ContainsKey(string key);
    void Clear();
    IDictionary<string, Type> GetDictionary();
}
