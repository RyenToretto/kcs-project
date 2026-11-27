using System;
using System.Collections.Generic;

/*
[主题]: Minimal API 心智——路由表 + 处理函数字典
[难度]: Medium
[前置]: 05_dotnet_tooling、01_oop_csharp/02_interface

一、主题说明
ASP.NET Core Minimal API 把 HTTP 请求映射到 C# 委托；本课用「路由表 + 处理器字典」离线模拟，
不启动 Kestrel 真实服务器。学完再对照官方 Minimal API，概念一一对应。

二、写法对比
1. 直观版：path → string 的简单字典。
2. 进阶版：METHOD + path 复合键，带请求/响应上下文对象。

三、真实用途
- Web API、微服务入口、BFF 层都围绕「匹配路由 → 执行业务」。
资料: https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core
      https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
      https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
      https://github.com/dotnet/aspnetcore
*/

record HttpRequestContext(string Method, string Path, string? Body);

record HttpResponseContext(int Status, string Body);

/** 直观版：只按路径查找处理器。 */
sealed class NaiveRouteTable
{
    private readonly Dictionary<string, Func<string>> _getHandlers = new(StringComparer.OrdinalIgnoreCase);

    public void MapGet(string path, Func<string> handler) => _getHandlers[path] = handler;

    public string? InvokeGet(string path) =>
        _getHandlers.TryGetValue(path, out var h) ? h() : null;
}

/** 进阶版：METHOD + path，更接近 ASP.NET Core 路由键。 */
sealed class BetterMiniHost
{
    private readonly Dictionary<string, Func<HttpRequestContext, HttpResponseContext>> _routes = new();

    static string Key(string method, string path) => $"{method.ToUpperInvariant()} {path}";

    public void Map(string method, string path, Func<HttpRequestContext, HttpResponseContext> handler) =>
        _routes[Key(method, path)] = handler;

    public HttpResponseContext Handle(HttpRequestContext req)
    {
        if (_routes.TryGetValue(Key(req.Method, req.Path), out var handler))
            return handler(req);
        return new HttpResponseContext(404, $"not-found:{req.Method} {req.Path}");
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveRouteTable();
        naive.MapGet("/ping", () => "pong");
        if (naive.InvokeGet("/ping") != "pong") throw new Exception("Naive GET /ping 失败");
        if (naive.InvokeGet("/nope") != null) throw new Exception("未注册路由应返回 null");

        var app = new BetterMiniHost();
        app.Map("GET", "/health", _ => new HttpResponseContext(200, "ok"));
        app.Map("POST", "/orders", req => new HttpResponseContext(201, $"created:{req.Body}"));

        var health = app.Handle(new HttpRequestContext("GET", "/health", null));
        if (health.Status != 200 || health.Body != "ok") throw new Exception("GET /health 失败");

        var order = app.Handle(new HttpRequestContext("POST", "/orders", "o-1"));
        if (order.Status != 201 || order.Body != "created:o-1") throw new Exception("POST /orders 失败");

        var miss = app.Handle(new HttpRequestContext("DELETE", "/orders", null));
        if (miss.Status != 404) throw new Exception("404 失败");

        Console.WriteLine("✅ MinimalApiMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Minimal API 心智 ************\n");
        Console.WriteLine("⚠ 本课为离线心智模型，非真实 ASP.NET Core 宿主。");

        var app = new BetterMiniHost();
        app.Map("GET", "/hello", _ => new HttpResponseContext(200, "hello-minimal"));
        var res = app.Handle(new HttpRequestContext("GET", "/hello", null));
        Console.WriteLine($"模拟响应: {res.Status} {res.Body}");
        RunTests();
    }

    static void Main() => Bootstrap();
}