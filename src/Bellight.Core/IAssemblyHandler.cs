using System.Reflection;

namespace Bellight.Core;

public interface IAssemblyHandler
{
    void Process(Assembly assembly);
}
