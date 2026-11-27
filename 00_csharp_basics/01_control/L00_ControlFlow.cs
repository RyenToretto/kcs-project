using System;
using System.Text;

/*
[主题]: 控制流 —— if / switch / for / foreach / while
[难度]: Easy
[前置]: 00_csharp_basics/00_hello

一、主题说明
程序很少永远从上到下直线执行。控制流决定「什么条件下做什么、重复多少次」。
本课覆盖：条件分支（if / else、switch）、循环（for / foreach / while）。

二、写法对比
1. 直观版：经典 if-else 与 for 计数循环，好读。
2. 进阶版：C# switch 表达式（可直接返回值）与 foreach；生产代码里更常见。

三、真实用途
- 订单状态机：按状态分支处理支付 / 发货 / 取消。
- 批处理：遍历列表做校验、汇总金额。
- HTTP 状态码、错误码映射常用 switch。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/selection-statements
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/iteration-statements
      https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/
*/
static class Program
{
    /** 直观版：用 if-else 判断成绩等级。 */
    static string GradeNaive(int score)
    {
        // score 约定 0～100
        if (score >= 90)
        {
            return "A";
        }
        else if (score >= 80)
        {
            return "B";
        }
        else if (score >= 60)
        {
            return "C";
        }
        else
        {
            return "D";
        }
    }

    /** 进阶版：switch 表达式，直接作为返回值。 */
    static string GradeBetter(int score)
    {
        int band = score / 10; // 90～100 → 9 或 10
        return band switch
        {
            10 or 9 => "A",
            8 => "B",
            7 or 6 => "C",
            _ => "D",
        };
    }

    /** 直观版：for 循环求 1..n 的和。 */
    static int SumToNaive(int n)
    {
        int sum = 0;
        for (int i = 1; i <= n; i++)
        {
            sum += i;
        }
        return sum;
    }

    /** 进阶版：等差数列公式（循环的对照）。 */
    static int SumToBetter(int n) => n * (n + 1) / 2;

    /** while：倒计数直到 0，返回拼接字符串，便于自测。 */
    static string Countdown(int from)
    {
        var sb = new StringBuilder();
        int i = from;
        while (i > 0)
        {
            sb.Append(i);
            if (i > 1)
            {
                sb.Append(',');
            }
            i--;
        }
        return sb.ToString();
    }

    /** foreach：遍历数组求和。 */
    static int SumArray(int[] nums)
    {
        int sum = 0;
        foreach (int num in nums)
        {
            sum += num;
        }
        return sum;
    }

    static void RunTests()
    {
        if (GradeNaive(95) != "A") throw new Exception("GradeNaive(95) 应为 A");
        if (GradeNaive(72) != "C") throw new Exception("GradeNaive(72) 应为 C");
        if (GradeBetter(100) != "A") throw new Exception("GradeBetter(100) 应为 A");
        if (GradeBetter(55) != "D") throw new Exception("GradeBetter(55) 应为 D");
        if (SumToNaive(10) != 55) throw new Exception("SumToNaive(10) 应为 55");
        if (SumToBetter(10) != 55) throw new Exception("SumToBetter(10) 应为 55");
        if (Countdown(3) != "3,2,1") throw new Exception("Countdown(3) 应为 3,2,1");
        if (SumArray(new[] { 1, 2, 3, 4 }) != 10) throw new Exception("SumArray 应为 10");
        Console.WriteLine("✅ ControlFlow 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 ControlFlow ************\n");
        Console.WriteLine($"成绩 88 → {GradeBetter(88)}");
        Console.WriteLine($"1..10 求和 = {SumToNaive(10)}");
        Console.WriteLine($"倒计时: {Countdown(5)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}
