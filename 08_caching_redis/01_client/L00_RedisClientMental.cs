using System;
using System.Collections.Generic;

/*
[主题]: Redis 客户端心智——StringGet / StringSet 与过期
[难度]: Medium
[前置]: 08_caching_redis/00_cache

一、主题说明
StackExchange.Redis 通过 IDatabase.StringGetAsync/StringSetAsync 操作键值。
本课用 FakeRedis 字典模拟，不要求安装 redis-server。

二、写法对比
1. 直观版：普通 Dictionary<string,string>。
2. 进阶版：带 NX/EX 语义（为分布式锁课铺垫）。

三、真实用途
- 会话、计数器、限流、分布式锁；见官方 .NET 客户端文档。
资料: https://redis.io/docs/latest/develop/clients/dotnet/
      https://github.com/StackExchange/StackExchange.Redis
*/

/** 直观版：无过期。 */
sealed class NaiveKvStore
{
    private readonly Dictionary<string, string> _data = new();
    public void Put(string key, string value) => _data[key] = value;
    public string? Get(string key) => _data.GetValueOrDefault(key);
}

/** 进阶版：模拟 Redis 字符串命令子集。 */
sealed class FakeRedisDatabase
{
    private readonly Dictionary<string, (string Value, DateTime? ExpireAt)> _strings = new();

    public bool StringSet(string key, string value, TimeSpan? expiry = null, bool onlyIfNotExists = false)
    {
        if (onlyIfNotExists && _strings.ContainsKey(key) && !IsExpired(key))
            return false;

        DateTime? expireAt = expiry is null ? null : DateTime.UtcNow.Add(expiry.Value);
        _strings[key] = (value, expireAt);
        return true;
    }

    public string? StringGet(string key)
    {
        if (!_strings.TryGetValue(key, out var entry)) return null;
        if (IsExpired(key))
        {
            _strings.Remove(key);
            return null;
        }
        return entry.Value;
    }

    bool IsExpired(string key)
    {
        if (!_strings.TryGetValue(key, out var entry)) return true;
        return entry.ExpireAt is not null && DateTime.UtcNow > entry.ExpireAt;
    }
}

static class Program
{
    static void RunTests()
    {
        var naive = new NaiveKvStore();
        naive.Put("k", "v");
        if (naive.Get("k") != "v") throw new Exception("Naive 失败");

        var db = new FakeRedisDatabase();
        if (!db.StringSet("session:u1", "token-abc", TimeSpan.FromHours(1)))
            throw new Exception("StringSet 应成功");
        if (db.StringGet("session:u1") != "token-abc") throw new Exception("StringGet 失败");

        if (!db.StringSet("counter", "1")) throw new Exception("set counter");
        if (db.StringSet("counter", "2", onlyIfNotExists: true))
            throw new Exception("已存在键 NX 应失败");
        if (db.StringGet("counter") != "1") throw new Exception("NX 不应覆盖");

        Console.WriteLine("✅ RedisClientMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Redis Client 心智 ************\n");
        Console.WriteLine("⚠ FakeRedisDatabase 非真实连接；对照 StackExchange.Redis IDatabase。");

        var db = new FakeRedisDatabase();
        db.StringSet("hello", "redis-offline", TimeSpan.FromMinutes(10));
        Console.WriteLine(db.StringGet("hello"));
        RunTests();
    }

    static void Main() => Bootstrap();
}