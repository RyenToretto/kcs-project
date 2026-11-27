using System;

/*
[主题]: Span<T> 直觉 —— 切片而不必复制整块数组
[难度]: Medium
[前置]: 00_csharp_basics/03_strings

一、主题说明
Span<T> 是对连续内存（数组、栈上缓冲）的「窗口」：可切片、少分配。
本课只做入门直觉；深度性能/GC 见后续 `17_clr_runtime`。

二、写法对比
1. 直观版：Substring / Array.Copy 产生新缓冲。
2. 进阶版：ReadOnlySpan<char> 切片解析，配合 stackalloc 小缓冲。

三、真实用途
- 协议解析：从 byte[] 上切出头部，不必复制整包。
- 高性能日志/CSV：在 char span 上找分隔符。
资料: https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/
      https://learn.microsoft.com/en-us/dotnet/api/
*/
static class Program
{
    /** 直观版：Substring 取「@」前的用户名。 */
    static string UserNameNaive(string email)
    {
        int at = email.IndexOf('@');
        if (at < 0)
        {
            return email;
        }
        return email.Substring(0, at);
    }

    /** 进阶版：ReadOnlySpan<char> 切片（无新字符串直到 ToString）。 */
    static ReadOnlySpan<char> UserNameSpan(ReadOnlySpan<char> email)
    {
        int at = email.IndexOf('@');
        return at < 0 ? email : email[..at];
    }

    /** 在 ReadOnlySpan 上统计分隔符数量（模拟 CSV 字段数-1）。 */
    static int CountDelimiter(ReadOnlySpan<char> line, char delimiter)
    {
        int count = 0;
        foreach (var ch in line)
        {
            if (ch == delimiter)
            {
                count++;
            }
        }
        return count;
    }

    /** stackalloc：小栈缓冲拼 traceId，减少堆分配（演示用）。 */
    static string BuildTraceId(int worker, int seq)
    {
        Span<char> buf = stackalloc char[16];
        var prefix = "TR".AsSpan();
        prefix.CopyTo(buf);
        int written = prefix.Length;
        worker.ToString().AsSpan().CopyTo(buf[written..]);
        written += worker.ToString().Length;
        buf[written++] = '-';
        seq.ToString().AsSpan().CopyTo(buf[written..]);
        written += seq.ToString().Length;
        return new string(buf[..written]);
    }

    static void RunTests()
    {
        const string mail = "dev@corp.com";
        if (UserNameNaive(mail) != "dev") throw new Exception("Naive 用户名");
        if (UserNameSpan(mail.AsSpan()).ToString() != "dev") throw new Exception("Span 用户名");

        if (CountDelimiter("a,b,c".AsSpan(), ',') != 2) throw new Exception("分隔符计数");

        var id = BuildTraceId(3, 42);
        if (!id.StartsWith("TR3-42", StringComparison.Ordinal)) throw new Exception("TraceId 格式");

        Console.WriteLine("✅ SpanIntro 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 SpanIntro ************\n");
        var email = "alice@example.com".AsSpan();
        Console.WriteLine($"用户名 = {UserNameSpan(email)}");
        Console.WriteLine($"CSV 逗号数 = {CountDelimiter("id,name,price".AsSpan(), ',')}");
        Console.WriteLine($"trace = {BuildTraceId(1, 7)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}