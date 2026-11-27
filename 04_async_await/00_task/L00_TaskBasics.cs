using System;
using System.Threading.Tasks;

/*
[主题]: Task 基础 —— 表示「将来完成的工作」
[难度]: Medium
[前置]: 03_collections_generics/00_list_dict

一、主题说明
Task / Task<T> 封装异步操作的状态与结果。可以 await，也可以用 ContinueWith（旧风格）。
本课用 Task.Delay 模拟 I/O，全程离线。

二、写法对比
1. 直观版：Thread.Sleep 阻塞当前线程（占用线程池，后端应避免）。
2. 进阶版：Task.Delay + await，等待时不占着线程干活。

三、真实用途
- 调用 HTTP、数据库、文件：返回 Task，释放线程处理其他请求。
- 组合：Task.WhenAll 并行等多个 I/O。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
      https://www.manning.com/books/csharp-concurrency
*/
static class Program
{
    /** 模拟慢速 I/O：返回已完成整数。 */
    static async Task<int> FetchScoreAsync(int id)
    {
        await Task.Delay(5);
        return id * 10;
    }

    /** 直观版：同步等待（阻塞）。 */
    static int FetchScoreBlocking(int id)
    {
        Task.Delay(5).Wait();
        return id * 10;
    }

    /** 组合两个 Task 的结果。 */
    static async Task<int> SumTwoScoresAsync(int a, int b)
    {
        var t1 = FetchScoreAsync(a);
        var t2 = FetchScoreAsync(b);
        await Task.WhenAll(t1, t2);
        return await t1 + await t2;
    }

    static async Task RunTestsAsync()
    {
        if (FetchScoreBlocking(3) != 30) throw new Exception("Blocking 分数");

        var score = await FetchScoreAsync(4);
        if (score != 40) throw new Exception("FetchScoreAsync");

        var sum = await SumTwoScoresAsync(2, 5);
        if (sum != 70) throw new Exception("WhenAll 求和");

        var completed = Task.FromResult(99);
        if (completed.IsCompletedSuccessfully == false || completed.Result != 99)
        {
            throw new Exception("FromResult");
        }

        Console.WriteLine("✅ TaskBasics 所有测试用例通过！");
    }

    static async Task BootstrapAsync()
    {
        Console.WriteLine("\n\n************ L00 TaskBasics ************\n");
        var s = await FetchScoreAsync(7);
        Console.WriteLine($"异步得分 = {s}");
        await RunTestsAsync();
    }

    static async Task Main() => await BootstrapAsync();
}