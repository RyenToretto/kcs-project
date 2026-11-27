using System;
using System.Buffers;
using System.Text;

/*
[主题]: Span<T> 与 Memory<T>——栈上切片、少分配
[难度]: Hard
[前置]: 03_collections_generics

一、主题说明
Span 是 ref struct，不能装箱、不能进 async。Memory 可存字段，配合 MemoryPool。
陷阱：对已回收数组仍持有 Span（use-after-free）在 unsafe 混用时出现。

二、写法对比
1. 直观版：Substring 反复分配。
2. 进阶版：ReadOnlySpan 解析、ArrayPool 租还。

三、真实用途
- Kestrel 管道、System.IO.Pipelines、高性能 JSON。
资料: https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/
*/

static class Program
{
    static int SumBytesNaive(byte[] data)
    {
        var sum = 0;
        for (var i = 0; i < data.Length; i++) sum += data[i];
        return sum;
    }

    static int SumBytesSpan(ReadOnlySpan<byte> data)
    {
        var sum = 0;
        foreach (var b in data) sum += b;
        return sum;
    }

    static int ParseIntNaive(string s) => int.Parse(s);

    static bool TryParseIntSpan(ReadOnlySpan<char> s, out int value) =>
        int.TryParse(s, out value);

    static void RunTests()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        if (SumBytesNaive(data) != SumBytesSpan(data)) throw new Exception("Span 求和");

        if (!TryParseIntSpan("42".AsSpan(), out var v) || v != 42)
            throw new Exception("Span 解析");

        var pool = ArrayPool<byte>.Shared;
        var rented = pool.Rent(256);
        try
        {
            rented.AsSpan(0, 10).Fill(7);
            if (rented[0] != 7) throw new Exception("ArrayPool");
        }
        finally
        {
            pool.Return(rented, clearArray: true);
        }

        Console.WriteLine("✅ SpanMemory 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Span & Memory ************\n");
        Console.WriteLine("⚠ async 方法参数用 ReadOnlyMemory<T> 而非 Span。");
        RunTests();
    }

    static void Main() => Bootstrap();
}