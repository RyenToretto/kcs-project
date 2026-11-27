using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: LINQ 入门 —— Where / Select / OrderBy / 聚合
[难度]: Medium
[前置]: 00_csharp_basics/01_control、01_oop_csharp/00_class

一、主题说明
LINQ（Language Integrated Query）用统一语法在内存集合上做筛选、投影、排序、分组。
写后端时：数据库查询（EF）和内存业务规则都经常见到 LINQ 风格。

二、写法对比
1. 直观版：foreach + if 手工筛选、累加。
2. 进阶版：链式 LINQ，可读、可组合，少写临时列表。

三、真实用途
- 订单列表：筛出「已支付且金额>100」再按时间排序导出。
- 日志分析：对一批事件做 GroupBy 统计。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/linq/
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/
      https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/
*/
static class Program
{
    record Order(int Id, string Status, decimal Amount, DateOnly Day);

    static readonly Order[] SampleOrders =
    {
        new(1, "Paid", 120m, new DateOnly(2026, 3, 1)),
        new(2, "Pending", 80m, new DateOnly(2026, 3, 2)),
        new(3, "Paid", 200m, new DateOnly(2026, 3, 1)),
        new(4, "Cancelled", 50m, new DateOnly(2026, 2, 28)),
    };

    /** 直观版：foreach 筛选已支付且金额>=min。 */
    static List<Order> FilterPaidNaive(IEnumerable<Order> orders, decimal min)
    {
        var result = new List<Order>();
        foreach (var o in orders)
        {
            if (o.Status == "Paid" && o.Amount >= min)
            {
                result.Add(o);
            }
        }
        return result;
    }

    /** 进阶版：LINQ Where。 */
    static IEnumerable<Order> FilterPaidBetter(IEnumerable<Order> orders, decimal min) =>
        orders.Where(o => o.Status == "Paid" && o.Amount >= min);

    /** 直观版：foreach 求已支付总金额。 */
    static decimal SumPaidNaive(IEnumerable<Order> orders)
    {
        decimal sum = 0;
        foreach (var o in orders)
        {
            if (o.Status == "Paid")
            {
                sum += o.Amount;
            }
        }
        return sum;
    }

    /** 进阶版：Where + Sum。 */
    static decimal SumPaidBetter(IEnumerable<Order> orders) =>
        orders.Where(o => o.Status == "Paid").Sum(o => o.Amount);

    /** 投影：只取 Id 与金额，按金额降序。 */
    static List<(int Id, decimal Amount)> TopAmounts(IEnumerable<Order> orders, int take) =>
        orders
            .OrderByDescending(o => o.Amount)
            .Take(take)
            .Select(o => (o.Id, o.Amount))
            .ToList();

    static void RunTests()
    {
        var paidNaive = FilterPaidNaive(SampleOrders, 100m);
        var paidBetter = FilterPaidBetter(SampleOrders, 100m).ToList();
        if (paidNaive.Count != 2 || paidBetter.Count != 2) throw new Exception("筛选数量应为 2");
        if (paidNaive[0].Id != paidBetter[0].Id) throw new Exception("Naive/Better 结果应对齐");

        if (SumPaidNaive(SampleOrders) != 320m) throw new Exception("SumPaidNaive 应为 320");
        if (SumPaidBetter(SampleOrders) != 320m) throw new Exception("SumPaidBetter 应为 320");

        var tops = TopAmounts(SampleOrders, 2);
        if (tops[0].Id != 3 || tops[1].Id != 1) throw new Exception("TopAmounts 排序错误");

        Console.WriteLine("✅ LinqBasics 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 LinqBasics ************\n");
        foreach (var o in FilterPaidBetter(SampleOrders, 100m))
        {
            Console.WriteLine($"已支付大额订单 #{o.Id} 金额={o.Amount}");
        }
        Console.WriteLine($"已支付总额 = {SumPaidBetter(SampleOrders)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}