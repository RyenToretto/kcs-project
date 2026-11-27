using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: 毕业综合——内存 Mini API：Bearer 鉴权 + Todo CRUD + Cache-Aside
[难度]: Hard
[前置]: 06_aspnet_core、08_caching_redis/00_cache、11_testing、12_observability

一、主题说明
把就业主线串起来：网关式路由、简单 Token 鉴权、仓储 CRUD、读路径走缓存旁路。
全程内存、不启动 Kestrel；对照真实 ASP.NET Core + EF + Redis。

二、写法对比
1. 直观版：无鉴权直读写字典。
2. 进阶版：AuthMiddleware + CacheAsideTodoService + 路由表。

三、真实用途
- 作品集 API：再换 SQLite/PostgreSQL 与 JWT 即可上线。
资料: https://learn.microsoft.com/en-us/aspnet/core/security/
      https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
      https://aka.ms/csharp-certification
*/

record TodoItem(int Id, string Title, bool Done);

record HttpRequest(string Method, string Path, string? Body, string? Authorization);

record HttpResponse(int Status, string Body);

interface ITodoRepository
{
    TodoItem? GetById(int id);
    IReadOnlyList<TodoItem> ListAll();
    TodoItem Create(string title);
    bool Update(int id, string title, bool done);
    bool Delete(int id);
}

interface ITokenValidator
{
    bool IsValid(string? bearerHeader, out string subject);
}

interface ITodoCache
{
    bool TryGet(int id, out TodoItem item);
    void Set(TodoItem item);
    void Remove(int id);
}

/** 直观版：裸字典 CRUD。 */
sealed class NaiveTodoStore
{
    private readonly Dictionary<int, TodoItem> _data = new();
    private int _next = 1;

    public TodoItem Add(string title)
    {
        var item = new TodoItem(_next++, title, false);
        _data[item.Id] = item;
        return item;
    }

    public TodoItem? Get(int id) => _data.GetValueOrDefault(id);
}

/** 仓储 */
sealed class InMemoryTodoRepository : ITodoRepository
{
    private readonly Dictionary<int, TodoItem> _store = new();
    private int _nextId = 1;

    public TodoItem Create(string title)
    {
        var t = new TodoItem(_nextId++, title, false);
        _store[t.Id] = t;
        return t;
    }

    public bool Delete(int id) => _store.Remove(id);

    public TodoItem? GetById(int id) => _store.GetValueOrDefault(id);

    public IReadOnlyList<TodoItem> ListAll() => new List<TodoItem>(_store.Values);

    public bool Update(int id, string title, bool done)
    {
        if (!_store.TryGetValue(id, out var cur)) return false;
        _store[id] = cur with { Title = title, Done = done };
        return true;
    }
}

sealed class InMemoryTodoCache : ITodoCache
{
    private readonly Dictionary<int, TodoItem> _cache = new();

    public void Remove(int id) => _cache.Remove(id);

    public void Set(TodoItem item) => _cache[item.Id] = item;

    public bool TryGet(int id, out TodoItem item) => _cache.TryGetValue(id, out item!);
}

sealed class StaticTokenValidator : ITokenValidator
{
    private const string ValidToken = "dev-token-abc";

    public bool IsValid(string? bearerHeader, out string subject)
    {
        subject = "";
        if (string.IsNullOrWhiteSpace(bearerHeader)) return false;
        const string prefix = "Bearer ";
        if (!bearerHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var token = bearerHeader[prefix.Length..].Trim();
        if (token != ValidToken) return false;
        subject = "learner";
        return true;
    }
}

/** Cache-Aside：读先缓存，写穿仓储并失效/更新缓存。 */
sealed class CacheAsideTodoService
{
    private readonly ITodoRepository _repo;
    private readonly ITodoCache _cache;

    public CacheAsideTodoService(ITodoRepository repo, ITodoCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public TodoItem? GetById(int id)
    {
        if (_cache.TryGet(id, out var cached)) return cached;
        var fromDb = _repo.GetById(id);
        if (fromDb is not null) _cache.Set(fromDb);
        return fromDb;
    }

    public TodoItem Create(string title)
    {
        var created = _repo.Create(title);
        _cache.Set(created);
        return created;
    }

    public bool Update(int id, string title, bool done)
    {
        if (!_repo.Update(id, title, done)) return false;
        var fresh = _repo.GetById(id);
        if (fresh is not null) _cache.Set(fresh);
        return true;
    }

    public bool Delete(int id)
    {
        if (!_repo.Delete(id)) return false;
        _cache.Remove(id);
        return true;
    }

    public IReadOnlyList<TodoItem> ListAll() => _repo.ListAll();
}

sealed class MiniApiCapstoneHost
{
    private readonly CacheAsideTodoService _todos;
    private readonly ITokenValidator _auth;

