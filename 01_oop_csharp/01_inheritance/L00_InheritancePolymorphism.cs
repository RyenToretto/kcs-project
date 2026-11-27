using System;

/*
[主题]: 继承与多态
[难度]: Easy
[前置]: 01_oop_csharp/00_class

一、主题说明
继承：子类复用父类成员。多态：同一调用，不同子类有不同行为（virtual / override）。
C# 默认方法不可被覆盖，必须父类 virtual、子类 override。

二、写法对比
1. 直观版：父类虚方法 + 子类 override。
2. 进阶版：抽象基类强制子类实现；优先组合而非深继承（后面接口课会强化）。

三、真实用途
- 支付渠道：AlipayPayment / WeChatPayment 共用 Payment 抽象。
- 异常层次、中间件基类等。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/
      https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/virtual
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/inheritance
*/

abstract class Shape
{
    public abstract double Area();
    public virtual string Kind() => "Shape";
}

sealed class Rectangle : Shape
{
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public override double Area() => Width * Height;
    public override string Kind() => "Rectangle";
}

sealed class Circle : Shape
{
    public double Radius { get; }

    public Circle(double radius) => Radius = radius;

    public override double Area() => Math.PI * Radius * Radius;
    public override string Kind() => "Circle";
}

static class Program
{
    /** 多态：按基类引用调用，实际跑子类实现。 */
    static string Describe(Shape shape) => $"{shape.Kind()} area={shape.Area():0.##}";

    static void RunTests()
    {
        Shape r = new Rectangle(3, 4);
        Shape c = new Circle(1);
        if (r.Area() != 12) throw new Exception("Rectangle.Area 应为 12");
        if (Math.Abs(c.Area() - Math.PI) > 1e-9) throw new Exception("Circle.Area 应为 π");
        if (!Describe(r).StartsWith("Rectangle")) throw new Exception("多态 Kind 失败");
        if (!Describe(c).StartsWith("Circle")) throw new Exception("多态 Circle 失败");
        Console.WriteLine("✅ InheritancePolymorphism 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 InheritancePolymorphism ************\n");
        Console.WriteLine(Describe(new Rectangle(2, 5)));
        Console.WriteLine(Describe(new Circle(2)));
        RunTests();
    }

    static void Main() => Bootstrap();
}
