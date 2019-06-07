namespace Bellight.IoC
{
    public interface IKeyedServiceFactory
    {
        T Resolve<T>(string name);
    }
}
