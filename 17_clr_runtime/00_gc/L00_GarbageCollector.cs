using System;
using System.Runtime;

/*
[主题]: GC 世代与分配压力——为何 Gen2 频繁 Full GC 是警报
[难度]: Hard
[前置]: 03_collections_generics、15_concurrency_advanced

一、主题说明
小对象进 Gen0；幸存晋升 Gen1/Gen2。LOH 大对象单独管理。
陷阱：事件处理器/静态字典长期引用短生命周期对象 → 内存泄漏式晋升。

二、写法对比
1. 直观版：循环 new 大 string 不闻不问。
2. 进阶版：复用 StringBuilder/ArrayPool；观察 GC.GetGeneration。

三、真实用途
- dotnet-counters、PerfView；服务端避免 per-request 大分配。
资料: https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/
      https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454
*/

static class Program
{
    static void NaiveAlloc(int rounds)
    {
        for (var i = 0; i < rounds; i++)
            _ = new string('x', 128);
    }

    static void BetterReuse(int rounds)
    {
        var buffer = new char[128];
        for (var i = 0; i < rounds; i++)
        {
            buffer.AsSpan().Fill('x');
            _ = new string(buffer);
        }
    }

    static void RunTests()
    {
        var before = GC.CollectionCount(2);
        NaiveAlloc(2000);
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        var afterNaive = GC.CollectionCount(2);

        var obj = new object();
        if (GC.GetGeneration(obj) != 0) throw new Exception("新对象应在 Gen0");

        BetterReuse(2000);
        if (afterNaive < before) throw new Exception("Gen2 计数应单调");

        // 模拟「可达性」：静态缓存持有引用会阻止回收（教学示意）
        LeakHolder.Hold(new byte[1024]);
        if (LeakHolder.Stored is null) throw new Exception("静态持有");

        Console.WriteLine("✅ GarbageCollector 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Garbage Collector ************\n");
        Console.WriteLine($"GC 总内存: {GC.GetTotalMemory(false)} bytes");
        RunTests();
    }

    static void Main() => Bootstrap();
}

static class LeakHolder
{
    private static byte[]? _stored;
    public static byte[]? Stored => _stored;
    public static void Hold(byte[] data) => _stored = data;
}