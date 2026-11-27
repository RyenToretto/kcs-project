using System;

/*
[主题]: 封装与访问修饰符
[难度]: Easy
[前置]: 01_oop_csharp/00_class

一、主题说明
封装：把状态藏在对象内部，只通过受控方法修改，避免外部随意改坏不变量。
访问修饰符：public / private / protected / internal / protected internal / private protected。

二、写法对比
1. 直观版：余额字段 private，存取款方法校验。
2. 进阶版：不可变余额 + 返回新对象（更函数式）；或用属性私有 set。

三、真实用途
- 订单金额、库存数量绝不能被外部随意赋值。
- 领域驱动设计里「聚合根保护不变量」就是封装。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers
      https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
      https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
*/

/** 直观版：可变余额，方法内校验。 */
sealed class BankAccountNaive
{
    private decimal _balance;

    public BankAccountNaive(decimal opening)
    {
        if (opening < 0) throw new ArgumentOutOfRangeException(nameof(opening));
        _balance = opening;
    }

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > _balance) throw new InvalidOperationException("余额不足");
        _balance -= amount;
    }
}

/** 进阶版：每次操作返回新账户（教学用不可变封装）。 */
sealed class BankAccountBetter
{
    public decimal Balance { get; }

    public BankAccountBetter(decimal balance)
    {
        if (balance < 0) throw new ArgumentOutOfRangeException(nameof(balance));
        Balance = balance;
    }

    public BankAccountBetter Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new BankAccountBetter(Balance + amount);
    }

    public BankAccountBetter Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > Balance) throw new InvalidOperationException("余额不足");
        return new BankAccountBetter(Balance - amount);
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new BankAccountNaive(100m);
        naive.Deposit(50m);
        naive.Withdraw(30m);
        if (naive.Balance != 120m) throw new Exception("Naive 余额应为 120");

        try
        {
            naive.Withdraw(1000m);
            throw new Exception("应因余额不足失败");
        }
        catch (InvalidOperationException)
        {
            // 预期
        }

        var better = new BankAccountBetter(100m).Deposit(20m).Withdraw(10m);
        if (better.Balance != 110m) throw new Exception("Better 余额应为 110");
        Console.WriteLine("✅ Encapsulation 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Encapsulation ************\n");
        var acc = new BankAccountNaive(10m);
        acc.Deposit(5m);
        Console.WriteLine($"余额 = {acc.Balance}");
        RunTests();
    }

    static void Main() => Bootstrap();
}
