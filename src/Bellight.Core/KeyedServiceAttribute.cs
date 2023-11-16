namespace Bellight.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class KeyedServiceAttribute : Attribute
{
    public KeyedServiceAttribute()
    {
        Name = string.Empty;
    }

    public KeyedServiceAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
}