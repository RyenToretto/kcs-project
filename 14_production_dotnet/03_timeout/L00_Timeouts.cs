using System;
using System.Threading;
using System.Threading.Tasks;

/*
[主题]: 超时与取消——CancellationToken 贯穿调用链
[难度]: Hard
[前置]: 04_async_await

一、主题说明
超时不是「多等一会」：要用 CTS 链接超时 Token，并传到 HttpClient/SQL。
陷阱：只设 HttpClient.Timeout 但不取消下游，连接仍占用。

二、写法对比
1. 直观版：Task.Delay(60000) 盲等。
2. 进阶版：CancelAfter + 链式 Token + OperationCanceledException 区分用户取消与超时。

三、真实用途
- HttpClient、EF、gRPC 均支持 CancellationToken；K8s 终止 Pod 会发 SIGTERM。
资料: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
      https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
*/

static class Program
{
    static async Task<int> NaiveWaitAsync(int ms)
    {
        await Task.Delay(ms);
        return 42;
    }

    static async Task<int> BetterWaitAsync(int ms, CancellationToken ct)
    {
        await Task.Delay(ms, ct);
        return 42;
    }

    static async Task<int> CallWithTimeoutAsync(
        Func<CancellationToken, Task<int>> work,
        int timeoutMs)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        return await work(cts.Token);
    }

    static void RunTests()
    {
        var fast = CallWithTimeoutAsync(ct => BetterWaitAsync(10, ct), 500);
        if (fast.Result != 42) throw new Exception("短任务应成功");

        try
        {
            var slow = CallWithTimeoutAsync(ct => BetterWaitAsync(2000, ct), 50);
            slow.Wait();
            throw new Exception("应超时");
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException) { }

        using var parent = new CancellationTokenSource(30);
        try
        {
            BetterWaitAsync(500, parent.Token).Wait();
            throw new Exception("父取消应中断");
        }
        catch (AggregateException ae) when (ae.InnerException is OperationCanceledException) { }

        Console.WriteLine("✅ Timeouts 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Timeouts ************\n");
        Console.WriteLine("⚠ 生产：Polly 超时策略 + 每出站请求传递 RequestAborted。");
        RunTests();
    }

    static void Main() => Bootstrap();
}