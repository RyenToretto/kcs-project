using System;
using System.Collections.Generic;

/*
[主题]: 接口（interface）——约定能力，不绑死实现
[难度]: Easy
[前置]: 01_oop_csharp/01_inheritance

一、主题说明
接口只描述「能做什么」，不规定「怎么做」。依赖接口而非具体类，是 DI、可测试性的基础。
C# 接口可含默认实现（default interface methods），入门先掌握纯契约写法。

二、写法对比
1. 直观版：一个接口 + 一个实现。
2. 进阶版：同一接口多个实现，业务代码只依赖接口（便于假实现/单测）。

三、真实用途
- IRepository / IEmailSender / IClock：ASP.NET Core DI 天天用。
- 本仓库后续 EF / Redis / MQ 课都会用接口假实现做离线自测。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface
      https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
*/

interface INotifier
{
    string Notify(string userId, string message);
}

/** 直观版：控制台通知。 */
sealed class ConsoleNotifier : INotifier
{
    public string Notify(string userId, string message) => $"console:{userId}:{message}";
}

/** 进阶版：内存记录，便于测试断言。 */
sealed class MemoryNotifier : INotifier
{
    public List<string> Sent { get; } = new();

    public string Notify(string userId, string message)
    {
        var line = $"memory:{userId}:{message}";
        Sent.Add(line);
        return line;
    }
}

/** 业务只依赖接口。 */
sealed class OrderService
{
    private readonly INotifier _notifier;

    public OrderService(INotifier notifier) => _notifier = notifier;

    public string PlaceOrder(string userId, string orderId) =>
        _notifier.Notify(userId, $"order-placed:{orderId}");
}

static class Program
{
    static void RunTests()
    {
        INotifier console = new ConsoleNotifier();
        if (console.Notify("u1", "hi") != "console:u1:hi") throw new Exception("ConsoleNotifier 失败");

        var memory = new MemoryNotifier();
        var svc = new OrderService(memory);
        var result = svc.PlaceOrder("u2", "o9");
        if (result != "memory:u2:order-placed:o9") throw new Exception("OrderService 失败");
        if (memory.Sent.Count != 1) throw new Exception("应记录 1 条通知");
        Console.WriteLine("✅ Interfaces 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Interfaces ************\n");
        var svc = new OrderService(new ConsoleNotifier());
        Console.WriteLine(svc.PlaceOrder("ada", "ord-1"));
        RunTests();
    }

    static void Main() => Bootstrap();
}
