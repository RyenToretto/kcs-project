using System;

/*
[主题]: 第一个 C# 程序：Main、基本类型与自测结构
[难度]: Easy
[前置]: 无（C# 小白第一课）

一、主题说明
C# 程序通常从 Main 入口开始。本课建立「写逻辑 → Bootstrap 演示 → RunTests 断言」的学习习惯。
工具链：.NET SDK；本课为可执行控制台项目。

二、写法对比
1. 直观版 AddNaive: 直接用 +。
2. 进阶版 AddBetter: 同样加法，强调命名与可测函数拆分。

三、真实用途
- 所有控制台、Web、Worker 都有入口。
- 数值类型选择在金额/计数场景很重要（金额常用 decimal）。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/
      https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/
      https://learn.microsoft.com/en-us/shows/csharp-for-beginners/
*/
static class Program
{
    static int AddNaive(int left, int right) => left + right;

    static int AddBetter(int left, int right) => left + right;

    static string DescribePrimitives()
    {
        int age = 18;
        double price = 19.9;
        bool ok = true;
        char grade = 'A';
        decimal money = 19.9m;
        return $"age={age}, price={price}, ok={ok}, grade={grade}, money={money}";
    }

    static void RunTests()
    {
        if (AddNaive(1, 2) != 3) throw new Exception("AddNaive(1,2) 应为 3");
        if (AddBetter(2, 3) != 5) throw new Exception("AddBetter(2,3) 应为 5");
        if (!DescribePrimitives().Contains("age=18")) throw new Exception("应包含 age=18");
        Console.WriteLine("✅ HelloAndTypes 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 HelloAndTypes ************\n");
        Console.WriteLine("基本类型示例: " + DescribePrimitives());
        Console.WriteLine("1 + 2 = " + AddNaive(1, 2));
        RunTests();
    }

    static void Main() => Bootstrap();
}
