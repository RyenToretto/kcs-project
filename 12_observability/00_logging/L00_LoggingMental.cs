using System;
using System.Collections.Generic;
using System.Text;

/*
[主题]: 结构化日志心智——级别、模板、作用域（对照 ILogger）
[难度]: Easy
[前置]: 06_aspnet_core、05_dotnet_tooling

一、主题说明
生产排障靠**结构化日志**：固定字段（级别、类别、TraceId）+ 消息模板，便于 ELK/Loki 检索。
本课用内存收集器模拟 LogLevel 过滤与 scope 嵌套，不对接真实 Serilog/OpenTelemetry。

二、写法对比
1. 直观版：Console.WriteLine 拼接字符串。
2. 进阶版：LogEntry 记录 + 按级别过滤 + BeginScope 栈。

三、真实用途
- Microsoft.Extensions.Logging、Serilog、OpenTelemetry Logs。
资料: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging
      https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
*/

enum LogLevel { Trace, Debug, Information, Warning, Error, Critical }

record LogEntry(LogLevel Level, string Category, string Message, IReadOnlyDictionary<string, object>? Scope);

/** 直观版：无级别过滤的列表收集。 */
sealed class NaiveLogCollector
{
    public List<string> Lines { get; } = [];

    public void Write(string line) => Lines.Add(line);
}

/** 进阶版：模拟 ILogger 的核心行为。 */
sealed class StructuredLogger
{
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly List<LogEntry> _sink;
    private readonly Stack<Dictionary<string, object>> _scopes = new();

    public StructuredLogger(string category, LogLevel minLevel, List<LogEntry> sink)
    {
        _category = category;
        _minLevel = minLevel;
        _sink = sink;
    }

    public IDisposable BeginScope(string key, object value)
    {
        var dict = new Dictionary<string, object> { [key] = value };
        _scopes.Push(dict);
        return new ScopeDisposable(this);
    }

    internal void PopScope() => _scopes.Pop();

    public void Log(LogLevel level, string message)
    {
        if (level < _minLevel) return;

        Dictionary<string, object>? merged = null;
        foreach (var s in _scopes)
        {
            merged ??= new Dictionary<string, object>();
            foreach (var kv in s) merged[kv.Key] = kv.Value;
        }

        _sink.Add(new LogEntry(level, _category, message, merged));
    }

    sealed class ScopeDisposable(StructuredLogger parent) : IDisposable
    {
        public void Dispose() => parent.PopScope();
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveLogCollector();
        naive.Write("[INFO] started");
        if (naive.Lines.Count != 1) throw new Exception("naive 行数");

        var sink = new List<LogEntry>();
        var log = new StructuredLogger("OrderService", LogLevel.Information, sink);

        log.Log(LogLevel.Debug, "不应出现");
        log.Log(LogLevel.Information, "订单创建");

        using (log.BeginScope("OrderId", 42))
        {
            log.Log(LogLevel.Warning, "库存偏低");
        }

        if (sink.Count != 2) throw new Exception($"应 2 条日志，实际 {sink.Count}");
        if (sink[0].Level != LogLevel.Information) throw new Exception("第一条级别");
        var scope = sink[1].Scope;
        if (scope is null || !scope.TryGetValue("OrderId", out var oid) || (int)oid != 42)
            throw new Exception("scope 应含 OrderId=42");

        Console.WriteLine("✅ LoggingMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 结构化日志心智 ************\n");

        var sink = new List<LogEntry>();
        var log = new StructuredLogger("Demo", LogLevel.Debug, sink);
        using (log.BeginScope("RequestId", "abc-123"))
            log.Log(LogLevel.Information, "处理请求完成");

        var e = sink[0];
        Console.WriteLine($"[{e.Level}] {e.Category}: {e.Message} | Scope={e.Scope?["RequestId"]}");

        RunTests();
    }

    static void Main() => Bootstrap();
}