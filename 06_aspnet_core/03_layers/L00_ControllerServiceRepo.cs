using System;
using System.Collections.Generic;

/*
[主题]: 分层架构——Controller → Service → Repository
[难度]: Medium
[前置]: 06_aspnet_core/01_di、01_oop_csharp/02_interface

一、主题说明
Web 层只处理 HTTP 语义；Service 放业务规则；Repository 管数据访问。
本课用迷你 Controller 模拟 ASP.NET Core，数据层用内存字典代替 EF。

二、写法对比
1. 直观版：一个类既处理 HTTP 又访问字典。
2. 进阶版：三层各司其职，Controller 只调 Service。

三、真实用途
- 团队分工、单元测试（Mock Repository）、替换存储都不乱。
资料: https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
      https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core
      https://github.com/dotnet/aspnetcore
*/

record ProductDto(int Id, string Name, decimal Price);

interface IProductRepository
{
    ProductDto? GetById(int id);
    void Add(ProductDto product);
}

interface IProductService
{
    string? GetName(int id);
    string Create(string name, decimal price);
}

/** 直观版：肥 Controller，难以单独测业务。 */
sealed class NaiveProductEndpoint
{
    private readonly Dictionary<int, ProductDto> _db = new() { [1] = new(1, "Keyboard", 99m) };

    public string HandleGet(int id) =>
        _db.TryGetValue(id, out var p) ? p.Name : "not-found";
}

/** Repository：只关心存取。 */
sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<int, ProductDto> _store = new();
    private int _nextId = 1;

    public ProductDto? GetById(int id) => _store.TryGetValue(id, out var p) ? p : null;

    public void Add(ProductDto product)
    {
        var id = product.Id == 0 ? _nextId++ : product.Id;
        _store[id] = product with { Id = id };
    }
}

/** Service：业务规则（校验、编排）。 */
sealed class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public string? GetName(int id) => _repo.GetById(id)?.Name;

    public string Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name 不能为空");
        if (price <= 0) throw new ArgumentException("price 必须 > 0");
        _repo.Add(new ProductDto(0, name.Trim(), price));
        return "created";
    }
}

/** Controller：翻译 HTTP 概念为 Service 调用。 */
sealed class ProductController
{
    private readonly IProductService _service;
    public ProductController(IProductService service) => _service = service;

    public (int Status, string Body) Get(int id)
    {
        var name = _service.GetName(id);
        return name is null ? (404, "not-found") : (200, name);
    }

    public (int Status, string Body) Post(string name, decimal price)
    {
        try
        {
            return (201, _service.Create(name, price));
        }
        catch (ArgumentException ex)
        {
            return (400, ex.Message);
        }
    }
}

static class Program
{
    static void RunTests()
    {
        if (new NaiveProductEndpoint().HandleGet(1) != "Keyboard") throw new Exception("Naive 失败");

        var repo = new InMemoryProductRepository();
        repo.Add(new ProductDto(10, "Mouse", 49m));
        var svc = new ProductService(repo);
        var ctrl = new ProductController(svc);

        var get = ctrl.Get(10);
        if (get.Status != 200 || get.Body != "Mouse") throw new Exception("GET 失败");

        var post = ctrl.Post("Monitor", 299m);
        if (post.Status != 201) throw new Exception("POST 应 201");

        var bad = ctrl.Post(" ", 1m);
        if (bad.Status != 400) throw new Exception("校验应 400");

        Console.WriteLine("✅ ControllerServiceRepo 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Controller → Service → Repo ************\n");
        Console.WriteLine("⚠ Controller 为教学类，非 [ApiController]；存储为内存假实现。");

        var ctrl = new ProductController(new ProductService(new InMemoryProductRepository()));
        Console.WriteLine(ctrl.Post("Desk", 199m));
        RunTests();
    }

    static void Main() => Bootstrap();
}