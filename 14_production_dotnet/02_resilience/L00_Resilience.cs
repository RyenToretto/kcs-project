using System;

/*
[主题]: 弹性策略——重试、指数退避与熔断器心智
[难度]: Hard
[前置]: 04_async_await

一、主题说明
下游抖动时：有限次重试 + 退避；持续失败则熔断，快速失败保护线程池。
陷阱：对非幂等 POST 无限重试造成重复下单。

二、写法对比
1. 直观版：while(true) 立即重试。
2. 进阶版：最大次数、退避、熔断打开后短路。

三、真实用途
- .NET 8+ Microsoft.Extensions.Http.Resilience / Polly 集成。
资料: https://learn.microsoft.com/en-us/dotnet/core/resilience/
      https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
*/

sealed class FakeRemote
{
    private int _calls;
    public int FailUntil { get; set; } = 2;

    public bool TryCall()
    {
        _calls++;
        return _calls > FailUntil;
    }

    public int CallCount => _calls;
}

/** 直观版：紧循环重试，无退避、无上限意识。 */
static class NaiveRetry
{
    public static void CallUntilSuccess(FakeRemote remote)
    {
        while (!remote.TryCall()) { /* 占满 CPU */ }
    }
}

enum CircuitState { Closed, Open, HalfOpen }

sealed class ResiliencePolicy
{
    public int MaxRetries { get; init; } = 3;
    public int FailureThreshold { get; init; } = 3;
    public int OpenTicks { get; init; } = 5;

    private int _consecutiveFailures;
    private CircuitState _state = CircuitState.Closed;
    private int _openAtTick;

    public bool Execute(Func<bool> action, int tick)
    {
        if (_state == CircuitState.Open)
        {
            if (tick - _openAtTick < OpenTicks)
                throw new InvalidOperationException("熔断打开，快速失败");
            _state = CircuitState.HalfOpen;
        }

        Exception? last = null;
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (action()) {
                    _consecutiveFailures = 0;
                    _state = CircuitState.Closed;
                    return true;
                }
                throw new Exception("下游返回失败");
            }
            catch (Exception ex)
            {
                last = ex;
                _consecutiveFailures++;
            }
        }

        if (_consecutiveFailures >= FailureThreshold)
        {
            _state = CircuitState.Open;
            _openAtTick = tick;
        }
        throw last ?? new Exception("执行失败");
    }
}

static class Program
{
    static void RunTests()
    {
        var remote = new FakeRemote { FailUntil = 2 };
        NaiveRetry.CallUntilSuccess(remote);
        if (remote.CallCount < 3) throw new Exception("Naive 应多次调用");

        remote = new FakeRemote { FailUntil = 1 };
        var policy = new ResiliencePolicy { MaxRetries = 2 };
        if (!policy.Execute(() => remote.TryCall(), tick: 0))
            throw new Exception("应在重试后成功");

        var alwaysFail = new FakeRemote { FailUntil = 100 };
        var breaker = new ResiliencePolicy { MaxRetries = 0, FailureThreshold = 2 };
        try { breaker.Execute(() => alwaysFail.TryCall(), 0); }
        catch { /* expected */ }
        try { breaker.Execute(() => alwaysFail.TryCall(), 1); }
        catch { /* expected */ }
        try
        {
            breaker.Execute(() => alwaysFail.TryCall(), 2);
            throw new Exception("熔断打开后应抛错");
        }
        catch (InvalidOperationException) { /* ok */ }

        Console.WriteLine("✅ Resilience 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Resilience ************\n");
        Console.WriteLine("⚠ 仅对幂等读/带幂等键写做重试；生产用 Resilience 扩展 + HttpClientFactory。");
        RunTests();
    }

    static void Main() => Bootstrap();
}