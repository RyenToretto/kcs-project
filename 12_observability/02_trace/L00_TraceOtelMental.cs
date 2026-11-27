using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/*
[主题]: 分布式追踪心智——TraceId/SpanId 父子关系（对照 OpenTelemetry）
[难度]: Medium
[前置]: 12_observability/00_logging、04_async_await/00_task

一、主题说明
追踪把一次请求穿越多个服务的路径串成**一棵树**：根 Span 下挂数据库、HTTP 子调用。
W3C traceparent 头携带 trace-id；本课用内存 Activity 栈模拟，不对接 OTLP 导出器。

二、写法对比
1. 直观版：单条调用记录字符串。
2. 进阶版：MiniTracer 生成 Span 树，断言父子链接与耗时。

三、真实用途
- System.Diagnostics.ActivitySource、OpenTelemetry .NET SDK、Jaeger/Tempo。
资料: https://opentelemetry.io/docs/languages/net/
      https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
*/

record SpanRecord(
    string TraceId,
    string SpanId,
    string? ParentSpanId,
    string Operation,
    long DurationMs);

/** 直观版：扁平操作列表。 */
sealed class NaiveCallLog
{
    public List<string> Operations { get; } = [];

    public void Record(string op) => Operations.Add(op);
}

/** 进阶版：活动栈 + 导出 Span 列表。 */
sealed class MiniTracer
{
    private readonly List<SpanRecord> _completed = [];
    private readonly Stack<(string SpanId, string Op, long StartTicks)> _stack = new();
    private readonly string _traceId = Guid.NewGuid().ToString("N")[..16];

    public IDisposable StartSpan(string operation)
    {
        var spanId = Guid.NewGuid().ToString("N")[..8];
        var parent = _stack.Count > 0 ? _stack.Peek().SpanId : null;
        var start = Stopwatch.GetTimestamp();
        _stack.Push((spanId, operation, start));
        return new SpanScope(this, spanId, parent, operation, start);
    }

    void EndSpan(string spanId, string? parentSpanId, string operation, long startTicks)
    {
        var end = Stopwatch.GetTimestamp();
        var ms = (end - startTicks) * 1000 / Stopwatch.Frequency;
        _completed.Add(new SpanRecord(_traceId, spanId, parentSpanId, operation, ms));
        if (_stack.Count > 0 && _stack.Peek().SpanId == spanId)
            _stack.Pop();
    }

    public IReadOnlyList<SpanRecord> Spans => _completed;

    sealed class SpanScope(MiniTracer tracer, string spanId, string? parent, string op, long start) : IDisposable
    {
        private readonly long _start = start;
        public void Dispose() => tracer.EndSpan(spanId, parent, op, _start);
    }

}

sealed class OrderWorkflow
{
    public static void Process(MiniTracer tracer)
    {
        using (tracer.StartSpan("HTTP POST /orders"))
        {
            using (tracer.StartSpan("sql.insert order"))
            {
                // 模拟极短 DB
            }
            using (tracer.StartSpan("http.call inventory"))
            {
            }
        }
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveCallLog();
        naive.Record("order");
        naive.Record("db");
        if (naive.Operations.Count != 2) throw new Exception("naive log");

        var tracer = new MiniTracer();
        OrderWorkflow.Process(tracer);
        var spans = tracer.Spans;

        if (spans.Count != 3) throw new Exception($"应有 3 个 span，得 {spans.Count}");

        var root = spans.FirstOrDefault(s => s.Operation == "HTTP POST /orders")
            ?? throw new Exception("缺根 span");
        if (root.ParentSpanId != null) throw new Exception("根 span 无父级");

        foreach (var childOp in new[] { "sql.insert order", "http.call inventory" })
        {
            var child = spans.FirstOrDefault(s => s.Operation == childOp)
                ?? throw new Exception($"缺 {childOp}");
            if (child.ParentSpanId != root.SpanId) throw new Exception($"{childOp} 父级应为根");
            if (child.TraceId != root.TraceId) throw new Exception("TraceId 应一致");
        }

        Console.WriteLine("✅ TraceOtelMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 分布式追踪心智 ************\n");

        var tracer = new MiniTracer();
        OrderWorkflow.Process(tracer);
        foreach (var s in tracer.Spans)
            Console.WriteLine($"  [{s.SpanId}] {s.Operation} parent={s.ParentSpanId ?? "—"} {s.DurationMs}ms");

        RunTests();
    }

    static void Main() => Bootstrap();
}