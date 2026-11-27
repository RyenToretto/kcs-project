using System;
using System.Collections.Generic;

/*
[主题]: 表驱动单元测试心智（自研 Assert，不依赖 xUnit 包）
[难度]: Easy
[前置]: 00_csharp_basics、05_dotnet_tooling

一、主题说明
表驱动测试：用「数据表」列出多组 (输入, 期望) 喂给同一个被测函数，避免复制粘贴断言。
本课在单文件里实现迷你 Assert + Theory 风格循环；正式项目请用 xUnit 的 [Theory]/[InlineData]。

二、写法对比
1. 直观版：每个用例手写 if + throw。
2. 进阶版：TestCase 记录 + RunTable 统一执行并带用例名。

三、真实用途
- 边界值、格式化、校验规则等「多输入同逻辑」场景的首选用法。
- 生产环境: xUnit + dotnet test — https://xunit.net/
资料: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test
      https://xunit.net/
*/

/** 迷你断言（名称贴近 xUnit，方便对照文档）。 */
static class MiniAssert
{
    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception(message ?? $"期望 {expected}，实际 {actual}");
    }

    public static void True(bool condition, string? message = null)
    {
        if (!condition) throw new Exception(message ?? "条件应为 true");
    }

    public static void Throws<TException>(Action action, string? message = null) where TException : Exception
    {
        try
        {
            action();
            throw new Exception(message ?? $"应抛出 {typeof(TException).Name}");
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new Exception(message ?? $"抛出了 {ex.GetType().Name} 而非 {typeof(TException).Name}");
        }
    }
}

record TestCase<TIn, TOut>(string Name, TIn Input, TOut Expected);

static class TableRunner
{
    public static void Run<TIn, TOut>(IEnumerable<TestCase<TIn, TOut>> cases, Func<TIn, TOut> sut)
    {
        foreach (var c in cases)
        {
            var actual = sut(c.Input);
            MiniAssert.Equal(c.Expected, actual, $"用例 [{c.Name}] 失败");
        }
    }
}

static class DiscountCalculator
{
    /** 订单金额满减：≥100 打九折，否则原价（四舍五入到分）。 */
    public static decimal Apply(decimal amount) =>
        amount >= 100m ? Math.Round(amount * 0.9m, 2) : amount;
}

static class Program
{
    static void RunTestsNaive()
    {
        if (DiscountCalculator.Apply(50m) != 50m) throw new Exception("naive 50");
        if (DiscountCalculator.Apply(100m) != 90m) throw new Exception("naive 100");
        if (DiscountCalculator.Apply(200m) != 180m) throw new Exception("naive 200");
    }

    static void RunTestsTableDriven()
    {
        var cases = new List<TestCase<decimal, decimal>>
        {
            new("未达门槛", 50m, 50m),
            new("刚好一百", 100m, 90m),
            new("大额订单", 200m, 180m),
            new("小数", 99.99m, 99.99m),
        };
        TableRunner.Run(cases, DiscountCalculator.Apply);
    }

    static void RunTests()
    {
        RunTestsNaive();
        RunTestsTableDriven();

        MiniAssert.Throws<FormatException>(
            () => _ = int.Parse("not-a-number"),
            "Parse 无效数字应抛 FormatException");

        Console.WriteLine("✅ XunitTableDriven 所有测试用例通过！");
        Console.WriteLine("ℹ 正式项目请创建 xUnit 测试项目: dotnet new xunit");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 表驱动测试心智 ************\n");
        Console.WriteLine("演示: 一张表跑 4 组满减用例。");
        RunTests();
    }

    static void Main() => Bootstrap();
}