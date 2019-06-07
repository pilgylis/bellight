using System;

namespace Bellight.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class KeyedServiceAttribute : Attribute
    {
        public KeyedServiceAttribute() { }
        public KeyedServiceAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
