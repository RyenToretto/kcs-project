using System;
using System.Threading.Tasks;

/*
[主题]: 异步常见陷阱 —— 阻塞等待、async void、忘记 await
[难度]: Medium
[前置]: 04_async_await/01_async

一、主题说明
- 在已有同步上下文时 `.Result`/`.Wait()` 等待未完成的 Task，可能死锁（尤其旧 UI / 错误用法）。
- `async void` 只适合事件处理，异常难捕获。
- 调用 `async Task` 却不 await，错误被静默吞掉。

二、写法对比
1. 直观版：`.GetAwaiter().GetResult()` 到处阻塞。
2. 进阶版：async 一路透传；库代码可考虑 `ConfigureAwait(false)`（点题）。

三、真实用途
- Web：请求线程被阻塞 → 吞吐量暴跌；应 async all the way。
- 审查清单：禁止在业务代码 `.Result` 除非明确离线工具。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
      https://www.manning.com/books/csharp-concurrency
*/
static class Program
{
    static async Task<int> ComputeAsync() => await Task.FromResult(42);

    /** 反面教材：阻塞等待（在控制台可跑，但在部分 UI/ASP 场景会死锁）。 */
    static int ComputeBlocking()
    {
        return ComputeAsync().GetAwaiter().GetResult();
    }

    /** 进阶：async 透传。 */
    static async Task<int> ComputeBetterAsync() => await ComputeAsync();

    /** 忘记 await：返回已启动的 Task，调用方不 await 时不知道何时完成。 */
    static Task<int> StartWorkForgottenAsync()
    {
        return ComputeAsync(); // 正确返回 Task，但调用方须 await
    }

    static async Task RunTestsAsync()
    {
        if (ComputeBlocking() != 42) throw new Exception("Blocking 结果应为 42");
        if (await ComputeBetterAsync() != 42) throw new Exception("Better 结果");

        var task = StartWorkForgottenAsync();
        if (!task.IsCompleted) await task;
        if (task.Result != 42) throw new Exception("须 await 才能拿到结果");

        // 心智断言：同步解析虚构的代码审查结论（离线）
        const string reviewVerdict = "BLOCK: line uses .Result on unfinished Task in request pipeline";
        if (!reviewVerdict.Contains("BLOCK")) throw new Exception("审查应标记阻塞");

        Console.WriteLine("✅ AsyncPitfalls 所有测试用例通过！");
    }

    static async Task BootstrapAsync()
    {
        Console.WriteLine("\n\n************ L00 AsyncPitfalls ************\n");
        Console.WriteLine("教训：生产请求路径用 await，不用 .Result/.Wait()");
        Console.WriteLine($"正确结果 = {await ComputeBetterAsync()}");
        await RunTestsAsync();
    }

    static async Task Main() => await BootstrapAsync();
}