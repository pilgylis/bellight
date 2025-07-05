using System;

namespace SimpleIoC;

public class ServiceA : IService
{
    public void DoSomething()
    {
        Console.WriteLine("ServiceA - DoSomething()");
    }
}