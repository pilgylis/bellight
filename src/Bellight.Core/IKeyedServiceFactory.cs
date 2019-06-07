namespace Bellight.Core
{
    public interface IKeyedServiceFactory
    {
        T Resolve<T>(string name);
    }
}
