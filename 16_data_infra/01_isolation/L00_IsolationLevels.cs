using System;
using System.Collections.Generic;

/*
[主题]: 事务隔离级别——脏读/不可重复读/幻读
[难度]: Hard
[前置]: 07_ef_core

一、主题说明
Read Uncommitted 可读未提交；Repeatable Read 锁定读；Serializable 最严。
许多云数据库默认 RC/Snapshot——与教科书 Serializable 不同。

二、写法对比
1. 直观版：无事务 long-running 读。
2. 进阶版：内存 DB 模拟两事务交错与隔离行为。

三、真实用途
- EF Database.BeginTransaction(IsolationLevel.ReadCommitted)；报表用快照或只读副本。
资料: https://learn.microsoft.com/en-us/ef/core/
*/

enum IsolationLevel { ReadUncommitted, ReadCommitted, RepeatableRead, Serializable }

sealed class InMemoryDb
{
    private readonly Dictionary<int, decimal> _balances = new();
    private readonly object _lock = new();
    public IsolationLevel Level { get; init; } = IsolationLevel.ReadCommitted;

    public void Seed(int id, decimal balance) { lock (_lock) _balances[id] = balance; }

    public decimal? Read(int id, int txnId, Dictionary<int, decimal>? snapshot)
    {
        lock (_lock)
        {
            if (Level == IsolationLevel.RepeatableRead && snapshot is not null)
            {
                if (!snapshot.ContainsKey(id))
                    snapshot[id] = _balances[id];
                return snapshot[id];
            }
            return _balances.TryGetValue(id, out var v) ? v : null;
        }
    }

    public void Write(int id, decimal value)
    {
        lock (_lock) _balances[id] = value;
    }
}

static class Program
{
    static void RunTests()
    {
        var db = new InMemoryDb { Level = IsolationLevel.ReadCommitted };
        db.Seed(1, 100m);
        db.Write(1, 80m);
        if (db.Read(1, 0, null) != 80m) throw new Exception("读已提交应见最新");

        var rr = new InMemoryDb { Level = IsolationLevel.RepeatableRead };
        rr.Seed(2, 50m);
        var snap = new Dictionary<int, decimal>();
        var first = rr.Read(2, 1, snap);
        rr.Write(2, 10m);
        var second = rr.Read(2, 1, snap);
        if (first != second || first != 50m) throw new Exception("可重复读应固定快照读");

        Console.WriteLine("✅ IsolationLevels 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Isolation Levels ************\n");
        Console.WriteLine("⚠ SQL Server 默认 RC；PostgreSQL 默认 RR（快照）——以云厂商文档为准。");
        RunTests();
    }

    static void Main() => Bootstrap();
}