using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: 索引选择性心智——何时走索引、何时全表扫
[难度]: Hard
[前置]: 07_ef_core

一、主题说明
选择性 = 不重复值 / 总行数。低选择性列单独索引往往无效；联合索引遵循最左前缀。
陷阱：在 UUID 低基数状态列建索引期望救慢查询。

二、写法对比
1. 直观版：WHERE 有列就加 INDEX。
2. 进阶版：迷你「代价模型」比较索引查找 vs 全表扫描行数。

三、真实用途
- SQL Server/PostgreSQL EXPLAIN；EF 生成 SQL 后 DBA 复核。
资料: https://learn.microsoft.com/en-us/ef/core/
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record Row(int Id, string Status, string Email);

/** 内存「表」+ 主键/状态索引，模拟查询计划选择。 */
sealed class FakeTable
{
    private readonly List<Row> _rows = new();
    private readonly Dictionary<int, Row> _byId = new();
    private readonly Dictionary<string, List<Row>> _byStatus = new();

    public void Insert(Row row)
    {
        _rows.Add(row);
        _byId[row.Id] = row;
        if (!_byStatus.TryGetValue(row.Status, out var list))
        {
            list = new();
            _byStatus[row.Status] = list;
        }
        list.Add(row);
    }

    public Row? ByIdNaive(int id) => _rows.FirstOrDefault(r => r.Id == id);

    public Row? ByIdIndexed(int id) => _byId.TryGetValue(id, out var r) ? r : null;

    public int CountByStatusNaive(string status) =>
        _rows.Count(r => r.Status == status);

    public int CountByStatusIndexed(string status) =>
        _byStatus.TryGetValue(status, out var list) ? list.Count : 0;

    public double Selectivity(string columnValues)
    {
        var distinct = _rows.Select(r => r.Status).Distinct().Count();
        return _rows.Count == 0 ? 0 : (double)distinct / _rows.Count;
    }

    public int RowCount => _rows.Count;
}

static class Planner
{
    /** 行数小于阈值或选择性太差则「全表」更快（教学简化）。 */
    public static bool UseIndexForStatus(FakeTable table, string status, int fullScanCostPerRow = 1, int indexSeekCost = 50)
    {
        var matches = table.CountByStatusIndexed(status);
        var selective = table.Selectivity(status);
        var seqCost = table.RowCount * fullScanCostPerRow;
        var idxCost = indexSeekCost + matches;
        return selective < 0.4 && idxCost < seqCost;
    }
}

static class Program
{
    static void RunTests()
    {
        var t = new FakeTable();
        for (var i = 0; i < 5000; i++)
            t.Insert(new Row(i, i % 2 == 0 ? "Active" : "Active", $"u{i}@x.com"));

        if (t.ByIdNaive(42)?.Id != 42) throw new Exception("朴素主键");
        if (t.ByIdIndexed(42)?.Id != 42) throw new Exception("索引主键");

        if (t.CountByStatusNaive("Active") != 5000) throw new Exception("计数");
        if (t.CountByStatusIndexed("Active") != 5000) throw new Exception("索引计数");

        // 极低选择性（几乎全 Active）→ 计划器倾向全表
        if (Planner.UseIndexForStatus(t, "Active"))
            throw new Exception("低选择性应不优先索引");

        var skewed = new FakeTable();
        for (var i = 0; i < 1000; i++)
            skewed.Insert(new Row(i, i == 0 ? "VIP" : "Active", $"e{i}"));

        if (!Planner.UseIndexForStatus(skewed, "VIP"))
            throw new Exception("高选择性稀少值应用索引");

        Console.WriteLine("✅ SqlIndexExplain 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 SQL Index Explain ************\n");
        Console.WriteLine("⚠ 真实库用 EXPLAIN ANALYZE；本课为离线选择性模型。");
        RunTests();
    }

    static void Main() => Bootstrap();
}