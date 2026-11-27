using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
[主题]: 诊断——Activity、EventSource、dotnet-trace 心智
[难度]: Hard
[前置]: 12_observability

一、主题说明
Activity 链式追踪；EventSource 低开销事件；生产用 dotnet-counters / trace。
陷阱：Console.WriteLine 打日志在高 QPS 下成为瓶颈。

二、写法对比
1. 直观版：到处 Debug.Print。
2. 进阶版：ActivitySource + 结构化字段模拟。

三、真实用途
- OpenTelemetry Activity 与 System.Diagnostics 对齐。
资料: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
      https://opentelemetry.io/docs/languages/net/
*/

static class Program
{
    static readonly ActivitySource Source = new("kcs.lesson.diag", "1.0");

    static void NaiveWork()
    {
        Console.WriteLine("start"); // 不适合生产热路径
    }

    static string BetterWork(string input)
    {
        using var activity = Source.StartActivity("BetterWork");
        activity?.SetTag("input.len", input.Length);
        var result = input.ToUpperInvariant();
        activity?.SetTag("output.len", result.Length);
        return result;
    }

    static void RunTests()
    {
        NaiveWork();

        var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "kcs.lesson.diag",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = a => _stopped.Add(a.OperationName)
        };
        ActivitySource.AddActivityListener(listener);

        var r = BetterWork("hi");
        if (r != "HI") throw new Exception("工作结果");

        if (!_stopped.Contains("BetterWork")) throw new Exception("应收到 Activity");

        Console.WriteLine("✅ Diagnostics 所有测试用例通过！");
    }

    static readonly List<string> _stopped = new();

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Diagnostics ************\n");
        Console.WriteLine(BetterWork("kcs"));
        RunTests();
    }

    static void Main() => Bootstrap();
}