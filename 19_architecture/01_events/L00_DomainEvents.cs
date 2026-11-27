using System;
using System.Collections.Generic;

/*
[主题]: 领域事件——聚合内产生、基础设施派发
[难度]: Hard
[前置]: 19_architecture/00_ddd

一、主题说明
事件表示已发生的事；在事务提交后异步通知。
陷阱：在聚合未持久化前就发 MQ 导致消费者读到不存在的数据。

二、写法对比
1. 直观版：改完状态直接调 email 服务。
2. 进阶版：聚合收集 IDomainEvent，UoW 提交后派发。

三、真实用途
- MediatR / 自带 Outbox；与 MassTransit 集成。
资料: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
      https://github.com/MassTransit/MassTransit
*/

interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

record OrderPlaced(Guid OrderId, decimal Total) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

sealed class NaiveOrderService
{
    public void Place() => Console.WriteLine("发邮件"); // 与持久化耦合
}

sealed class OrderAggregate
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> Events => _events;

    public void Place(Guid id, decimal total)
    {
        _events.Add(new OrderPlaced(id, total));
    }

    public void ClearEvents() => _events.Clear();
}

sealed class EventDispatcher
{
    private readonly List<IDomainEvent> _inbox = new();
    public void DispatchPending(IEnumerable<IDomainEvent> events) => _inbox.AddRange(events);
    public int Count => _inbox.Count;
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveOrderService();

        var order = new OrderAggregate();
        var id = Guid.NewGuid();
        order.Place(id, 99m);
        if (order.Events.Count != 1) throw new Exception("应有事件");

        var dispatcher = new EventDispatcher();
        // 模拟：先 Commit 再 Dispatch
        var committed = true;
        if (committed) dispatcher.DispatchPending(order.Events);
        order.ClearEvents();

        if (dispatcher.Count != 1) throw new Exception("应派发一条");
        if (order.Events.Count != 0) throw new Exception("清理后无挂起事件");

        Console.WriteLine("✅ DomainEvents 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Domain Events ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}