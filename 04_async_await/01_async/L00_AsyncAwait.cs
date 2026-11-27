using System;
using System.Threading.Tasks;

/*
[主题]: async / await —— 用同步写法组织异步流程
[难度]: Medium
[前置]: 04_async_await/00_task

一、主题说明
`async` 方法里可以用 `await` 等待 Task；编译器生成状态机。
`await` 后代码在 I/O 完成后继续，不阻塞线程。

二、写法对比
1. 直观版：.Result / .Wait() 硬等（易死锁，见 pitfalls 课）。
2. 进阶版：一路 async/await，异常沿调用栈正常抛出。

三、真实用途
- ASP.NET Core 控制器/Minimal API：async Task<IResult>。
- 服务层编排：先查用户，再查订单，顺序 await。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
      https://www.manning.com/books/csharp-concurrency
*/
static class Program
{
    static async Task<string> LoadUserNameAsync(int userId)
    {
        await Task.Delay(3);
        return $"User-{userId}";
    }

    static async Task<int> LoadOrderCountAsync(int userId)
    {
        await Task.Delay(3);
        return userId % 5;
    }

    /** 直观版：同步拼结果（阻塞两个 Delay）。 */
    static string BuildSummaryBlocking(int userId)
    {
        var name = LoadUserNameAsync(userId).GetAwaiter().GetResult();
        var count = LoadOrderCountAsync(userId).GetAwaiter().GetResult();
        return $"{name} 有 {count} 单";
    }

    /** 进阶版：async 串联。 */
    static async Task<string> BuildSummaryBetterAsync(int userId)
    {
        var name = await LoadUserNameAsync(userId);
        var count = await LoadOrderCountAsync(userId);
        return $"{name} 有 {count} 单";
    }

    /** 并行进阶：同时发起再 await。 */
    static async Task<string> BuildSummaryParallelAsync(int userId)
    {
        var nameTask = LoadUserNameAsync(userId);
        var countTask = LoadOrderCountAsync(userId);
        await Task.WhenAll(nameTask, countTask);
        return $"{nameTask.Result} 有 {countTask.Result} 单";
    }

    static async Task RunTestsAsync()
    {
        var blocking = BuildSummaryBlocking(10);
        if (!blocking.Contains("User-10")) throw new Exception("Blocking 摘要");

        var better = await BuildSummaryBetterAsync(10);
        if (better != "User-10 有 0 单") throw new Exception("Better 摘要");

        var parallel = await BuildSummaryParallelAsync(11);
        if (parallel != "User-11 有 1 单") throw new Exception("Parallel 摘要");

        try
        {
            await ThrowingAsync();
            throw new Exception("应抛出异常");
        }
        catch (InvalidOperationException)
        {
            // 预期
        }

        Console.WriteLine("✅ AsyncAwait 所有测试用例通过！");
    }

    static async Task ThrowingAsync()
    {
        await Task.Delay(1);
        throw new InvalidOperationException("业务失败");
    }

    static async Task BootstrapAsync()
    {
        Console.WriteLine("\n\n************ L00 AsyncAwait ************\n");
        Console.WriteLine(await BuildSummaryBetterAsync(42));
        await RunTestsAsync();
    }

    static async Task Main() => await BootstrapAsync();
}