using System.Collections.Concurrent;

/*
[主题]: DI 生命周期陷阱——Singleton 持 Scoped、Captive Dependency
[难度]: Hard
[前置]: 06_aspnet_core/01_di

一、主题说明
Singleton 跨请求共享；Scoped 每请求/作用域一份；Transient 每次 Resolve 新建。
陷阱：Singleton 注入 Scoped DbContext → 连接泄漏与线程安全问题。

二、写法对比
1. 直观版：全部 Singleton「省事」。
2. 进阶版：按线程安全与状态边界选生命周期，并检测 Captive Dependency。

三、真实用途
- ASP.NET Core 每请求 Scope；BackgroundService 需自己 CreateScope。
资料: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
*/

interface IUnitOfWork { int Id { get; } }

sealed class NaiveRegistry
{
    // 直观版：所有服务注册为单例（DbContext 类服务在生产会炸）
    private readonly object _db = new object();
    public object Db => _db;
}

enum LifetimeKind { Singleton, Scoped, Transient }

sealed class DiRegistry
{
    private readonly LifetimeKind _uowLifetime;
    private object? _singletonUow;
    private readonly ConcurrentDictionary<int, object> _scopedCache = new();
    private int _scopeId;

    public DiRegistry(LifetimeKind uowLifetime) => _uowLifetime = uowLifetime;

    public void BeginScope() { _scopeId++; _scopedCache.Clear(); }

    public IUnitOfWork ResolveUow()
    {
        if (_uowLifetime == LifetimeKind.Singleton)
        {
            return (IUnitOfWork)(_singletonUow ??= new FakeUnitOfWork());
        }

        if (_uowLifetime == LifetimeKind.Scoped)
        {
            return (IUnitOfWork)_scopedCache.GetOrAdd(_scopeId, _ => new FakeUnitOfWork());
        }

        return new FakeUnitOfWork();
    }
}

sealed class FakeUnitOfWork : IUnitOfWork
{
  private static int _counter;
  public int Id { get; } = Interlocked.Increment(ref _counter);
}

/** 模拟 Singleton 服务捕获 Scoped 依赖（反模式）。 */
sealed class CaptiveHost
{
    public IUnitOfWork Captured { get; }
    public CaptiveHost(IUnitOfWork uow) => Captured = uow;
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveRegistry();

        var scopedReg = new DiRegistry(LifetimeKind.Scoped);
        scopedReg.BeginScope();
        var a = scopedReg.ResolveUow();
        var b = scopedReg.ResolveUow();
        if (a.Id != b.Id) throw new Exception("Scoped 同作用域应同 Id");

        scopedReg.BeginScope();
        var c = scopedReg.ResolveUow();
        if (a.Id == c.Id) throw new Exception("新 Scope 应有新 UoW");

        var transientReg = new DiRegistry(LifetimeKind.Transient);
        if (transientReg.ResolveUow().Id == transientReg.ResolveUow().Id)
            throw new Exception("Transient 应每次新建");

        // Captive：Singleton 构造时拿到第一个 Scoped 实例，之后所有请求复用——错误
        var wrong = new DiRegistry(LifetimeKind.Scoped);
        wrong.BeginScope();
        var captive = new CaptiveHost(wrong.ResolveUow());
        wrong.BeginScope();
        var fresh = wrong.ResolveUow();
        if (captive.Captured.Id == fresh.Id)
            throw new Exception("本测试期望捕获旧实例；若相等说明未模拟 captivity");

        Console.WriteLine("✅ DiLifetimes 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 DI Lifetimes ************\n");
        Console.WriteLine("⚠ 生产禁止 Singleton 直接注入 DbContext/Scoped 服务。");
        RunTests();
    }

    static void Main() => Bootstrap();
}