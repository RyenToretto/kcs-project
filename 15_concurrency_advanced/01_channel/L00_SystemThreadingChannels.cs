using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

/*
[主题]: System.Threading.Channels——生产者/消费者背压
[难度]: Hard
[前置]: 04_async_await

一、主题说明
Channel 解耦生产与消费，Bounded 选项提供背压。比 lock+Queue 更清晰，比 BlockingCollection 更 async 友好。

二、写法对比
1. 直观版：List 一次性装入内存。
2. 进阶版：Channel.CreateBounded + 多读者单写者。

三、真实用途
- 日志管道、工作队列、与 IAsyncEnumerable 组合。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.manning.com/books/csharp-concurrency
*/

static class Program
{
    static async Task<int> NaiveAccumulateAsync(int count)
    {
        var list = new System.Collections.Generic.List<int>();
        for (var i = 0; i < count; i++) list.Add(i);
        await Task.Yield();
        return list.Sum();
    }

    static async Task<int> ChannelPipelineAsync(int count, int capacity)
    {
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var writer = channel.Writer;
        var reader = channel.Reader;

        var producer = Task.Run(async () =>
        {
            for (var i = 0; i < count; i++)
            {
                await writer.WriteAsync(i);
            }
            writer.Complete();
        });

        var sum = 0;
        await foreach (var item in reader.ReadAllAsync())
            sum += item;

        await producer;
        return sum;
    }

    static void RunTests()
    {
        var naive = NaiveAccumulateAsync(100).Result;
        if (naive != 4950) throw new Exception("朴素求和错");

        var better = ChannelPipelineAsync(100, capacity: 4).Result;
        if (better != 4950) throw new Exception("Channel 管道求和错");

        // 有界 channel 背压：容量为 1 时仍可完成
        var tight = ChannelPipelineAsync(50, capacity: 1).Result;
        if (tight != 1225) throw new Exception("有界 Channel 失败");

        Console.WriteLine("✅ SystemThreadingChannels 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Channels ************\n");
        Console.WriteLine("⚠ 生产：BoundedChannelFullMode.DropWrite 要谨慎，易丢数据。");
        RunTests();
    }

    static void Main() => Bootstrap();
}