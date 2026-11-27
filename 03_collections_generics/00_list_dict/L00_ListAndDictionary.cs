using System;
using System.Collections.Generic;

/*
[主题]: List 与 Dictionary —— 最常用集合怎么选
[难度]: Easy
[前置]: 00_csharp_basics/03_strings

一、主题说明
List<T>：有序、可按索引、允许重复，适合「一串同类元素」。
Dictionary<TKey,TValue>：按键 O(1) 查找，适合「唯一键 → 值」映射（缓存、索引表）。

二、写法对比
1. 直观版：用 List 存键值对再线性查找（小数据可以，大数据慢）。
2. 进阶版：Dictionary 建索引，TryGetValue 安全读取。

三、真实用途
- 购物车：List 存行项目；SKU → 单价用 Dictionary。
- 内存缓存：用户 Id → 会话对象。
资料: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/
      https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/
      https://learn.microsoft.com/en-us/dotnet/api/
*/
static class Program
{
    record CartLine(string Sku, int Qty);

    /** 直观版：在 List 里按 Sku 找价格（O(n)）。 */
    static decimal LookupPriceNaive(List<(string Sku, decimal Price)> catalog, string sku)
    {
        foreach (var item in catalog)
        {
            if (item.Sku == sku)
            {
                return item.Price;
            }
        }
        return 0m;
    }

    /** 进阶版：Dictionary 索引。 */
    static Dictionary<string, decimal> BuildCatalogMap(IEnumerable<(string Sku, decimal Price)> items)
    {
        var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            map[item.Sku] = item.Price;
        }
        return map;
    }

    static decimal LookupPriceBetter(Dictionary<string, decimal> map, string sku) =>
        map.TryGetValue(sku, out var price) ? price : 0m;

    /** List：维护购物车行并求总价。 */
    static decimal CartTotal(List<CartLine> lines, Dictionary<string, decimal> prices)
    {
        decimal total = 0;
        foreach (var line in lines)
        {
            total += line.Qty * LookupPriceBetter(prices, line.Sku);
        }
        return total;
    }

    static void RunTests()
    {
        var catalogList = new List<(string, decimal)>
        {
            ("A001", 10m),
            ("B002", 25.5m),
        };
        if (LookupPriceNaive(catalogList, "B002") != 25.5m) throw new Exception("Naive 查找");
        if (LookupPriceNaive(catalogList, "X") != 0m) throw new Exception("Naive 未找到");

        var map = BuildCatalogMap(catalogList);
        if (LookupPriceBetter(map, "a001") != 10m) throw new Exception("字典应忽略大小写");
        if (LookupPriceBetter(map, "B002") != 25.5m) throw new Exception("Better 查找");

        var cart = new List<CartLine>
        {
            new("A001", 2),
            new("B002", 1),
        };
        if (CartTotal(cart, map) != 45.5m) throw new Exception("购物车总价");

        Console.WriteLine("✅ ListAndDictionary 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 ListAndDictionary ************\n");
        var map = BuildCatalogMap(new[] { ("SKU-1", 99m), ("SKU-2", 19m) });
        var cart = new List<CartLine> { new("SKU-1", 1), new("SKU-2", 3) };
        Console.WriteLine($"购物车合计 ¥{CartTotal(cart, map)}");
        RunTests();
    }

    static void Main() => Bootstrap();
}