using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: LINQ 查询——IQueryable（延迟、可组合）vs IEnumerable（内存立刻执行）
[难度]: Medium
[前置]: 02_modern_csharp/00_linq

一、主题说明
EF 把 IQueryable 表达式翻译成 SQL；在内存集合上演练同一区别，避免误以为 LINQ 总在内存跑。
本课不引用 EF NuGet，用 MiniQueryable 模拟 Provider。

二、写法对比
1. 直观版：IEnumerable 链式 Where 立即过滤。
2. 进阶版：IQueryable 只记录表达式，Execute 时才真正查询。

三、真实用途
- 在 Service 层继续 .Where/.OrderBy 直到分页前才 ToListAsync，减少数据库往返。
资料: https://learn.microsoft.com/en-us/ef/core/
      https://github.com/dotnet/efcore
      https://learn.microsoft.com/en-us/shows/entity-framework-core-101/
*/

record Customer(int Id, string City, decimal Balance);

/** 直观版：内存 Where，立刻枚举。 */
static class NaiveQuery
{
    public static IEnumerable<Customer> FilterByCity(IEnumerable<Customer> source, string city) =>
        source.Where(c => c.City == city).ToList();
}

/** 迷你 IQueryable：记录委托，延迟执行。 */
sealed class MiniQueryable<T>
{
    private readonly Func<IEnumerable<T>, IEnumerable<T>> _pipeline;

    public MiniQueryable(IEnumerable<T> source)
    {
        Source = source;
        _pipeline = s => s;
    }

    private MiniQueryable(IEnumerable<T> source, Func<IEnumerable<T>, IEnumerable<T>> pipeline)
    {
        Source = source;
        _pipeline = pipeline;
    }

    public IEnumerable<T> Source { get; }

    public MiniQueryable<T> Where(Func<T, bool> predicate) =>
        new(Source, s => _pipeline(s).Where(predicate));

    public MiniQueryable<T> OrderByDescending(Func<T, decimal> keySelector) =>
        new(Source, s => _pipeline(s).OrderByDescending(keySelector));

    public List<T> ToList() => _pipeline(Source).ToList();

    public string Explain() => "IN-MEMORY QUERY (真实 EF 此处为 SQL)";
}

static class Program
{
    static readonly Customer[] Seed =
    {
        new(1, "Shanghai", 100m),
        new(2, "Shanghai", 50m),
        new(3, "Beijing", 200m),
    };

    static void RunTests()
    {
        var naive = NaiveQuery.FilterByCity(Seed, "Shanghai");
        if (naive.Count() != 2) throw new Exception("IEnumerable 过滤失败");

        var query = new MiniQueryable<Customer>(Seed)
            .Where(c => c.City == "Shanghai")
            .OrderByDescending(c => c.Balance);

        var list = query.ToList();
        if (list.Count != 2 || list[0].Id != 1) throw new Exception("IQueryable 排序失败");
        if (!query.Explain().Contains("SQL")) throw new Exception("Explain 提示失败");

        // 演示：在 ToList 前可继续叠加条件（类似 EF 组合查询）
        var composed = new MiniQueryable<Customer>(Seed)
            .Where(c => c.Balance >= 100)
            .Where(c => c.City == "Beijing");
        if (composed.ToList().Count != 1) throw new Exception("组合 Where 失败");

        Console.WriteLine("✅ LinqToEntities 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 LINQ to Entities 心智 ************\n");
        Console.WriteLine("⚠ MiniQueryable 模拟延迟执行；EF Core 翻译为 SQL。");

        var q = new MiniQueryable<Customer>(Seed).Where(c => c.City == "Shanghai");
        Console.WriteLine(q.Explain());
        Console.WriteLine(string.Join(", ", q.ToList().Select(c => c.Id)));
        RunTests();
    }

    static void Main() => Bootstrap();
}