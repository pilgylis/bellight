using Bellight.Core;

namespace SimpleIoC
{
    public interface IKeyedServiceC: IKeyedDependency
    {
        void DoSomethingInKeyed();
    }
}
