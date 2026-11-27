using System;
using System.Collections.Generic;

/*
[主题]: 消息 Broker 心智——Exchange 路由 + Queue 存储 + Consumer 拉取
[难度]: Medium
[前置]: 09_messaging/00_async

一、主题说明
RabbitMQ 等有 Exchange（路由）与 Queue（缓冲）；本课用 MiniBroker 模拟 publish / bind / consume，
离线理解 AMQP 核心概念。MassTransit 在其上提供更高级的 .NET API。

二、写法对比
1. 直观版：单队列 FIFO。
2. 进阶版：按 routingKey 绑定多个队列（topic 简化版）。

三、真实用途
- 微服务事件总线、集成第三方系统、削峰填谷。
资料: https://github.com/MassTransit/MassTransit
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record BrokerMessage(string RoutingKey, string Body);

/** 直观版：单队列。 */
sealed class NaiveFifoQueue
{
    private readonly Queue<string> _q = new();
    public void Enqueue(string body) => _q.Enqueue(body);
    public string? Dequeue() => _q.Count > 0 ? _q.Dequeue() : null;
}

/** 进阶版：简化 Topic Exchange。 */
sealed class MiniBroker
{
    private readonly Dictionary<string, List<string>> _bindings = new(); // routingKey -> queue names
    private readonly Dictionary<string, Queue<BrokerMessage>> _queues = new();

    public void DeclareQueue(string queueName)
    {
        if (!_queues.ContainsKey(queueName))
            _queues[queueName] = new Queue<BrokerMessage>();
    }

    public void Bind(string routingKey, string queueName)
    {
        if (!_bindings.TryGetValue(routingKey, out var list))
        {
            list = new List<string>();
            _bindings[routingKey] = list;
        }
        if (!list.Contains(queueName)) list.Add(queueName);
    }

    public void Publish(BrokerMessage message)
    {
        if (!_bindings.TryGetValue(message.RoutingKey, out var targets)) return;
        foreach (var q in targets)
        {
            if (_queues.TryGetValue(q, out var queue))
                queue.Enqueue(message);
        }
    }

    public BrokerMessage? Consume(string queueName) =>
        _queues.TryGetValue(queueName, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveFifoQueue();
        naive.Enqueue("a");
        if (naive.Dequeue() != "a") throw new Exception("FIFO 失败");

        var broker = new MiniBroker();
        broker.DeclareQueue("email");
        broker.DeclareQueue("billing");
        broker.Bind("order.created", "email");
        broker.Bind("order.created", "billing");
        broker.Bind("order.paid", "billing");

        broker.Publish(new BrokerMessage("order.created", "o-100"));
        if (broker.Consume("email")?.Body != "o-100") throw new Exception("email 应收到");
        if (broker.Consume("billing")?.Body != "o-100") throw new Exception("billing 应收到");

        broker.Publish(new BrokerMessage("order.paid", "pay-1"));
        if (broker.Consume("email") is not null) throw new Exception("email 未绑定 paid");
        if (broker.Consume("billing")?.Body != "pay-1") throw new Exception("billing 应收 paid");

        Console.WriteLine("✅ BrokerMentalModel 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Broker Mental Model ************\n");
        Console.WriteLine("⚠ MiniBroker 非 RabbitMQ；对照 MassTransit/RabbitMQ 拓扑。");

        var broker = new MiniBroker();
        broker.DeclareQueue("notify");
        broker.Bind("demo.event", "notify");
        broker.Publish(new BrokerMessage("demo.event", "hello-broker"));
        Console.WriteLine(broker.Consume("notify")?.Body);
        RunTests();
    }

    static void Main() => Bootstrap();
}