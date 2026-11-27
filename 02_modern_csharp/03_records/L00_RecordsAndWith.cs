using System;

/*
[主题]: record 与 with 表达式 —— 不可变数据、值相等
[难度]: Medium
[前置]: 01_oop_csharp/00_class

一、主题说明
`record` 适合表示「一整块一起变」的数据（DTO、事件、配置快照）。
自动生成值相等、Deconstruct、复制时可用 `with` 改个别字段。

二、写法对比
1. 直观版：普通 class + 手写 Equals/GetHashCode（易错）。
2. 进阶版：record + `with` 产生新实例，不污染原对象。

三、真实用途
- 领域事件：`OrderPlaced` record 在消息总线传递。
- 应用层：从查询结果 `with { Status = "Shipped" }` 得到新视图模型。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/
      https://learn.microsoft.com/en-us/dotnet/csharp/linq/
*/
static class Program
{
    /** 进阶惯用：record 主构造函数语法。 */
    record ShippingAddress(string City, string Street, string? Zip);

    record OrderLine(int Sku, int Qty, decimal UnitPrice)
    {
        public decimal LineTotal => Qty * UnitPrice;
    }

    record OrderSnapshot(int Id, string Status, ShippingAddress Address, OrderLine[] Lines);

    /** 直观版：新建 class 实例逐个字段复制（易漏字段）。 */
    static OrderSnapshot BumpStatusNaive(OrderSnapshot src, string newStatus)
    {
        return new OrderSnapshot(src.Id, newStatus, src.Address, src.Lines);
    }

    /** 进阶版：with 只改 Status。 */
    static OrderSnapshot BumpStatusBetter(OrderSnapshot src, string newStatus) =>
        src with { Status = newStatus };

    /** with 嵌套：改地址城市。 */
    static OrderSnapshot MoveCity(OrderSnapshot src, string city) =>
        src with { Address = src.Address with { City = city } };

    static void RunTests()
    {
        var addr = new ShippingAddress("上海", "南京路 1 号", "200000");
        var lines = new[] { new OrderLine(1001, 2, 50m) };
        var order = new OrderSnapshot(42, "Paid", addr, lines);

        var naive = BumpStatusNaive(order, "Shipped");
        var better = BumpStatusBetter(order, "Shipped");
        if (naive.Status != "Shipped" || better.Status != "Shipped") throw new Exception("状态应更新");
        if (ReferenceEquals(order, naive)) throw new Exception("应产生新对象");
        if (order.Status != "Paid") throw new Exception("原订单 Status 不应被改");
        if (better.Id != order.Id) throw new Exception("with 应保留 Id");

        var moved = MoveCity(order, "北京");
        if (moved.Address.City != "北京") throw new Exception("城市应更新");
        if (order.Address.City != "上海") throw new Exception("原地址不变");

        var copy = order with { };
        if (!copy.Equals(order)) throw new Exception("record 值相等");
        if (ReferenceEquals(copy, order)) throw new Exception("with 产生新引用");

        if (lines[0].LineTotal != 100m) throw new Exception("LineTotal");

        Console.WriteLine("✅ RecordsAndWith 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 RecordsAndWith ************\n");
        var o = new OrderSnapshot(
            1,
            "Paid",
            new ShippingAddress("杭州", "文三路", null),
            new[] { new OrderLine(9, 3, 19.9m) });
        Console.WriteLine($"原状态={o.Status} 行金额={o.Lines[0].LineTotal}");
        var shipped = BumpStatusBetter(o, "Shipped");
        Console.WriteLine($"新状态={shipped.Status}（原单仍为 {o.Status}）");
        RunTests();
    }

    static void Main() => Bootstrap();
}