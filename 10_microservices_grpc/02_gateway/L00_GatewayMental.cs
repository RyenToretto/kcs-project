using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: API 网关心智——统一入口、路由转发、简单聚合
[难度]: Medium
[前置]: 10_microservices_grpc/00_split、06_aspnet_core/02_middleware

一、主题说明
客户端通常不直接打十几个微服务地址，而是经过 **API Gateway**：
认证、限流、路由到后端、有时把多个服务结果**聚合成一个响应**。
本课用内存路由表 + 下游假服务演示 BFF/Gateway 常见模式。

二、写法对比
1. 直观版：网关只做 path 前缀转发。
2. 进阶版：带下游服务注册表 + 聚合接口（如首页拉商品+库存摘要）。

三、真实用途
- YARP、Ocelot、云厂商 APIM、K8s Ingress 都承担类似职责。
资料: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
      https://learn.microsoft.com/en-us/aspnet/core/grpc/
*/

record DownstreamResponse(int Status, string Body);

interface IDownstreamService
{
    string ServiceName { get; }
    DownstreamResponse Call(string relativePath);
}

sealed class CatalogDownstream : IDownstreamService
{
    public string ServiceName => "catalog";

    public DownstreamResponse Call(string relativePath) => relativePath switch
    {
        "/items/1" => new(200, """{"id":1,"name":"键盘"}"""),
        _ => new(404, "not-found")
    };
}

sealed class InventoryDownstream : IDownstreamService
{
    public string ServiceName => "inventory";

    public DownstreamResponse Call(string relativePath) => relativePath switch
    {
        "/stock/1" => new(200, """{"productId":1,"qty":42}"""),
        _ => new(404, "not-found")
    };
}

/** 直观版：仅按前缀 strip 后转发。 */
sealed class NaiveGateway
{
    private readonly Dictionary<string, IDownstreamService> _routes = new(StringComparer.OrdinalIgnoreCase);

    public void Map(string prefix, IDownstreamService downstream) => _routes[prefix] = downstream;

    public DownstreamResponse Forward(string incomingPath)
    {
        foreach (var (prefix, svc) in _routes)
        {
            if (incomingPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var relative = incomingPath[prefix.Length..];
                if (relative.Length == 0) relative = "/";
                return svc.Call(relative);
            }
        }
        return new(404, "gateway:no-route");
    }
}

/** 进阶版：除转发外，提供聚合端点。 */
sealed class BetterGateway
{
    private readonly NaiveGateway _forwarder = new();
    private readonly Dictionary<string, IDownstreamService> _byName = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IDownstreamService svc)
    {
        _byName[svc.ServiceName] = svc;
        _forwarder.Map($"/api/{svc.ServiceName}", svc);
    }

    public DownstreamResponse Handle(string path) =>
        path.Equals("/api/home/1", StringComparison.OrdinalIgnoreCase)
            ? AggregateProductHome(1)
            : _forwarder.Forward(path);

    DownstreamResponse AggregateProductHome(int productId)
    {
        if (!_byName.TryGetValue("catalog", out var catalog) ||
            !_byName.TryGetValue("inventory", out var inventory))
            return new(503, "missing-downstream");

        var item = catalog.Call($"/items/{productId}");
        var stock = inventory.Call($"/stock/{productId}");
        if (item.Status != 200 || stock.Status != 200)
            return new(502, "upstream-error");

        var body = $"{{\"product\":{item.Body},\"stock\":{stock.Body}}}";
        return new(200, body);
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveGateway();
        var catalog = new CatalogDownstream();
        naive.Map("/api/catalog", catalog);

        var fwd = naive.Forward("/api/catalog/items/1");
        if (fwd.Status != 200 || !fwd.Body.Contains("键盘")) throw new Exception("转发 catalog 失败");

        var miss = naive.Forward("/api/unknown/x");
        if (miss.Status != 404) throw new Exception("未知路由应 404");

        var gw = new BetterGateway();
        gw.Register(new CatalogDownstream());
        gw.Register(new InventoryDownstream());

        var home = gw.Handle("/api/home/1");
        if (home.Status != 200 || !home.Body.Contains("product") || !home.Body.Contains("stock"))
            throw new Exception("聚合 /api/home/1 失败");

        var single = gw.Handle("/api/inventory/stock/1");
        if (single.Status != 200 || !single.Body.Contains("42")) throw new Exception("转发 inventory 失败");

        Console.WriteLine("✅ GatewayMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 API 网关心智 ************\n");

        var gw = new BetterGateway();
        gw.Register(new CatalogDownstream());
        gw.Register(new InventoryDownstream());

        var demo = gw.Handle("/api/home/1");
        Console.WriteLine($"聚合响应 {demo.Status}: {demo.Body[..Math.Min(80, demo.Body.Length)]}...");

        RunTests();
    }

    static void Main() => Bootstrap();
}