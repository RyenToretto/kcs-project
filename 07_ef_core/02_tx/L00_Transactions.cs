using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: 数据库事务——要么全部成功，要么全部回滚
[难度]: Medium
[前置]: 07_ef_core/00_dbcontext

一、主题说明
转账、下单减库存需要原子性。EF 可用 BeginTransaction；本课用假事务日志模拟 Commit/Rollback。

二、写法对比
1. 直观版：两次 SaveChanges，中间失败会留下脏数据。
2. 进阶版：事务块内多次操作，失败 Rollback 恢复原状。

三、真实用途
- 与 DbContext SaveChanges 配合；分布式事务另议（Outbox / Saga）。
资料: https://learn.microsoft.com/en-us/ef/core/
      https://github.com/dotnet/efcore
      https://learn.microsoft.com/en-us/shows/entity-framework-core-101/
*/

record Account(int Id, decimal Balance);

/** 直观版：只扣款不加款，模拟半提交脏状态。 */
sealed class NaiveTransfer
{
    private readonly Dictionary<int, Account> _accounts = new();

    public void Seed(Account a) => _accounts[a.Id] = a;

    public void BrokenTransfer(int from, int to, decimal amount)
    {
        _accounts[from] = _accounts[from] with { Balance = _accounts[from].Balance - amount };
        // 本应给 to 加款，此处故意省略，演示无事务时的不一致
    }
}

/** 内存账户表，支持快照恢复。 */
sealed class AccountDb
{
    private readonly Dictionary<int, Account> _live = new();

    public void Reset(IEnumerable<Account> rows)
    {
        _live.Clear();
        foreach (var a in rows) _live[a.Id] = a;
    }

    public Account Get(int id) => _live[id];

    public void Update(Account account) => _live[account.Id] = account;

    public Dictionary<int, Account> Snapshot() => _live.ToDictionary(kv => kv.Key, kv => kv.Value);

    public void Restore(Dictionary<int, Account> snap)
    {
        _live.Clear();
        foreach (var kv in snap) _live[kv.Key] = kv.Value;
    }
}

sealed class MiniTransaction
{
    private readonly AccountDb _db;
    private Dictionary<int, Account>? _snapshot;
    public MiniTransaction(AccountDb db) => _db = db;

    public void Begin() => _snapshot = _db.Snapshot();

    public void Commit() => _snapshot = null;

    public void Rollback()
    {
        if (_snapshot is not null)
            _db.Restore(_snapshot);
        _snapshot = null;
    }
}

sealed class TransactionalTransferService
{
    private readonly AccountDb _db;

    public TransactionalTransferService(AccountDb db) => _db = db;

    public bool Transfer(int from, int to, decimal amount, bool simulateFailure)
    {
        var tx = new MiniTransaction(_db);
        tx.Begin();
        try
        {
            if (amount <= 0) throw new ArgumentException("amount");
            var src = _db.Get(from);
            var dst = _db.Get(to);
            if (src.Balance < amount) throw new InvalidOperationException("余额不足");
            _db.Update(src with { Balance = src.Balance - amount });
            _db.Update(dst with { Balance = dst.Balance + amount });
            if (simulateFailure) throw new InvalidOperationException("模拟下游失败");
            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }
}

static class Program
{
    static void Seed(AccountDb db) =>
        db.Reset(new[] { new Account(1, 100m), new Account(2, 20m) });

    static void RunTests()
    {
        var naive = new NaiveTransfer();
        naive.Seed(new Account(1, 100m));
        naive.Seed(new Account(2, 20m));
        naive.BrokenTransfer(1, 2, 30m);
        // from 已扣，to 未加 —— 脏状态（演示风险，无断言）

        var db = new AccountDb();
        var svc = new TransactionalTransferService(db);

        Seed(db);
        if (!svc.Transfer(1, 2, 30m, simulateFailure: false)) throw new Exception("正常转账应成功");
        if (db.Get(1).Balance != 70m || db.Get(2).Balance != 50m) throw new Exception("余额错误");

        Seed(db);
        if (svc.Transfer(1, 2, 500m, simulateFailure: false)) throw new Exception("余额不足应失败");
        if (db.Get(1).Balance != 100m) throw new Exception("失败应 Rollback");

        Seed(db);
        if (svc.Transfer(1, 2, 10m, simulateFailure: true)) throw new Exception("模拟失败应 Rollback");
        if (db.Get(1).Balance != 100m || db.Get(2).Balance != 20m) throw new Exception("Rollback 后应还原");

        Console.WriteLine("✅ Transactions 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Transactions ************\n");
        Console.WriteLine("⚠ 假事务快照；真实 EF 用 Database.BeginTransaction()。");

        var db = new AccountDb();
        Seed(db);
        var ok = new TransactionalTransferService(db).Transfer(1, 2, 5m, false);
        Console.WriteLine($"转账结果: {ok}；账户2余额={db.Get(2).Balance}");
        RunTests();
    }

    static void Main() => Bootstrap();
}