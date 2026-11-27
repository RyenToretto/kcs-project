using System;
using System.Collections.Generic;

/*
[主题]: 微服务拆分——单体与按业务边界划分的内存服务
[难度]: Medium
[前置]: 06_aspnet_core/03_layers、09_messaging

一、主题说明
「微服务」不是把代码拆成很多 Git 仓库那么简单，而是按**业务边界**（限界上下文）
划分独立部署单元。本课用内存里的 Order / Catalog 两个假服务 + 一个单体版对照，
体会「改一个模块不用全站发布」与「跨服务调用复杂度」的权衡。

二、写法对比
1. 直观版：单体类里所有逻辑堆一起。
2. 进阶版：接口隔离 + 每个服务独立存储，通过契约调用。

三、真实用途
- 团队并行、独立扩容、技术栈多样；代价是分布式事务、观测、网关。
资料: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
      https://learn.microsoft.com/en-us/aspnet/core/grpc/
*/

record Product(int Id, string Name, decimal Price);

record OrderLine(int ProductId, int Qty);

record Order(int Id, IReadOnlyList<OrderLine> Lines, decimal Total);

/** 直观版：大单体，订单和商品逻辑混在一起。 */
sealed class MonolithNaive
{
    private readonly List<Product> _products = [];
    private readonly List<Order> _orders = [];
    private int _nextOrderId = 1;

    public void SeedProduct(Product p) => _products.Add(p);

    public Order PlaceOrder(IReadOnlyList<OrderLine> lines)
    {
        decimal total = 0;
        foreach (var line in lines)
        {
            var p = _products.Find(x => x.Id == line.ProductId)
                ?? throw new InvalidOperationException($"商品 {line.ProductId} 不存在");
            total += p.Price * line.Qty;
        }
        var order = new Order(_nextOrderId++, lines, total);
        _orders.Add(order);
        return order;
    }

    public int OrderCount => _orders.Count;
}

/** 进阶版：Catalog 与 Order 服务分离，仅通过公开契约协作。 */
interface ICatalogService
{
    Product? GetProduct(int id);
}

interface IOrderService
{
    Order PlaceOrder(IReadOnlyList<OrderLine> lines);
}

sealed class InMemoryCatalogService : ICatalogService
{
    private readonly Dictionary<int, Product> _store = new();

    public void Seed(Product p) => _store[p.Id] = p;

    public Product? GetProduct(int id) =>
        _store.TryGetValue(id, out var p) ? p : null;
}

sealed class InMemoryOrderService : IOrderService
{
    private readonly ICatalogService _catalog;
    private readonly List<Order> _orders = [];
    private int _nextId = 1;

    public InMemoryOrderService(ICatalogService catalog) => _catalog = catalog;

    public Order PlaceOrder(IReadOnlyList<OrderLine> lines)
    {
        decimal total = 0;
        foreach (var line in lines)
        {
            var p = _catalog.GetProduct(line.ProductId)
                ?? throw new InvalidOperationException($"商品 {line.ProductId} 不存在");
            total += p.Price * line.Qty;
        }
        var order = new Order(_nextId++, lines, total);
        _orders.Add(order);
        return order;
    }

    public int OrderCount => _orders.Count;
}

static class Program
{
    static void RunTests()
    {
        var mono = new MonolithNaive();
        mono.SeedProduct(new Product(1, "键盘", 299m));
        var o1 = mono.PlaceOrder([new OrderLine(1, 2)]);
        if (o1.Total != 598m) throw new Exception("单体下单总价错误");
        if (mono.OrderCount != 1) throw new Exception("单体订单数错误");

        var catalog = new InMemoryCatalogService();
        catalog.Seed(new Product(1, "键盘", 299m));
        var orders = new InMemoryOrderService(catalog);
        var o2 = orders.PlaceOrder([new OrderLine(1, 1)]);
        if (o2.Total != 299m) throw new Exception("微服务式下单总价错误");
        if (orders.OrderCount != 1) throw new Exception("订单服务计数错误");

        try
        {
            orders.PlaceOrder([new OrderLine(99, 1)]);
            throw new Exception("应抛出商品不存在");
        }
        catch (InvalidOperationException) { /* 预期 */ }

        Console.WriteLine("✅ ServiceSplit 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 微服务拆分心智 ************\n");
        Console.WriteLine("对比：单体一次 PlaceOrder vs Catalog+Order 两服务协作。");

        var catalog = new InMemoryCatalogService();
        catalog.Seed(new Product(2, "鼠标", 99m));
        var orders = new InMemoryOrderService(catalog);
        var demo = orders.PlaceOrder([new OrderLine(2, 3)]);
        Console.WriteLine($"演示订单 #{demo.Id}，合计 {demo.Total} 元");

        RunTests();
    }

    static void Main() => Bootstrap();
}