using Bellight.Core;
using System;

namespace SimpleIoC
{
    [KeyedService("C2")]
    public class KeyedServiceC2 : IKeyedServiceC
    {
        public void DoSomethingInKeyed()
        {
            Console.WriteLine("IKeyedServiceC - KeyedServiceC2 - DoSomethingInKeyed()");
        }
    }
}
