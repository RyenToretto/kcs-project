using System;
using System.Collections.Generic;

/*
[主题]: 泛型 —— 类型安全的可复用容器与算法
[难度]: Medium
[前置]: 03_collections_generics/00_list_dict

一、主题说明
泛型让你在编译期写明「这个盒子装什么类型」，避免装箱与强制转换。
常见：`List<T>`、`Dictionary<TKey,TValue>`、自建 `Repository<T>`。

二、写法对比
1. 直观版：用 object / ArrayList（已过时，运行时转型易错）。
2. 进阶版：泛型类/方法 + 约束 `where T : IComparable<T>`。

三、真实用途
- 仓储接口 `IReadOnlyList<T>` 返回任意实体。
- 工具方法 `Max<T>(T a, T b) where T : IComparable<T>` 复用在 int、decimal、string。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/
      https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/
      https://learn.microsoft.com/en-us/dotnet/api/
*/
static class Program
{
    /** 直观版：object 列表取最大值（需转型）。 */
    static int MaxIntNaive(List<object> values)
    {
        int max = int.MinValue;
        foreach (var v in values)
        {
            int n = (int)v;
            if (n > max)
            {
                max = n;
            }
        }
        return max;
    }

    /** 进阶版：泛型方法 + 约束。 */
    static T MaxBetter<T>(T a, T b) where T : IComparable<T> =>
        a.CompareTo(b) >= 0 ? a : b;

    /** 泛型栈（迷你版），演示类型参数 T。 */
    class MiniStack<T>
    {
        private readonly List<T> _items = new();

        public void Push(T item) => _items.Add(item);

        public T Pop()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException("栈空");
            }
            var last = _items[^1];
            _items.RemoveAt(_items.Count - 1);
            return last;
        }

        public int Count => _items.Count;
    }

    /** 泛型接口：内存「仓储」只读列表。 */
    interface IRepository<T>
    {
        IReadOnlyList<T> GetAll();
    }

    sealed class InMemoryRepo<T> : IRepository<T>
    {
        private readonly List<T> _data;
        public InMemoryRepo(IEnumerable<T> seed) => _data = seed.ToList();
        public IReadOnlyList<T> GetAll() => _data;
    }

    static void RunTests()
    {
        var naive = MaxIntNaive(new List<object> { 3, 9, 1 });
        if (naive != 9) throw new Exception("MaxIntNaive");

        if (MaxBetter(3, 9) != 9) throw new Exception("MaxBetter int");
        if (MaxBetter("b", "a") != "b") throw new Exception("MaxBetter string");

        var stack = new MiniStack<string>();
        stack.Push("first");
        stack.Push("second");
        if (stack.Pop() != "second" || stack.Count != 1) throw new Exception("MiniStack Pop");

        var repo = new InMemoryRepo<int>(new[] { 10, 20 });
        if (repo.GetAll().Count != 2 || repo.GetAll()[1] != 20) throw new Exception("InMemoryRepo");

        Console.WriteLine("✅ Generics 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Generics ************\n");
        Console.WriteLine($"较大值 = {MaxBetter(100, 250)}");
        var repo = new InMemoryRepo<string>(new[] { "SKU-A", "SKU-B" });
        Console.WriteLine($"仓储条目数 = {repo.GetAll().Count}");
        RunTests();
    }

    static void Main() => Bootstrap();
}