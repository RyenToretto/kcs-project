using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: 指标心智——Counter / Histogram 的内存实现（对照 Prometheus/OpenTelemetry Metrics）
[难度]: Medium
[前置]: 12_observability/00_logging

一、主题说明
可观测性第二支柱：**指标**是带标签的时间序列（请求数、延迟分位）。
本课实现 Counter 累加与 Histogram 分桶，理解 RED/USE 方法里「Rate、Errors、Duration」从哪来。

二、写法对比
1. 直观版：单个 long 计数器。
2. 进阶版：带 label 的 Counter + 简单直方图分桶。

三、真实用途
- System.Diagnostics.Metrics、Prometheus exporter、Application Insights。
资料: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
      https://opentelemetry.io/docs/languages/net/
*/

/** 直观版：无标签计数。 */
sealed class NaiveCounter
{
    private long _value;

    public void Inc() => _value++;
    public long Value => _value;
}

/** 进阶版：Counter + Histogram（固定桶边界毫秒）。 */
sealed class MetricsRegistry
{
    private readonly Dictionary<string, long> _counters = new();
    private readonly Dictionary<string, List<double>> _histograms = new();
    private static readonly double[] LatencyBucketsMs = [5, 10, 25, 50, 100, 250, 500];

    static string Key(string name, string? label) => label is null ? name : $"{name}{{{label}}}";

    public void CounterInc(string name, string? label = null, long delta = 1)
    {
        var key = Key(name, label);
        _counters.TryGetValue(key, out var cur);
        _counters[key] = cur + delta;
    }

    public long CounterGet(string name, string? label = null) =>
        _counters.GetValueOrDefault(Key(name, label));

    public void HistogramObserve(string name, double valueMs, string? label = null)
    {
        var key = Key(name, label);
        if (!_histograms.ContainsKey(key)) _histograms[key] = [];
        _histograms[key].Add(valueMs);
    }

  public int HistogramBucketLeCount(string name, double leMs, string? label = null)
    {
        var key = Key(name, label);
        if (!_histograms.TryGetValue(key, out var obs)) return 0;
        return obs.Count(v => v <= leMs);
    }

    public double HistogramSum(string name, string? label = null)
    {
        var key = Key(name, label);
        return _histograms.TryGetValue(key, out var obs) ? obs.Sum() : 0;
    }
}

static class OrderApiSimulator
{
    public static void HandleRequest(MetricsRegistry m, string route, double latencyMs, bool success)
    {
        m.CounterInc("http_requests_total", $"route={route},status={(success ? "200" : "500")}");
        m.HistogramObserve("http_request_duration_ms", latencyMs, $"route={route}");
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveCounter();
        naive.Inc(); naive.Inc();
        if (naive.Value != 2) throw new Exception("NaiveCounter");

        var reg = new MetricsRegistry();
        OrderApiSimulator.HandleRequest(reg, "/orders", 12.5, success: true);
        OrderApiSimulator.HandleRequest(reg, "/orders", 80, success: true);
        OrderApiSimulator.HandleRequest(reg, "/orders", 3, success: false);

        var ok = reg.CounterGet("http_requests_total", "route=/orders,status=200");
        if (ok != 2) throw new Exception($"200 计数应为 2，得 {ok}");

        var err = reg.CounterGet("http_requests_total", "route=/orders,status=500");
        if (err != 1) throw new Exception("500 计数应为 1");

        var le50 = reg.HistogramBucketLeCount("http_request_duration_ms", 50, "route=/orders");
        if (le50 != 2) throw new Exception("≤50ms 应有 2 次观测");

        if (reg.HistogramSum("http_request_duration_ms", "route=/orders") < 90)
            throw new Exception("直方图 sum 异常");

        Console.WriteLine("✅ MetricsMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 指标心智 ************\n");

        var reg = new MetricsRegistry();
        for (var i = 0; i < 5; i++)
            OrderApiSimulator.HandleRequest(reg, "/health", i * 10, true);

        Console.WriteLine($"/health 200 次数: {reg.CounterGet("http_requests_total", "route=/health,status=200")}");
        Console.WriteLine($"≤25ms 桶内样本: {reg.HistogramBucketLeCount("http_request_duration_ms", 25, "route=/health")}");

        RunTests();
    }

    static void Main() => Bootstrap();
}