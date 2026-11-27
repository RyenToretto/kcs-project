using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/*
[主题]: 并发陷阱——async void、ConfigureAwait、死锁、线程池饥饿
[难度]: Hard
[前置]: 04_async_await、本阶段前几课

一、主题说明
async void 无法被 await/捕获异常；UI/ASP.NET 上下文死锁 .Result/.Wait；
同步阻塞耗尽线程池 → 全局变慢。

二、写法对比
1. 直观版：button_Click async void；GetResult().Wait()。
2. 进阶版：async Task、ConfigureAwait(false) 在库代码、纯异步链。

三、真实用途
- 库作者默认 ConfigureAwait(false)；应用入口用 Task 非 void。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.manning.com/books/csharp-concurrency
*/

static class Program
{
    static async Task<int> ComputeAsync()
    {
        await Task.Delay(5).ConfigureAwait(false);
        return 7;
    }

    /** 陷阱：在「捕获同步上下文」的环境里 .Result 可能死锁；此处用线程池隔离演示。 */
    static int NaiveGetResult()
    {
        return ComputeAsync().Result; // 在线程池调用时通常不死锁，但仍反模式
    }

    static async Task<int> BetterAsync()
    {
        return await ComputeAsync().ConfigureAwait(false);
    }

    static void RunTests()
    {
        if (NaiveGetResult() != 7) throw new Exception("Result");

        if (BetterAsync().Result != 7) throw new Exception("Better async");

        // 线程池饥饿模拟：大量 Task.Run + 同步 Wait
        var bag = new List<Task<int>>();
        for (var i = 0; i < 20; i++)
        {
            bag.Add(Task.Run(() =>
            {
                Thread.Sleep(5);
                return 1;
            }));
        }
        if (Task.WhenAll(bag).Result.Length != 20) throw new Exception("WhenAll");

        // volatile / Interlocked 可见性
        var flag = 0;
        var t = new Thread(() => { Interlocked.Exchange(ref flag, 1); });
        t.Start();
        t.Join();
        if (Volatile.Read(ref flag) != 1) throw new Exception("可见性");

        Console.WriteLine("✅ ConcurrencyTraps 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Concurrency Traps ************\n");
        Console.WriteLine("⚠ 禁止库代码 async void；ASP.NET Core 无 SyncContext 但仍勿 .Wait()。");
        RunTests();
    }

    static void Main() => Bootstrap();
}