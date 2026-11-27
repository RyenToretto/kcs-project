using System;
using System.Collections.Generic;
/*
[主题]: 中间件管道——洋葱模型（先进后出）
[难度]: Medium
[前置]: 06_aspnet_core/00_minimal

一、主题说明
每个中间件可改写请求/响应，并决定是否调用「下一个」委托；像洋葱一样一层层包起来。
本课用 Func<Request,Next,Response> 链模拟，无需真实 HTTP 服务器。

二、写法对比
1. 直观版：单个日志中间件包着固定 Handler。
2. 进阶版：可注册多段中间件，Use() 构建管道。

三、真实用途
- 认证、鉴权、异常处理、请求日志、CORS 都是中间件。
资料: https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core
      https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
      https://github.com/dotnet/aspnetcore
*/

record MiniRequest(string Path);
record MiniResponse(int Status, string Body, List<string> Headers);

delegate MiniResponse RequestDelegate(MiniRequest request);

/** 直观版：手写一层 logging 包裹。 */
static class NaiveLogging
{
    public static MiniResponse Handle(MiniRequest req)
    {
        Console.WriteLine($"[naive-in] {req.Path}");
        var res = new MiniResponse(200, $"ok:{req.Path}", new List<string> { "X-Naive:1" });
        Console.WriteLine($"[naive-out] {res.Status}");
        return res;
    }
}

/** 进阶版：可组合的管道构建器。 */
sealed class MiddlewarePipeline
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new();

    public void Use(Func<RequestDelegate, RequestDelegate> middleware) => _middlewares.Add(middleware);

    public RequestDelegate Build(RequestDelegate terminal)
    {
        RequestDelegate next = terminal;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
            next = _middlewares[i](next);
        return next;
    }
}

static class Program
{
    static Func<RequestDelegate, RequestDelegate> TimingMiddleware() =>
        next => request =>
        {
            var start = Environment.TickCount64;
            var response = next(request);
            response.Headers.Add($"X-ElapsedMs:{Environment.TickCount64 - start}");
            return response;
        };

    static Func<RequestDelegate, RequestDelegate> AuthMiddleware(bool requireToken) =>
        next => request =>
        {
            if (requireToken && !request.Path.Contains("token=", StringComparison.Ordinal))
                return new MiniResponse(401, "unauthorized", new List<string>());
            return next(request);
        };

    static void RunTests()
    {
        _ = NaiveLogging.Handle(new MiniRequest("/ping"));

        var pipeline = new MiddlewarePipeline();
        pipeline.Use(TimingMiddleware());
        pipeline.Use(AuthMiddleware(requireToken: true));

        var app = pipeline.Build(req => new MiniResponse(200, $"terminal:{req.Path}", new List<string>()));

        var ok = app(new MiniRequest("/api?token=abc"));
        if (ok.Status != 200 || !ok.Body.Contains("terminal")) throw new Exception("合法请求应到达终端");

        var deny = app(new MiniRequest("/api"));
        if (deny.Status != 401) throw new Exception("无 token 应 401");

        if (!ok.Headers.Exists(h => h.StartsWith("X-ElapsedMs:", StringComparison.Ordinal)))
            throw new Exception("Timing 中间件应写入耗时头");

        Console.WriteLine("✅ MiddlewarePipeline 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Middleware Pipeline ************\n");
        Console.WriteLine("⚠ 离线管道模型；真实 ASP.NET Core 用 RequestDelegate + IApplicationBuilder.");

        var pipeline = new MiddlewarePipeline();
        pipeline.Use(next => req =>
        {
            var res = next(req);
            res.Headers.Add("X-Demo: onion");
            return res;
        });
        var app = pipeline.Build(req => new MiniResponse(200, "core", new List<string>()));
        var res = app(new MiniRequest("/demo"));
        Console.WriteLine($"{res.Status} {res.Body} headers=[{string.Join(", ", res.Headers)}]");
        RunTests();
    }

    static void Main() => Bootstrap();
}