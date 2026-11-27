using System;

/*
[主题]: 可空引用类型（Nullable Reference Types）
[难度]: Medium
[前置]: 00_csharp_basics/00_hello

一、主题说明
C# 8+ 在开启 Nullable 后，引用类型默认「不应为 null」；`string?` 表示可能为 null。
编译器会警告可疑的空引用；业务上配合 `?.`、`??`、提前判空减少 NullReferenceException。

二、写法对比
1. 直观版：到处 if (x != null)，能用但啰嗦。
2. 进阶版：可空注解 + 空合并 / 空传播 + 尽早返回（guard）。

三、真实用途
- API DTO：可选字段用 `string?`；必填用 `string` 并在入口校验。
- 服务层：从缓存/DB 取不到时返回 null，调用方用 `??` 给默认值。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/
      https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/
      https://learn.microsoft.com/en-us/dotnet/csharp/linq/
*/
static class Program
{
    /** 直观版：多层 if 判空再取长度。 */
    static int DisplayNameLengthNaive(string? nick, string? legalName)
    {
        if (legalName != null)
        {
            return legalName.Length;
        }
        if (nick != null)
        {
            return nick.Length;
        }
        return "访客".Length;
    }

    /** 进阶版：空合并运算符。 */
    static int DisplayNameLengthBetter(string? nick, string? legalName) =>
        (legalName ?? nick ?? "访客").Length;

    /** 空传播：联系人可能不存在。 */
    static string? TryGetEmail(Contact? contact) => contact?.Email;

    /** 非空保证：入口 guard 后返回非空 string。 */
    static string RequireNonEmpty(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("名称不能为空");
        }
        return input;
    }

    record Contact(string Name, string? Email);

    static void RunTests()
    {
        if (DisplayNameLengthNaive(null, "张三") != 2) throw new Exception("legalName 长度");
        if (DisplayNameLengthBetter(null, "李四") != 2) throw new Exception("Better legalName");
        if (DisplayNameLengthBetter("nick", null) != 4) throw new Exception("nick 长度");
        if (DisplayNameLengthBetter(null, null) != 2) throw new Exception("默认访客");

        if (TryGetEmail(null) != null) throw new Exception("无联系人应为 null");
        if (TryGetEmail(new Contact("A", null)) != null) throw new Exception("无邮箱应为 null");
        if (TryGetEmail(new Contact("A", "a@b.c")) != "a@b.c") throw new Exception("邮箱读取");

        if (RequireNonEmpty("ok") != "ok") throw new Exception("RequireNonEmpty 正常路径");

        Console.WriteLine("✅ NullableReference 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 NullableReference ************\n");
        Console.WriteLine($"显示名长度 = {DisplayNameLengthBetter(null, "王五")}");
        Console.WriteLine($"邮箱 = {TryGetEmail(new Contact("测试", "test@example.com")) ?? "(无)"}");
        RunTests();
    }

    static void Main() => Bootstrap();
}