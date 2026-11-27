using System;
using System.Collections.Generic;

/*
[主题]: 至少一次投递 + 消费者幂等
[难度]: Medium
[前置]: 09_messaging/00_async

一、主题说明
MQ 常保证「至少一次」：同一条消息可能重复消费。业务要用幂等键/去重表保证多次执行结果相同。
本课用内存 ProcessedIds 模拟，无需真实 Broker。

二、写法对比
1. 直观版：每消息直接累加库存/余额，重复消费就错。
2. 进阶版：先检查 messageId 是否处理过，再执行业务。

三、真实用途
- 支付回调、物流状态同步、积分发放；与分布式锁/唯一索引配合。
资料: https://github.com/MassTransit/MassTransit
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record PaymentSettled(string MessageId, string OrderId, decimal Amount);

/** 直观版：无去重。 */
sealed class NaiveWallet
{
    public decimal Balance { get; private set; }

    public void OnPayment(PaymentSettled msg) => Balance += msg.Amount;
}

/** 进阶版：幂等消费者。 */
sealed class IdempotentPaymentHandler
{
    private readonly HashSet<string> _processed = new(StringComparer.Ordinal);
    private readonly Dictionary<string, decimal> _orderPaid = new();

    public bool Handle(PaymentSettled msg)
    {
        if (!_processed.Add(msg.MessageId))
            return false; // 已处理，幂等跳过

        _orderPaid[msg.OrderId] = msg.Amount;
        return true;
    }

    public decimal GetPaidAmount(string orderId) => _orderPaid.GetValueOrDefault(orderId);
    public int ProcessedMessageCount => _processed.Count;
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveWallet();
        var dup = new PaymentSettled("m1", "o1", 50m);
        naive.OnPayment(dup);
        naive.OnPayment(dup);
        if (naive.Balance != 100m) throw new Exception("重复消费导致重复加钱");

        var handler = new IdempotentPaymentHandler();
        var msg = new PaymentSettled("m2", "o2", 80m);
        if (!handler.Handle(msg)) throw new Exception("首次应成功");
        if (handler.Handle(msg)) throw new Exception("重复 messageId 应返回 false");
        if (handler.GetPaidAmount("o2") != 80m) throw new Exception("金额应只记一次");
        if (handler.ProcessedMessageCount != 1) throw new Exception("去重表应 1 条");

        Console.WriteLine("✅ AtLeastOnceIdempotent 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 At-Least-Once + Idempotent ************\n");
        Console.WriteLine("⚠ 内存去重；生产用唯一索引 / Redis SET / 业务状态机。");

        var h = new IdempotentPaymentHandler();
        var evt = new PaymentSettled("demo-msg", "ord-1", 9.9m);
        Console.WriteLine($"first={h.Handle(evt)} second={h.Handle(evt)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}