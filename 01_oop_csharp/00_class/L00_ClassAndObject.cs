using System;

/*
[主题]: 类与对象、属性与构造
[难度]: Easy
[前置]: 00_csharp_basics 全部

一、主题说明
类（class）是数据 + 行为的蓝图；对象（object）是类的实例。
C# 后端几乎一切领域模型都是类：Order、User、Product。

二、写法对比
1. 直观版：字段 + 公开方法读写。
2. 进阶版：自动属性、构造函数、只读属性、对象初始化器。

三、真实用途
- DTO / Entity / 领域对象。
- ASP.NET Core 里注入的服务本身也是类。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
*/

/** 直观版：用字段 + 方法。 */
sealed class ProductNaive
{
    public string Name;
    public decimal Price;

    public ProductNaive(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public string Describe() => Name + ":" + Price;
}

/** 进阶版：自动属性 + 只读 Id。 */
sealed class ProductBetter
{
    public string Id { get; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public ProductBetter(string id, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id 不能为空");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Id = id;
        Name = name;
        Price = price;
    }

    public string Describe() => $"{Id}:{Name}:{Price:0.00}";
}

static class Program
{
    static void RunTests()
    {
        var naive = new ProductNaive("Book", 29.9m);
        if (naive.Describe() != "Book:29.9") throw new Exception("ProductNaive.Describe 失败");

        var better = new ProductBetter("p1", "Book", 29.9m) { Name = "C# Book" };
        if (better.Id != "p1") throw new Exception("Id 应为 p1");
        if (!better.Describe().Contains("C# Book")) throw new Exception("Name 更新失败");
        if (!better.Describe().EndsWith("29.90")) throw new Exception("价格格式失败");

        try
        {
            _ = new ProductBetter("", "x", 1m);
            throw new Exception("空 id 应抛异常");
        }
        catch (ArgumentException)
        {
            // 预期
        }

        Console.WriteLine("✅ ClassAndObject 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 ClassAndObject ************\n");
        var p = new ProductBetter("sku-1", "Keyboard", 199m);
        Console.WriteLine(p.Describe());
        RunTests();
    }

    static void Main() => Bootstrap();
}
