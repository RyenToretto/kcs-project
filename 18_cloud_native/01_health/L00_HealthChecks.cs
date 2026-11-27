using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/*
[主题]: 健康检查——Liveness vs Readiness
[难度]: Hard
[前置]: 06_aspnet_core、12_observability

一、主题说明
Liveness：进程是否死锁；Readiness：能否接流量（DB/依赖就绪）。
陷阱：把耗时外部依赖放进 Liveness 导致反复重启。

二、写法对比
1. 直观版：/health 永远 200。
2. 进阶版：分别注册检查、聚合状态 Unhealthy/Degraded/Healthy。

三、真实用途
- ASP.NET Core AddHealthChecks、Kubernetes probe。
资料: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
      https://learn.microsoft.com/en-us/dotnet/core/docker/introduction
*/

enum HealthStatus { Unhealthy, Degraded, Healthy }

record HealthReport(HealthStatus Status, string Description);

interface IHealthCheck
{
    string Name { get; }
    Task<HealthReport> CheckAsync(CancellationToken ct);
}

sealed class NaiveAlwaysOk : IHealthCheck
{
    public string Name => "naive";
    public Task<HealthReport> CheckAsync(CancellationToken ct) =>
        Task.FromResult(new HealthReport(HealthStatus.Healthy, "always"));
}

sealed class DbReadinessCheck : IHealthCheck
{
    private readonly Func<bool> _canConnect;
    public string Name => "db";
    public DbReadinessCheck(Func<bool> canConnect) => _canConnect = canConnect;
    public Task<HealthReport> CheckAsync(CancellationToken ct) =>
        Task.FromResult(_canConnect()
            ? new HealthReport(HealthStatus.Healthy, "db ok")
            : new HealthReport(HealthStatus.Unhealthy, "db down"));
}

sealed class HealthAggregator
{
    private readonly List<IHealthCheck> _checks = new();
    public void Add(IHealthCheck c) => _checks.Add(c);

    public async Task<HealthStatus> EvaluateAsync(bool readinessOnly)
    {
        var worst = HealthStatus.Healthy;
        foreach (var c in _checks)
        {
            if (readinessOnly && c.Name == "self") continue;
            var r = await c.CheckAsync(CancellationToken.None);
            if (r.Status == HealthStatus.Unhealthy) return HealthStatus.Unhealthy;
            if (r.Status == HealthStatus.Degraded && worst == HealthStatus.Healthy)
                worst = HealthStatus.Degraded;
        }
        return worst;
    }
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveAlwaysOk();

        var agg = new HealthAggregator();
        agg.Add(new DbReadinessCheck(() => true));
        if (agg.EvaluateAsync(readinessOnly: true).Result != HealthStatus.Healthy)
            throw new Exception("应健康");

        agg = new HealthAggregator();
        agg.Add(new DbReadinessCheck(() => false));
        if (agg.EvaluateAsync(readinessOnly: true).Result != HealthStatus.Unhealthy)
            throw new Exception("DB 挂应 Unhealthy");

        Console.WriteLine("✅ HealthChecks 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Health Checks ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}