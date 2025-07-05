using Bellight.Core;
using System;

namespace SimpleIoC;

[KeyedService("C1")]
public class KeyedServiceC1 : IKeyedServiceC
{
    public void DoSomethingInKeyed()
    {
        Console.WriteLine("IKeyedServiceC - KeyedServiceC1 - DoSomethingInKeyed()");
    }
}