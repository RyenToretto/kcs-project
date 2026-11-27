using System;
using System.Collections.Generic;

/*
[主题]: SRE 清单——SLO、错误预算、值班与 Blameless Postmortem
[难度]: Hard
[前置]: 12_observability、14_production_dotnet

一、主题说明
SLI 度量用户体验；SLO 是目标；错误预算用尽应停更功能修可靠性。
陷阱：无 on-call 演练的告警形同虚设。

二、写法对比
1. 直观版：「尽量不宕机」。
2. 进阶版：窗口内可用率、燃尽错误预算、行动项追踪。

三、真实用途
- Google SRE 书；与 Prometheus/Alertmanager 联动。
资料: https://sre.google/books/
      https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
*/

record SloDefinition(string Name, double TargetAvailability, TimeSpan Window);

sealed class ErrorBudgetTracker
{
    public double AllowedUnavailabilityRatio => 1.0 - _slo.TargetAvailability;

    private readonly SloDefinition _slo;
    private double _consumedRatio;

    public ErrorBudgetTracker(SloDefinition slo) => _slo = slo;

    public void RecordOutage(TimeSpan outage)
    {
        var ratio = outage.TotalMinutes / _slo.Window.TotalMinutes;
        _consumedRatio += ratio;
    }

    public bool BudgetExhausted() => _consumedRatio >= AllowedUnavailabilityRatio;

    public double RemainingRatio() => Math.Max(0, AllowedUnavailabilityRatio - _consumedRatio);
}

sealed class Postmortem
{
    public required string IncidentId { get; init; }
    public required string RootCause { get; init; }
    public required IReadOnlyList<string> ActionItems { get; init; }
    public bool Blameless { get; init; } = true;
}

static class Program
{
    static void RunTests()
    {
        var slo = new SloDefinition("API", TargetAvailability: 0.999, Window: TimeSpan.FromDays(30));
        var budget = new ErrorBudgetTracker(slo);

        budget.RecordOutage(TimeSpan.FromMinutes(5));
        if (budget.BudgetExhausted()) throw new Exception("5 分钟不应耗尽月预算");

        var brutal = new ErrorBudgetTracker(slo);
        brutal.RecordOutage(TimeSpan.FromHours(2));
        if (!brutal.BudgetExhausted()) throw new Exception("2 小时应超 0.1% 月预算");

        var pm = new Postmortem
        {
            IncidentId = "INC-001",
            RootCause = "配置未覆盖导致连接池耗尽",
            ActionItems = new[] { "添加连接池指标告警", "Staging 压测纳入发布门禁" }
        };
        if (!pm.Blameless || pm.ActionItems.Count < 2) throw new Exception("Postmortem");

        Console.WriteLine("✅ SreChecklist 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 SRE Checklist ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}