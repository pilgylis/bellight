using Bellight.Core;

namespace SimpleIoC;

public interface IService : ITransientDependency
{
    void DoSomething();
}