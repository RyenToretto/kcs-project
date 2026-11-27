using System;
using System.Collections.Generic;

/*
[主题]: 依赖注入（DI）——从 new 具体类到容器解析
[难度]: Medium
[前置]: 01_oop_csharp/02_interface

一、主题说明
DI 让类通过构造函数声明「我需要什么接口」，由容器在运行时组装。
ASP.NET Core 内置 IServiceCollection；本课用内存迷你容器讲清 Singleton / Scoped / Transient。

二、写法对比
1. 直观版：手写 new，强耦合具体类。
2. 进阶版：注册接口+实现+生命周期，按需 Resolve。

三、真实用途
- DbContext、HttpClient、业务 Service 都经 DI 注入；错误生命周期是线上 Bug 温床。
资料: https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core
      https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
      https://github.com/dotnet/aspnetcore
*/

interface IClock
{
    DateTime UtcNow { get; }
}

/** 直观版：业务里直接 new，难以单测替换。 */
sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

sealed class NaiveGreeting
{
    private readonly SystemClock _clock = new();

    public string Hello(string name) => $"hi {name} at {_clock.UtcNow:O}";
}

enum ServiceLifetime { Singleton, Scoped, Transient }

sealed class ServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public required Type ImplementationType { get; init; }
    public ServiceLifetime Lifetime { get; init; }
}

/** 进阶版：极简 DI 容器（仅供学习，非生产组件）。 */
sealed class MiniServiceContainer
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, object> _scopedCache = new();

    public void AddSingleton<TService, TImpl>() where TImpl : TService =>
        _descriptors.Add(new ServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImpl),
            Lifetime = ServiceLifetime.Singleton
        });

    public void AddScoped<TService, TImpl>() where TImpl : TService =>
        _descriptors.Add(new ServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImpl),
            Lifetime = ServiceLifetime.Scoped
        });

    public void AddTransient<TService, TImpl>() where TImpl : TService =>
        _descriptors.Add(new ServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImpl),
            Lifetime = ServiceLifetime.Transient
        });

    public object Resolve(Type serviceType)
    {
        var desc = _descriptors.Find(d => d.ServiceType == serviceType)
            ?? throw new InvalidOperationException($"未注册: {serviceType.Name}");

        return desc.Lifetime switch
        {
            ServiceLifetime.Singleton => GetOrCreateSingleton(desc),
            ServiceLifetime.Scoped => GetOrCreateScoped(desc),
            ServiceLifetime.Transient => CreateInstance(desc.ImplementationType),
            _ => throw new InvalidOperationException()
        };
    }

    public T Resolve<T>() => (T)Resolve(typeof(T));

    public void BeginScope() => _scopedCache.Clear();

    /** 按构造函数参数递归 Resolve（教学版：取参数最多的那个公开构造）。 */
    object CreateInstance(Type implType)
    {
        var ctor = implType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"无公开构造: {implType.Name}");
        var args = ctor.GetParameters()
            .Select(p => Resolve(p.ParameterType))
            .ToArray();
        return ctor.Invoke(args);
    }

    object GetOrCreateSingleton(ServiceDescriptor desc)
    {
        if (!_singletons.TryGetValue(desc.ServiceType, out var instance))
        {
            instance = CreateInstance(desc.ImplementationType);
            _singletons[desc.ServiceType] = instance;
        }
        return instance;
    }

    object GetOrCreateScoped(ServiceDescriptor desc)
    {
        if (!_scopedCache.TryGetValue(desc.ServiceType, out var instance))
        {
            instance = CreateInstance(desc.ImplementationType);
            _scopedCache[desc.ServiceType] = instance;
        }
        return instance;
    }
}

sealed class FixedClock : IClock
{
    public DateTime UtcNow { get; init; } = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}

interface IGreeter
{
    string Greet(string name);
}

sealed class GreeterService : IGreeter
{
    private readonly IClock _clock;
    public GreeterService(IClock clock) => _clock = clock;
    public string Greet(string name) => $"hello {name} @ {_clock.UtcNow:yyyy-MM-dd}";
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveGreeting().Hello("ada"); // 仅演示可编译运行

        var container = new MiniServiceContainer();
        container.AddSingleton<IClock, FixedClock>();
        container.AddScoped<IGreeter, GreeterService>();

        container.BeginScope();
        var g1 = container.Resolve<IGreeter>();
        var g2 = container.Resolve<IGreeter>();
        if (!ReferenceEquals(g1, g2)) throw new Exception("Scoped 在同一作用域应同实例");

        container.BeginScope();
        var g3 = container.Resolve<IGreeter>();
        if (ReferenceEquals(g1, g3)) throw new Exception("新 Scope 应是新实例");

        if (g1.Greet("bob") != "hello bob @ 2026-01-01") throw new Exception("Greeter 失败");

        Console.WriteLine("✅ DependencyInjection 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Dependency Injection ************\n");
        Console.WriteLine("⚠ MiniServiceContainer 是教学心智模型，生产用 ASP.NET Core 内置 DI。");

        var container = new MiniServiceContainer();
        container.AddSingleton<IClock, FixedClock>();
        container.AddScoped<IGreeter, GreeterService>();
        container.BeginScope();
        Console.WriteLine(container.Resolve<IGreeter>().Greet("dotnet"));
        RunTests();
    }

    static void Main() => Bootstrap();
}