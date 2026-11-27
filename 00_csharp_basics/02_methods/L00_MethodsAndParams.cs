using System;

/*
[主题]: 方法、参数、返回值与可选参数
[难度]: Easy
[前置]: 00_csharp_basics/00_hello、01_control

一、主题说明
方法（method）把一段可复用逻辑起名。C# 里几乎所有业务都拆成方法：校验、计算、写库、调 API。
本课掌握：返回值、参数、可选参数、命名参数、params、简单重载。

二、写法对比
1. 直观版：显式参数列表 + return。
2. 进阶版：表达式体方法、可选参数、params 打包可变个数参数。

三、真实用途
- Web API Controller / Minimal API 的 handler 本质是方法。
- 领域服务：CreateOrder(userId, items) 这类签名每天都写。
- 工具方法用 params 接收任意个参数（日志、拼接）。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/params
*/
static class Program
{
    /** 直观版：两个整数相加。 */
    static int AddNaive(int a, int b)
    {
        return a + b;
    }

    /** 进阶版：表达式体方法，语义相同、更短。 */
    static int AddBetter(int a, int b) => a + b;

    /** 可选参数：折扣默认 0；命名参数可跳过位置。 */
    static decimal PriceWithDiscount(decimal price, decimal discount = 0m)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (discount < 0 || discount > 1) throw new ArgumentOutOfRangeException(nameof(discount));
        return price * (1 - discount);
    }

    /** params：接收任意个整数求和。 */
    static int SumAll(params int[] values)
    {
        int sum = 0;
        foreach (int v in values)
        {
            sum += v;
        }
        return sum;
    }

    /** 重载：同名不同参数列表。 */
    static string FormatId(int id) => $"ID-{id:D4}";
    static string FormatId(int id, string prefix) => $"{prefix}-{id:D4}";

    /** out 参数：尝试解析，失败不抛异常（教学用；生产更常用 TryParse 模式）。 */
    static bool TryParsePositive(string text, out int value)
    {
        if (int.TryParse(text, out value) && value > 0)
        {
            return true;
        }
        value = 0;
        return false;
    }

    static void RunTests()
    {
        if (AddNaive(2, 3) != 5) throw new Exception("AddNaive 失败");
        if (AddBetter(2, 3) != 5) throw new Exception("AddBetter 失败");
        if (PriceWithDiscount(100m) != 100m) throw new Exception("无折扣应为原价");
        if (PriceWithDiscount(100m, 0.2m) != 80m) throw new Exception("2 折应为 80");
        if (PriceWithDiscount(discount: 0.1m, price: 200m) != 180m) throw new Exception("命名参数失败");
        if (SumAll(1, 2, 3, 4) != 10) throw new Exception("SumAll 失败");
        if (FormatId(7) != "ID-0007") throw new Exception("FormatId 失败");
        if (FormatId(7, "ORD") != "ORD-0007") throw new Exception("FormatId 重载失败");
        if (!TryParsePositive("42", out int n) || n != 42) throw new Exception("TryParsePositive 成功路径失败");
        if (TryParsePositive("-1", out _)) throw new Exception("负数不应解析成功");
        Console.WriteLine("✅ MethodsAndParams 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 MethodsAndParams ************\n");
        Console.WriteLine($"AddBetter(3,4) = {AddBetter(3, 4)}");
        Console.WriteLine($"折后价 = {PriceWithDiscount(199m, 0.15m)}");
        Console.WriteLine($"SumAll = {SumAll(10, 20, 30)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}
