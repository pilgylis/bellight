using System;

namespace SimpleIoC;

public class ServiceB : IService
{
    public void DoSomething()
    {
        Console.WriteLine("ServiceB - DoSomething()");
    }
}