    public MiniApiCapstoneHost(CacheAsideTodoService todos, ITokenValidator auth)
    {
        _todos = todos;
        _auth = auth;
    }

    public HttpResponse Handle(HttpRequest req)
    {
        if (req.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
            return new(200, "ok");

        if (!_auth.IsValid(req.Authorization, out _))
            return new(401, "unauthorized");

        if (req.Method == "GET" && req.Path == "/todos")
        {
            var items = _todos.ListAll();
            var body = string.Join(";", items.Select(t => $"{t.Id}:{t.Title}:{t.Done}"));
            return new(200, body);
        }

        if (req.Method == "POST" && req.Path == "/todos")
        {
            var title = req.Body ?? "";
            var created = _todos.Create(title);
            return new(201, $"{created.Id}|{created.Title}");
        }

        if (req.Method == "GET" && req.Path.StartsWith("/todos/", StringComparison.Ordinal))
        {
            if (!int.TryParse(req.Path["/todos/".Length..], out var id))
                return new(400, "bad-id");
            var item = _todos.GetById(id);
            return item is null ? new(404, "not-found") : new(200, $"{item.Id}|{item.Title}|{item.Done}");
        }

        if (req.Method == "PUT" && req.Path.StartsWith("/todos/", StringComparison.Ordinal))
        {
            if (!int.TryParse(req.Path["/todos/".Length..], out var id))
                return new(400, "bad-id");
            var parts = (req.Body ?? "").Split('|');
            if (parts.Length < 2) return new(400, "body");
            var done = parts.Length > 2 && bool.Parse(parts[2]);
            return _todos.Update(id, parts[0], done) ? new(204, "") : new(404, "not-found");
        }

        if (req.Method == "DELETE" && req.Path.StartsWith("/todos/", StringComparison.Ordinal))
        {
            if (!int.TryParse(req.Path["/todos/".Length..], out var id))
                return new(400, "bad-id");
            return _todos.Delete(id) ? new(204, "") : new(404, "not-found");
        }

        return new(404, "not-found");
    }
}

static class Program
{
    const string AuthHeader = "Bearer dev-token-abc";

    static MiniApiCapstoneHost CreateHost()
    {
        var repo = new InMemoryTodoRepository();
        var cache = new InMemoryTodoCache();
        return new MiniApiCapstoneHost(new CacheAsideTodoService(repo, cache), new StaticTokenValidator());
    }

    static void RunTests()
    {
        var naive = new NaiveTodoStore();
        var t = naive.Add("学 C#");
        if (naive.Get(t.Id)?.Title != "学 C#") throw new Exception("naive CRUD");

        var host = CreateHost();

        var noAuth = host.Handle(new HttpRequest("GET", "/todos", null, null));
        if (noAuth.Status != 401) throw new Exception("无 Token 应 401");

        var health = host.Handle(new HttpRequest("GET", "/health", null, null));
        if (health.Status != 200) throw new Exception("/health 无需鉴权");

        var create = host.Handle(new HttpRequest("POST", "/todos", "写毕业项目", AuthHeader));
        if (create.Status != 201) throw new Exception("POST 创建失败");
        var id = int.Parse(create.Body.Split('|')[0]);

        var get1 = host.Handle(new HttpRequest("GET", $"/todos/{id}", null, AuthHeader));
        if (get1.Status != 200 || !get1.Body.Contains("写毕业项目")) throw new Exception("GET by id");

        var put = host.Handle(new HttpRequest("PUT", $"/todos/{id}", "写毕业项目|x|true", AuthHeader));
        if (put.Status != 204) throw new Exception("PUT 更新");

        var get2 = host.Handle(new HttpRequest("GET", $"/todos/{id}", null, AuthHeader));
        if (!get2.Body.EndsWith("|True", StringComparison.OrdinalIgnoreCase))
            throw new Exception("更新后 Done 应为 true");

        var list = host.Handle(new HttpRequest("GET", "/todos", null, AuthHeader));
        if (!list.Body.Contains("写毕业项目")) throw new Exception("列表应含待办");

        var del = host.Handle(new HttpRequest("DELETE", $"/todos/{id}", null, AuthHeader));
        if (del.Status != 204) throw new Exception("DELETE");

        var gone = host.Handle(new HttpRequest("GET", $"/todos/{id}", null, AuthHeader));
        if (gone.Status != 404) throw new Exception("删除后应 404");

        Console.WriteLine("✅ MiniApiCapstone 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 毕业综合 Mini API ************\n");
        Console.WriteLine("鉴权: Authorization: Bearer dev-token-abc");

        var host = CreateHost();
        host.Handle(new HttpRequest("POST", "/todos", "演示待办", AuthHeader));
        var res = host.Handle(new HttpRequest("GET", "/todos", null, AuthHeader));
        Console.WriteLine($"GET /todos => {res.Status} {res.Body}");

        RunTests();
    }

    static void Main() => Bootstrap();
}