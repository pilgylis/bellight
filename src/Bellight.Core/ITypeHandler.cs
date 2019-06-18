using Bellight.Core.DependencyCache;
using System;
using System.Collections.Generic;

namespace Bellight.Core
{
    public interface ITypeHandler
    {
        void Process(Type type);
        void LoadCache(IEnumerable<TypeHandlerCacheSection> sections);
        IEnumerable<TypeHandlerCacheSection> SaveCache();
    }
}
