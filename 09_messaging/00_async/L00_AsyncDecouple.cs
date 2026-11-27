using System;
using System.Collections.Generic;

/*
[主题]: 异步解耦——用消息队列削峰、延迟副作用
[难度]: Medium
[前置]: 04_async_await/01_async

一、主题说明
下单后不必同步发邮件/积分；把事件扔进队列，消费者慢慢处理。本课用内存 Channel 列表模拟，
不连接 RabbitMQ / Kafka。

二、写法对比
1. 直观版：下单方法里直接调用 Notify + Points，全同步。
2. 进阶版：只发布 OrderCreated 消息，订阅者各自处理。

三、真实用途
- 提升接口响应、隔离故障域；与 Outbox 模式配合保证可靠投递。
资料: https://github.com/MassTransit/MassTransit
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record OrderCreated(string OrderId, string UserId, decimal Amount);

interface IOrderRepository
{
    void Save(string orderId);
}

/** 直观版：上帝方法串联所有副作用。 */
sealed class NaiveCheckout
{
    public List<string> Log { get; } = new();

    public void Checkout(string orderId, string userId, decimal amount)
    {
        Log.Add($"save:{orderId}");
        Log.Add($"email:{userId}");
        Log.Add($"points:{userId}:{amount}");
    }
}

/** 进阶版：内存 Broker + 多处理器。 */
sealed class InMemoryMessageBus
{
    private readonly Queue<object> _queue = new();
    private readonly List<Action<object>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) =>
        _handlers.Add(msg => { if (msg is T typed) handler(typed); });

    public void Publish(object message) => _queue.Enqueue(message);

    public int DrainAll()
    {
        var count = 0;
        while (_queue.Count > 0)
        {
            var msg = _queue.Dequeue();
            foreach (var h in _handlers) h(msg);
            count++;
        }
        return count;
    }
}

sealed class DecoupledCheckoutService
{
    private readonly IOrderRepository _orders;
    private readonly InMemoryMessageBus _bus;

    public DecoupledCheckoutService(IOrderRepository orders, InMemoryMessageBus bus)
    {
        _orders = orders;
        _bus = bus;
    }

    public void Checkout(string orderId, string userId, decimal amount)
    {
        _orders.Save(orderId);
        _bus.Publish(new OrderCreated(orderId, userId, amount));
    }
}

sealed class InMemoryOrderRepo : IOrderRepository
{
    public List<string> Saved { get; } = new();
    public void Save(string orderId) => Saved.Add(orderId);
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveCheckout();
        naive.Checkout("o1", "u1", 10m);
        if (naive.Log.Count != 3) throw new Exception("同步应做 3 步");

        var bus = new InMemoryMessageBus();
        var emails = new List<string>();
        var points = new List<string>();
        bus.Subscribe<OrderCreated>(e => emails.Add(e.UserId));
        bus.Subscribe<OrderCreated>(e => points.Add($"{e.UserId}:{e.Amount}"));

        var repo = new InMemoryOrderRepo();
        var svc = new DecoupledCheckoutService(repo, bus);
        svc.Checkout("o2", "u2", 20m);
        if (repo.Saved.Count != 1) throw new Exception("应先落单");
        if (emails.Count != 0) throw new Exception("未 Drain 前副作用不应执行");

        if (bus.DrainAll() != 1) throw new Exception("应处理 1 条消息");
        if (emails.Count != 1 || points.Count != 1) throw new Exception("两个订阅者应各执行一次");

        Console.WriteLine("✅ AsyncDecouple 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Async Decouple ************\n");
        Console.WriteLine("⚠ InMemoryMessageBus 替代 MassTransit/RabbitMQ。");

        var bus = new InMemoryMessageBus();
        bus.Subscribe<OrderCreated>(e => Console.WriteLine($"handle {e.OrderId}"));
        new DecoupledCheckoutService(new InMemoryOrderRepo(), bus).Checkout("demo", "u", 1m);
        bus.DrainAll();
        RunTests();
    }

    static void Main() => Bootstrap();
}