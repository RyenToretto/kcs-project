using System;
using System.Collections.Generic;

/*
[主题]: 消息投递语义——At-most-once / At-least-once / Exactly-once
[难度]: Hard
[前置]: 09_messaging

一、主题说明
网络与重试决定语义。生产多是 At-least-once + 消费者幂等。
陷阱：以为「事务消息」就自动 exactly-once，却未实现业务幂等键。

二、写法对比
1. 直观版：收到即处理，失败吞掉。
2. 进阶版：消费记录 + 幂等表 + 可选手动 Ack 模拟。

三、真实用途
- MassTransit / RabbitMQ / Kafka offset；Outbox 模式与 EF 同事务。
资料: https://github.com/MassTransit/MassTransit
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record OrderPaid(Guid Id, decimal Amount);

enum DeliveryStyle { AtMostOnce, AtLeastOnce }

sealed class NaiveConsumer
{
    public void Handle(OrderPaid msg) => Console.WriteLine($"处理 {msg.Id}"); // 失败即丢
}

sealed class InMemoryBroker
{
    private readonly Queue<OrderPaid> _queue = new();
    public void Publish(OrderPaid msg) => _queue.Enqueue(msg);
    public bool TryDequeue(out OrderPaid msg) => _queue.TryDequeue(out msg!);
}

sealed class IdempotentConsumer
{
    private readonly HashSet<Guid> _processed = new();

    public bool TryHandle(OrderPaid msg, DeliveryStyle style)
    {
        if (_processed.Contains(msg.Id)) return true; // 幂等：重复投递安全

        // 模拟处理
        if (msg.Amount < 0) return style == DeliveryStyle.AtMostOnce;

        _processed.Add(msg.Id);
        return true;
    }
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveConsumer();

        var broker = new InMemoryBroker();
        var id = Guid.NewGuid();
        broker.Publish(new OrderPaid(id, 10m));
        broker.Publish(new OrderPaid(id, 10m)); // 重复投递

        var consumer = new IdempotentConsumer();
        broker.TryDequeue(out var m1);
        if (!consumer.TryHandle(m1!, DeliveryStyle.AtLeastOnce)) throw new Exception("首次应成功");
        broker.TryDequeue(out var m2);
        if (!consumer.TryHandle(m2!, DeliveryStyle.AtLeastOnce)) throw new Exception("重复应幂等略过");

        var bad = new OrderPaid(Guid.NewGuid(), -1m);
        if (consumer.TryHandle(bad, DeliveryStyle.AtLeastOnce))
            throw new Exception("至少一次语义下失败应返回 false 以便重试");

        Console.WriteLine("✅ MqDeliverySemantics 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 MQ Delivery Semantics ************\n");
        Console.WriteLine("⚠ 精确一次需端到端设计（Outbox + 幂等），无银弹。");
        RunTests();
    }

    static void Main() => Bootstrap();
}