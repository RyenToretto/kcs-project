using System;
using System.Collections.Generic;

/*
[主题]: DDD 战术——实体、值对象、聚合边界
[难度]: Hard
[前置]: 01_oop_csharp、07_ef_core

一、主题说明
聚合根是唯一修改入口；值对象不可变、按值相等。
陷阱：巨型 Order 聚合包含所有 OrderLine 外实体导致事务过大。

二、写法对比
1. 直观版：贫血模型 set 到处可调。
2. 进阶版：Money 值对象 + Order 聚合根封装状态变更。

三、真实用途
- 限界上下文内建模；与 EF 配置 Owned Type。
资料: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
      https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
*/

readonly record struct Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException("币种不同");
        return new Money(Amount + other.Amount, Currency);
    }
}

sealed class NaiveOrder
{
    public decimal Total { get; set; } // 任意层可改
}

sealed class OrderLine
{
    public Money UnitPrice { get; }
    public int Qty { get; }
    public OrderLine(Money unitPrice, int qty) { UnitPrice = unitPrice; Qty = qty; }
    public Money LineTotal() => new Money(UnitPrice.Amount * Qty, UnitPrice.Currency);
}

/** 聚合根 */
sealed class Order
{
    private readonly List<OrderLine> _lines = new();
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyList<OrderLine> Lines => _lines;

    public void AddLine(Money unitPrice, int qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        _lines.Add(new OrderLine(unitPrice, qty));
    }

    public Money Total()
    {
        Money sum = new(0, "CNY");
        foreach (var line in _lines) sum = sum.Add(line.LineTotal());
        return sum;
    }
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveOrder { Total = 1m };

        var order = new Order();
        order.AddLine(new Money(10, "CNY"), 2);
        order.AddLine(new Money(5, "CNY"), 1);
        if (order.Total().Amount != 25) throw new Exception("聚合总价");

        var m1 = new Money(1, "CNY");
        var m2 = new Money(1, "CNY");
        if (!m1.Equals(m2)) throw new Exception("值对象相等");

        try { order.AddLine(new Money(1, "CNY"), 0); throw new Exception("应拒绝数量"); }
        catch (ArgumentOutOfRangeException) { }

        Console.WriteLine("✅ DddTactical 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 DDD Tactical ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}