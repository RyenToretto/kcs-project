using System;
using System.Collections.Generic;

/*
[主题]: 集成测试心智——内存宿主串联路由 + 仓储 + 多模块协作
[难度]: Medium
[前置]: 06_aspnet_core/00_minimal、11_testing/01_mock

一、主题说明
集成测试验证**多个真实模块拼在一起**是否工作（仍可用内存假 DB，不必每次起 Docker）。
本课用 MiniHost + InMemory 仓储跑通「创建用户 → 查列表」完整链路，对照 WebApplicationFactory。

二、写法对比
1. 直观版：直接 new 服务类链式调用，不经 HTTP。
2. 进阶版：经迷你 HTTP 管线发请求，更接近 ASP.NET 集成测。

三、真实用途
- dotnet test + WebApplicationFactory、Testcontainers 测真实依赖。
资料: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test
      https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
*/

record UserDto(int Id, string Name);

record HttpRequestContext(string Method, string Path, string? Body);

record HttpResponseContext(int Status, string Body);

sealed class InMemoryUserRepo
{
    private readonly List<UserDto> _users = [];
    private int _nextId = 1;

    public UserDto Add(string name)
    {
        var u = new UserDto(_nextId++, name);
        _users.Add(u);
        return u;
    }

    public IReadOnlyList<UserDto> List() => _users;
}

sealed class UserAppService
{
    private readonly InMemoryUserRepo _repo;

    public UserAppService(InMemoryUserRepo repo) => _repo = repo;

    public UserDto Register(string name) => _repo.Add(name);
    public IReadOnlyList<UserDto> GetAll() => _repo.List();
}

/** 直观版：不经过 HTTP，直接测服务 + 仓储。 */
static class NaiveIntegration
{
    public static void RunScenario()
    {
        var repo = new InMemoryUserRepo();
        var svc = new UserAppService(repo);
        var u = svc.Register("Alice");
        if (u.Name != "Alice") throw new Exception("注册名错误");
        if (svc.GetAll().Count != 1) throw new Exception("列表数量应为 1");
    }
}

/** 进阶版：内存 HTTP 宿主串联 POST/GET。 */
sealed class IntegrationTestHost
{
    private readonly UserAppService _svc;
    private readonly Dictionary<string, Func<HttpRequestContext, HttpResponseContext>> _routes = new();

    public IntegrationTestHost(UserAppService svc)
    {
        _svc = svc;
        _routes["POST /users"] = req =>
        {
            var name = req.Body ?? "";
            var user = _svc.Register(name);
            return new HttpResponseContext(201, $"{user.Id}:{user.Name}");
        };
        _routes["GET /users"] = _ =>
        {
            var all = _svc.GetAll();
            var body = string.Join(",", all.Select(u => u.Name));
            return new HttpResponseContext(200, body);
        };
    }

    public HttpResponseContext Send(HttpRequestContext req)
    {
        var key = $"{req.Method.ToUpperInvariant()} {req.Path}";
        if (_routes.TryGetValue(key, out var handler))
            return handler(req);
        return new HttpResponseContext(404, "not-found");
    }
}

static class Program
{
    static void RunTests()
    {
        NaiveIntegration.RunScenario();

        var host = new IntegrationTestHost(new UserAppService(new InMemoryUserRepo()));

        var create = host.Send(new HttpRequestContext("POST", "/users", "Bob"));
        if (create.Status != 201 || !create.Body.Contains("Bob")) throw new Exception("POST /users 失败");

        host.Send(new HttpRequestContext("POST", "/users", "Carol"));
        var list = host.Send(new HttpRequestContext("GET", "/users", null));
        if (list.Status != 200 || list.Body != "Bob,Carol") throw new Exception($"GET /users 期望 Bob,Carol 得 {list.Body}");

        var missing = host.Send(new HttpRequestContext("DELETE", "/users", null));
        if (missing.Status != 404) throw new Exception("未支持方法应 404");

        Console.WriteLine("✅ IntegrationTestMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 集成测试心智 ************\n");
        Console.WriteLine("场景: POST 两名用户 → GET 列表断言顺序。");

        var host = new IntegrationTestHost(new UserAppService(new InMemoryUserRepo()));
        host.Send(new HttpRequestContext("POST", "/users", "演示用户"));
        var res = host.Send(new HttpRequestContext("GET", "/users", null));
        Console.WriteLine($"GET /users => {res.Status} [{res.Body}]");

        RunTests();
    }

    static void Main() => Bootstrap();
}