using System;
using System.Collections.Generic;

/*
[主题]: 分布式锁入门——SET key value NX EX（互斥 + 过期）
[难度]: Medium
[前置]: 08_caching_redis/01_client

一、主题说明
多实例同时抢同一资源时需要锁；Redis 常用 SET NX。本课在 FakeRedis 上实现，避免真实集群。
生产还需考虑锁续期、Redlock 争议等（16_ 阶段深挖）。

二、写法对比
1. 直观版：进程内 lock 关键字，无法跨机器。
2. 进阶版：Redis SET NX + token，释放时校验 token。

三、真实用途
- 定时任务防重、库存扣减串行化、幂等键占位。
资料: https://redis.io/docs/latest/develop/clients/dotnet/
      https://github.com/StackExchange/StackExchange.Redis
*/

sealed class FakeRedisForLock
{
    private readonly Dictionary<string, (string Value, DateTime ExpireAt)> _data = new();

    public bool SetNx(string key, string value, TimeSpan ttl)
    {
        if (_data.TryGetValue(key, out var cur) && DateTime.UtcNow <= cur.ExpireAt)
            return false;
        _data[key] = (value, DateTime.UtcNow.Add(ttl));
        return true;
    }

    public bool CompareAndDelete(string key, string expectedToken)
    {
        if (!_data.TryGetValue(key, out var cur)) return false;
        if (cur.Value != expectedToken) return false;
        _data.Remove(key);
        return true;
    }
}

/** 直观版：单机 lock，不能模拟多进程。 */
static class NaiveLocalLock
{
    private static readonly object Gate = new();
    public static void RunCriticalSection(Action work)
    {
        lock (Gate) work();
    }
}

/** 进阶版：分布式锁 helper。 */
sealed class RedisLock
{
    private readonly FakeRedisForLock _redis;
    private readonly string _key;
    private readonly string _token;
    private bool _held;

    public RedisLock(FakeRedisForLock redis, string key)
    {
        _redis = redis;
        _key = key;
        _token = Guid.NewGuid().ToString("N");
    }

    public bool TryAcquire(TimeSpan ttl) => _held = _redis.SetNx(_key, _token, ttl);

    public void Release()
    {
        if (_held)
            _redis.CompareAndDelete(_key, _token);
        _held = false;
    }
}

static class Program
{
    static void RunTests()
    {
        NaiveLocalLock.RunCriticalSection(() => { /* ok */ });

        var redis = new FakeRedisForLock();
        var lock1 = new RedisLock(redis, "job:daily-report");
        var lock2 = new RedisLock(redis, "job:daily-report");

        if (!lock1.TryAcquire(TimeSpan.FromSeconds(30))) throw new Exception("首次应拿到锁");
        if (lock2.TryAcquire(TimeSpan.FromSeconds(30))) throw new Exception("并发抢锁应失败");

        lock1.Release();
        if (!lock2.TryAcquire(TimeSpan.FromSeconds(30))) throw new Exception("释放后另一方应成功");
        lock2.Release();

        Console.WriteLine("✅ DistributedLockIntro 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Distributed Lock ************\n");
        Console.WriteLine("⚠ 教学级 SET NX；生产请用成熟库并处理续期与故障。");

        var redis = new FakeRedisForLock();
        var lk = new RedisLock(redis, "demo-lock");
        Console.WriteLine(lk.TryAcquire(TimeSpan.FromSeconds(5)) ? "acquired" : "busy");
        lk.Release();
        RunTests();
    }

    static void Main() => Bootstrap();
}