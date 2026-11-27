using System;
using System.Collections.Generic;
using System.Threading;

/*
[主题]: Redis 缓存陷阱——击穿/穿透/雪崩、锁与 TTL
[难度]: Hard
[前置]: 08_caching_redis

一、主题说明
缓存空值防穿透；随机 TTL 防雪崩；单飞 loading 防击穿。
陷阱：SETNX 锁不设过期 → 死锁；缓存与 DB 双写不一致。

二、写法对比
1. 直观版：Get 没有就查库、不写缓存。
2. 进阶版：负缓存、Jitter TTL、迷你分布式锁（内存模拟）。

三、真实用途
- StackExchange.Redis、IDistributedCache；生产锁用 Redlock 需理解争议。
资料: https://redis.io/docs/latest/develop/clients/dotnet/
      https://github.com/StackExchange/StackExchange.Redis
*/

sealed class FakeRedis
{
    private readonly Dictionary<string, (string? Value, DateTime Expire)> _store = new();
    private readonly object _lock = new();

    public bool Set(string key, string? value, TimeSpan ttl)
    {
        lock (_lock)
        {
            _store[key] = (value, DateTime.UtcNow.Add(ttl));
            return true;
        }
    }

    public string? Get(string key)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(key, out var entry)) return null;
            if (entry.Expire < DateTime.UtcNow)
            {
                _store.Remove(key);
                return null;
            }
            return entry.Value;
        }
    }

    public bool TryLock(string key, string token, TimeSpan ttl)
    {
        lock (_lock)
        {
            if (_store.ContainsKey(key)) return false;
            _store[key] = (token, DateTime.UtcNow.Add(ttl));
            return true;
        }
    }
}

sealed class NaiveCacheAside
{
    private readonly Dictionary<string, string> _mem = new();
    public string? Get(string key) => _mem.GetValueOrDefault(key);
}

sealed class BetterCacheAside
{
    private readonly FakeRedis _redis;
    private readonly Dictionary<string, string> _db;
    private static readonly Random Jitter = new();

    public BetterCacheAside(FakeRedis redis, Dictionary<string, string> db)
    {
        _redis = redis;
        _db = db;
    }

    public string? GetUser(string id)
    {
        var ck = $"user:{id}";
        var hit = _redis.Get(ck);
        if (hit is not null) return hit == "" ? null : hit;

        if (!_db.TryGetValue(id, out var fromDb))
        {
            _redis.Set(ck, "", TimeSpan.FromSeconds(30 + Jitter.Next(0, 10))); // 负缓存
            return null;
        }

        _redis.Set(ck, fromDb, TimeSpan.FromMinutes(5 + Jitter.Next(0, 60)));
        return fromDb;
    }
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveCacheAside();

        var redis = new FakeRedis();
        var db = new Dictionary<string, string> { ["1"] = "alice" };
        var cache = new BetterCacheAside(redis, db);

        if (cache.GetUser("1") != "alice") throw new Exception("应回源并缓存");
        if (cache.GetUser("1") != "alice") throw new Exception("应命中缓存");
        if (cache.GetUser("999") is not null) throw new Exception("不存在用户");
        if (redis.Get("user:999") != "") throw new Exception("应负缓存空占位");

        if (!redis.TryLock("lock:order", "t1", TimeSpan.FromSeconds(5)))
            throw new Exception("首次加锁应成功");
        if (redis.TryLock("lock:order", "t2", TimeSpan.FromSeconds(5)))
            throw new Exception("重复加锁应失败");

        Console.WriteLine("✅ RedisPitfalls 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Redis Pitfalls ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}