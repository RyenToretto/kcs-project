using System;
using System.Threading;

/*
[主题]: 同步原语——lock、Monitor、SemaphoreSlim、ReaderWriterLockSlim
[难度]: Hard
[前置]: 04_async_await

一、主题说明
lock 是 Monitor 语法糖；异步等待用 SemaphoreSlim.WaitAsync，别 Semaphore.WaitOne 堵线程池。
读写锁适合读多写少。

二、写法对比
1. 直观版：裸锁对象 + 长临界区。
2. 进阶版：私有 readonly 锁对象、最小临界区、ReaderWriterLockSlim 读共享。

三、真实用途
- 缓存字典、限流；避免 lock(this) 与 lock(typeof)。
资料: https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
      https://www.manning.com/books/csharp-concurrency
*/

sealed class NaiveCounter
{
    private readonly object _sync = new();
    private int _value;

    public void Inc()
    {
        lock (_sync) { _value++; /* 故意扩大临界区示例——生产应最小化 */ Thread.Sleep(0); }
    }

    public int Value => _value;
}

sealed class BetterCounter
{
    private readonly object _gate = new();
    private int _value;

    public void Inc()
    {
        lock (_gate) { _value++; }
    }

    public int Value { get { lock (_gate) return _value; } }
}

sealed class ReadMostlyCache
{
    private readonly ReaderWriterLockSlim _rw = new();
    private readonly System.Collections.Generic.Dictionary<string, string> _data = new();

    public string? Get(string key)
    {
        _rw.EnterReadLock();
        try { return _data.TryGetValue(key, out var v) ? v : null; }
        finally { _rw.ExitReadLock(); }
    }

    public void Set(string key, string value)
    {
        _rw.EnterWriteLock();
        try { _data[key] = value; }
        finally { _rw.ExitWriteLock(); }
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveCounter();
        Parallel.For(0, 500, _ => naive.Inc());
        if (naive.Value != 500) throw new Exception("Naive 计数");

        var better = new BetterCounter();
        Parallel.For(0, 500, _ => better.Inc());
        if (better.Value != 500) throw new Exception("Better 计数");

        var cache = new ReadMostlyCache();
        cache.Set("k", "v");
        if (cache.Get("k") != "v") throw new Exception("读写锁缓存");

        using var sem = new SemaphoreSlim(2, 2);
        var entered = 0;
        var tasks = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
        {
            await sem.WaitAsync();
            Interlocked.Increment(ref entered);
            await Task.Delay(10);
            sem.Release();
        })).ToArray();
        Task.WaitAll(tasks);
        if (entered != 4) throw new Exception("SemaphoreSlim");

        Console.WriteLine("✅ Synchronization 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Synchronization ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}