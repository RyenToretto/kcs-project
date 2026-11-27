using System;
using System.Collections.Generic;

/*
[主题]: DbContext 心智——工作单元（Unit of Work）+ ChangeTracker
[难度]: Medium
[前置]: 02_modern_csharp/00_linq、06_aspnet_core/01_di

一、主题说明
EF Core 的 DbContext 跟踪实体变更，SaveChanges 一次性提交。本课用内存 ChangeTracker 模拟，
不连接 SQL Server / PostgreSQL。

二、写法对比
1. 直观版：直接改字典里的对象，立刻「保存」。
2. 进阶版：Add/Update/Remove 先标记状态，SaveChanges 才落库。

三、真实用途
- 一次请求内多次改表仍是一个事务单元；DbContext 通常 Scoped 注册。
资料: https://learn.microsoft.com/en-us/ef/core/
      https://github.com/dotnet/efcore
      https://learn.microsoft.com/en-us/shows/entity-framework-core-101/
*/

enum EntityState { Unchanged, Added, Modified, Deleted }

record BlogPost(int Id, string Title, string Body);

/** 直观版：改完立刻写回，无变更跟踪概念。 */
sealed class NaiveStore
{
    private readonly Dictionary<int, BlogPost> _rows = new();

    public void UpsertImmediate(BlogPost post) => _rows[post.Id] = post;
}

sealed class TrackedEntry
{
    public required BlogPost Entity { get; set; }
    public EntityState State { get; set; } = EntityState.Unchanged;
}

/** 进阶版：迷你 DbContext + ChangeTracker。 */
sealed class MiniDbContext
{
    private readonly Dictionary<int, BlogPost> _database = new();
    private readonly Dictionary<int, TrackedEntry> _tracker = new();

    public BlogPost Add(BlogPost post)
    {
        var entry = new TrackedEntry { Entity = post, State = EntityState.Added };
        _tracker[post.Id] = entry;
        return post;
    }

    public void Update(BlogPost post)
    {
        if (!_tracker.ContainsKey(post.Id))
            _tracker[post.Id] = new TrackedEntry { Entity = post };
        _tracker[post.Id].Entity = post;
        _tracker[post.Id].State = EntityState.Modified;
    }

    public void Remove(int id)
    {
        if (_tracker.TryGetValue(id, out var entry))
        {
            entry.State = EntityState.Deleted;
            return;
        }
        if (_database.ContainsKey(id))
            _tracker[id] = new TrackedEntry { Entity = _database[id], State = EntityState.Deleted };
    }

    public int SaveChanges()
    {
        var count = 0;
        foreach (var (_, entry) in _tracker)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    _database[entry.Entity.Id] = entry.Entity;
                    entry.State = EntityState.Unchanged;
                    count++;
                    break;
                case EntityState.Deleted:
                    _database.Remove(entry.Entity.Id);
                    entry.State = EntityState.Unchanged;
                    count++;
                    break;
            }
        }
        _tracker.Clear();
        return count;
    }

    public BlogPost? Find(int id) =>
        _tracker.TryGetValue(id, out var t) ? t.Entity : _database.GetValueOrDefault(id);
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveStore();
        naive.UpsertImmediate(new BlogPost(1, "t", "b"));

        var ctx = new MiniDbContext();
        ctx.Add(new BlogPost(1, "First", "body"));
        if (ctx.Find(1) is null) throw new Exception("Add 后应能 Find");
        if (ctx.SaveChanges() != 1) throw new Exception("SaveChanges 应写入 1 条");

        ctx.Update(new BlogPost(1, "First-edited", "body"));
        if (ctx.SaveChanges() != 1) throw new Exception("Update SaveChanges 失败");
        if (ctx.Find(1)!.Title != "First-edited") throw new Exception("应持久化修改");

        ctx.Remove(1);
        ctx.SaveChanges();
        if (ctx.Find(1) is not null) throw new Exception("Remove 后应不存在");

        Console.WriteLine("✅ DbContextMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 DbContext 心智 ************\n");
        Console.WriteLine("⚠ 内存 ChangeTracker；真实 EF 用 DbSet<T> + 关系数据库。");

        var ctx = new MiniDbContext();
        ctx.Add(new BlogPost(42, "EF intro", "offline"));
        Console.WriteLine($"待提交变更: SaveChanges 写入 {ctx.SaveChanges()} 行");
        RunTests();
    }

    static void Main() => Bootstrap();
}