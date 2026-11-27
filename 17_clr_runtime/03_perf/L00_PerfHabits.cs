using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/*
[主题]: 性能习惯——先度量再优化、分配与算法
[难度]: Hard
[前置]: 本阶段前三课

一、主题说明
没有 profile 的优化是猜测。关注分配次数、锁竞争、算法复杂度。
陷阱：过早 StringBuilder、错误缓存反而更慢。

二、写法对比
1. 直观版：嵌套循环 O(n²) 去重。
2. 进阶版：HashSet O(n)；Stopwatch 对比（教学级）。

三、真实用途
- BenchmarkDotNet；生产用 Application Insights / OTel 指标。
资料: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
      https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa
*/

static class Program
{
    static int DistinctCountNaive(int[] data)
    {
        var list = new List<int>();
        foreach (var x in data)
            if (!list.Contains(x)) list.Add(x);
        return list.Count;
    }

    static int DistinctCountBetter(int[] data) => new HashSet<int>(data).Count;

    static void RunTests()
    {
        var data = new int[2000];
        for (var i = 0; i < data.Length; i++) data[i] = i % 50;

        var sw = Stopwatch.StartNew();
        var a = DistinctCountNaive(data);
        sw.Stop();
        var naiveMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var b = DistinctCountBetter(data);
        sw.Stop();
        var betterMs = sw.ElapsedMilliseconds;

        if (a != b || a != 50) throw new Exception("去重计数");
        if (betterMs > naiveMs + 5) throw new Exception("更好算法应不慢于朴素（容差）");

        // 少分配：复用 StringBuilder
        var sb = new StringBuilder();
        for (var i = 0; i < 100; i++) sb.Append('a');
        if (sb.Length != 100) throw new Exception("StringBuilder");

        Console.WriteLine("✅ PerfHabits 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Perf Habits ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}