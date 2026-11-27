using System;
using System.Threading;
using System.Threading.Tasks;

/*
[主题]: CancellationToken —— 协作式取消长任务
[难度]: Medium
[前置]: 04_async_await/01_async

一、主题说明
取消不是杀线程，而是通知「请尽快停下」：在循环/await 里检查 token。
客户端断开、用户点「停止」、超时应联动 CancellationTokenSource。

二、写法对比
1. 直观版：bool 标志位（难在所有 await 点传递）。
2. 进阶版：CancellationToken 贯穿调用链，ThrowIfCancellationRequested。

三、真实用途
- 批量导出：用户取消时停止读下一页。
- 宿主关闭：IHost 注入的 ApplicationStopping token。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
      https://www.manning.com/books/csharp-concurrency
*/
static class Program
{
    /** 模拟分页拉取，每页后检查取消。 */
    static async Task<int> SumPagesAsync(int pageCount, CancellationToken token)
    {
        int sum = 0;
        for (int page = 1; page <= pageCount; page++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(2, token);
            sum += page;
        }
        return sum;
    }

    /** 直观版：不响应取消，总是跑完（对照用）。 */
    static async Task<int> SumPagesNaiveAsync(int pageCount)
    {
        int sum = 0;
        for (int page = 1; page <= pageCount; page++)
        {
            await Task.Delay(2);
            sum += page;
        }
        return sum;
    }

    static async Task RunTestsAsync()
    {
        var full = await SumPagesNaiveAsync(5);
        if (full != 15) throw new Exception("Naive 求和应为 15");

        using var cts = new CancellationTokenSource();
        var ok = await SumPagesAsync(4, cts.Token);
        if (ok != 10) throw new Exception("完整分页求和");

        using var cancelEarly = new CancellationTokenSource();
        cancelEarly.CancelAfter(3);
        try
        {
            await SumPagesAsync(100, cancelEarly.Token);
            throw new Exception("应被取消");
        }
        catch (OperationCanceledException)
        {
            // 预期
        }

        Console.WriteLine("✅ CancellationToken 所有测试用例通过！");
    }

    static async Task BootstrapAsync()
    {
        Console.WriteLine("\n\n************ L00 CancellationToken ************\n");
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        try
        {
            await SumPagesAsync(30, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("演示：长时间任务在超时后被取消");
        }
        await RunTestsAsync();
    }

    static async Task Main() => await BootstrapAsync();
}