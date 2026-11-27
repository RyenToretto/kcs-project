using System;
using System.Text;

/*
[主题]: 字符串、插值与数组
[难度]: Easy
[前置]: 00_csharp_basics/00_hello、02_methods

一、主题说明
字符串与数组是每天最高频的数据结构之一。C# 的 string 不可变；大量拼接应优先用 StringBuilder 或插值。
数组是固定长度的连续存储；可变长度列表留到 03_collections_generics。

二、写法对比
1. 直观版：+ 拼接、索引访问数组。
2. 进阶版：$ 插值、StringBuilder、Span 切分直觉（本课只点到，深挖在 03_/17_）。

三、真实用途
- 日志、错误信息、API 响应文案。
- 配置 key、路由模板、缓存 key 拼接。
- 批处理输入常是 string[] args 或文件行数组。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/strings/
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays
      https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder
*/
static class Program
{
    /** 直观版：用 + 拼接问候语。 */
    static string GreetNaive(string name) => "Hello, " + name + "!";

    /** 进阶版：字符串插值（日常首选）。 */
    static string GreetBetter(string name) => $"Hello, {name}!";

    /** 直观版：循环 + 拼接（大量循环时会产生很多临时字符串）。 */
    static string JoinNaive(string[] parts)
    {
        string result = "";
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0) result += ",";
            result += parts[i];
        }
        return result;
    }

    /** 进阶版：StringBuilder；或直接用 string.Join。 */
    static string JoinBetter(string[] parts)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(parts[i]);
        }
        return sb.ToString();
    }

    static string JoinBest(string[] parts) => string.Join(",", parts);

    /** 数组：求最大值（直观 for）。 */
    static int MaxNaive(int[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException("空数组");
        int max = nums[0];
        for (int i = 1; i < nums.Length; i++)
        {
            if (nums[i] > max) max = nums[i];
        }
        return max;
    }

    /** 切片直觉：取前 n 个元素组成新数组（教学用；生产可用 Array.Copy / LINQ Take）。 */
    static int[] TakeFirst(int[] nums, int n)
    {
        if (n < 0 || n > nums.Length) throw new ArgumentOutOfRangeException(nameof(n));
        var result = new int[n];
        Array.Copy(nums, result, n);
        return result;
    }

    /** 常用字符串 API。 */
    static bool ContainsIgnoreCase(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    static void RunTests()
    {
        if (GreetNaive("Ada") != "Hello, Ada!") throw new Exception("GreetNaive 失败");
        if (GreetBetter("Ada") != "Hello, Ada!") throw new Exception("GreetBetter 失败");
        var parts = new[] { "a", "b", "c" };
        if (JoinNaive(parts) != "a,b,c") throw new Exception("JoinNaive 失败");
        if (JoinBetter(parts) != "a,b,c") throw new Exception("JoinBetter 失败");
        if (JoinBest(parts) != "a,b,c") throw new Exception("JoinBest 失败");
        if (MaxNaive(new[] { 3, 9, 1, 7 }) != 9) throw new Exception("MaxNaive 失败");
        var taken = TakeFirst(new[] { 1, 2, 3, 4 }, 2);
        if (taken.Length != 2 || taken[0] != 1 || taken[1] != 2) throw new Exception("TakeFirst 失败");
        if (!ContainsIgnoreCase("Hello World", "WORLD")) throw new Exception("ContainsIgnoreCase 失败");
        if ("  hi  ".Trim() != "hi") throw new Exception("Trim 失败");
        Console.WriteLine("✅ StringsAndArrays 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 StringsAndArrays ************\n");
        Console.WriteLine(GreetBetter("C#"));
        Console.WriteLine($"Join = {JoinBest(new[] { "red", "green", "blue" })}");
        Console.WriteLine($"Max = {MaxNaive(new[] { 5, 2, 8 })}");
        RunTests();
    }

    static void Main() => Bootstrap();
}
