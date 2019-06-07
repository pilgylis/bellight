using System.Collections.Generic;
using System.Reflection;

namespace Bellight.Core
{
    public interface IAssemblyScanner
    {
        void Scan(IEnumerable<Assembly> additionalAssemblies);
    }
}
