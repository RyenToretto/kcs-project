using System;

/*
[主题]: 模式匹配 —— is / switch 表达式 / 类型模式
[难度]: Medium
[前置]: 00_csharp_basics/01_control

一、主题说明
模式匹配让你用声明式语法判断「值长什么样」，而不仅是相等比较。
常见于：多态分发、解析 JSON/消息、状态机分支。

二、写法对比
1. 直观版：if + typeof / as + null 检查。
2. 进阶版：switch 表达式、属性模式、关系模式（>=）。

三、真实用途
- 支付回调：按消息类型分支到不同处理器。
- HTTP 管道：根据异常类型映射状态码。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/
      https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/
      https://learn.microsoft.com/en-us/dotnet/csharp/linq/
*/
static class Program
{
    abstract record PaymentMessage;
    record PaySuccess(int OrderId, decimal Amount) : PaymentMessage;
    record PayFailed(int OrderId, string Reason) : PaymentMessage;
    record PayTimeout(int OrderId) : PaymentMessage;

    /** 直观版：if + is 类型判断 + 强转。 */
    static string DescribeNaive(PaymentMessage msg)
    {
        if (msg is PaySuccess success)
        {
            return $"成功 订单{success.OrderId} 金额{success.Amount}";
        }
        if (msg is PayFailed failed)
        {
            return $"失败 订单{failed.OrderId} {failed.Reason}";
        }
        if (msg is PayTimeout timeout)
        {
            return $"超时 订单{timeout.OrderId}";
        }
        return "未知";
    }

    /** 进阶版：switch 表达式 + 属性模式。 */
    static string DescribeBetter(PaymentMessage msg) => msg switch
    {
        PaySuccess { Amount: >= 1000m } s => $"大额成功 #{s.OrderId} ¥{s.Amount}",
        PaySuccess s => $"成功 #{s.OrderId} ¥{s.Amount}",
        PayFailed f => $"失败 #{f.OrderId}: {f.Reason}",
        PayTimeout t => $"超时 #{t.OrderId}",
        _ => "未知",
    };

    /** 关系模式：按分数段给等级（与控制流课呼应）。 */
    static string GradeByPattern(int score) => score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 60 => "C",
        _ => "D",
    };

    static void RunTests()
    {
        var s = new PaySuccess(1, 1500m);
        var f = new PayFailed(2, "余额不足");
        var t = new PayTimeout(3);

        if (!DescribeNaive(s).Contains("成功")) throw new Exception("Naive 成功");
        if (!DescribeBetter(s).Contains("大额")) throw new Exception("Better 大额");
        if (!DescribeBetter(f).Contains("失败")) throw new Exception("Better 失败");
        if (!DescribeBetter(t).Contains("超时")) throw new Exception("Better 超时");

        if (GradeByPattern(95) != "A") throw new Exception("Grade A");
        if (GradeByPattern(40) != "D") throw new Exception("Grade D");

        Console.WriteLine("✅ PatternMatching 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 PatternMatching ************\n");
        Console.WriteLine(DescribeBetter(new PaySuccess(99, 88m)));
        Console.WriteLine(DescribeBetter(new PayFailed(99, "风控拦截")));
        Console.WriteLine($"分数 76 → {GradeByPattern(76)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}