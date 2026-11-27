using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/*
[主题]: Parallel.For 与 PLINQ——何时用、何时别用
[难度]: Hard
[前置]: 04_async_await、03_collections_generics

一、主题说明
CPU 密集可并行；IO 密集用 async。陷阱：Parallel.For 里阻塞 async/共享可变状态。

二、写法对比
1. 直观版：foreach 单线程累加。
2. 进阶版：Parallel.For + 线程局部聚合；PLINQ AsParallel 带度限制。

三、真实用途
- 批量计算、图像处理；别在 ASP.NET 请求线程里无脑 Parallel。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
      https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
*/

static class Program
{
    static long SumNaive(int[] data)
    {
        long sum = 0;
        foreach (var x in data) sum += x;
        return sum;
    }

    static long SumParallel(int[] data)
    {
        long total = 0;
        var bag = new ConcurrentBag<long>();
        Parallel.ForEach(Partitioner.Create(0, data.Length), range =>
        {
            long local = 0;
            for (var i = range.Item1; i < range.Item2; i++)
                local += data[i];
            bag.Add(local);
        });
        foreach (var part in bag) total += part;
        return total;
    }

    static long SumPlinq(int[] data) =>
        data.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).Sum(x => (long)x);

    static void RunTests()
    {
        var data = Enumerable.Range(1, 10_000).ToArray();
        var expected = SumNaive(data);
        if (SumParallel(data) != expected) throw new Exception("Parallel 求和不一致");
        if (SumPlinq(data) != expected) throw new Exception("PLINQ 求和不一致");

        // 陷阱演示：Parallel 中共享自增（故意不用——测试用 Interlocked 安全版对比）
        var unsafeCounter = 0;
        Parallel.For(0, 1000, _ => Interlocked.Increment(ref unsafeCounter));
        if (unsafeCounter != 1000) throw new Exception("应使用线程安全累加");

        Console.WriteLine("✅ ParallelAndPlinq 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Parallel & PLINQ ************\n");
        Console.WriteLine($"处理器数: {Environment.ProcessorCount}");
        RunTests();
    }

    static void Main() => Bootstrap();
}