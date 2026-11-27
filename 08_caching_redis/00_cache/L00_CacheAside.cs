using System;
using System.Collections.Generic;

/*
[主题]: Cache-Aside（旁路缓存）——先读缓存，未命中再读库并回填
[难度]: Medium
[前置]: 07_ef_core/01_query

一、主题说明
应用负责维护缓存与数据库一致性；更新时通常先写库再删缓存。本课用内存字典模拟缓存层，
不需要 Redis 服务器。

二、写法对比
1. 直观版：每次都查「数据库」字典。
2. 进阶版：Cache-Aside 读路径 + 写路径失效缓存。

三、真实用途
- 热点读、减轻数据库压力；注意穿透、击穿、雪崩（精通阶段 16_ 深挖）。
资料: https://redis.io/docs/latest/develop/clients/dotnet/
      https://github.com/StackExchange/StackExchange.Redis
*/

record Product(int Id, string Name, decimal Price);

interface IProductStore
{
    Product? Get(int id);
    void Save(Product product);
}

/** 直观版：无缓存，每次直读 Store。 */
sealed class NaiveReader
{
    private readonly IProductStore _store;
    public NaiveReader(IProductStore store) => _store = store;
    public Product? Get(int id) => _store.Get(id);
}

interface ICache
{
    bool TryGet(string key, out string? value);
    void Set(string key, string value, TimeSpan ttl);
    void Remove(string key);
}

/** 内存缓存，带简单 TTL。 */
sealed class MemoryCache : ICache
{
    private readonly Dictionary<string, (string Value, DateTime ExpireAt)> _data = new();

    public bool TryGet(string key, out string? value)
    {
        value = null;
        if (!_data.TryGetValue(key, out var entry)) return false;
        if (DateTime.UtcNow > entry.ExpireAt)
        {
            _data.Remove(key);
            return false;
        }
        value = entry.Value;
        return true;
    }

    public void Set(string key, string value, TimeSpan ttl) =>
        _data[key] = (value, DateTime.UtcNow.Add(ttl));

    public void Remove(string key) => _data.Remove(key);
}

sealed class InMemoryProductStore : IProductStore
{
    private readonly Dictionary<int, Product> _db = new();
    public int ReadCount { get; private set; }

    public Product? Get(int id)
    {
        ReadCount++;
        return _db.GetValueOrDefault(id);
    }

    public void Save(Product product) => _db[product.Id] = product;
}

/** 进阶版：Cache-Aside 读/写。 */
sealed class CacheAsideProductService
{
    private readonly IProductStore _store;
    private readonly ICache _cache;

    public CacheAsideProductService(IProductStore store, ICache cache)
    {
        _store = store;
        _cache = cache;
    }

    static string Key(int id) => $"product:{id}";

    public Product? Get(int id)
    {
        if (_cache.TryGet(Key(id), out var cached) && cached is not null)
            return Deserialize(cached);

        var fromDb = _store.Get(id);
        if (fromDb is not null)
            _cache.Set(Key(id), Serialize(fromDb), TimeSpan.FromMinutes(5));
        return fromDb;
    }

    public void Update(Product product)
    {
        _store.Save(product);
        _cache.Remove(Key(product.Id));
    }

    static string Serialize(Product p) => $"{p.Id}|{p.Name}|{p.Price}";
    static Product Deserialize(string raw)
    {
        var parts = raw.Split('|');
        return new Product(int.Parse(parts[0]), parts[1], decimal.Parse(parts[2]));
    }
}

static class Program
{
    static void RunTests()
    {
        var naiveStore = new InMemoryProductStore();
        naiveStore.Save(new Product(99, "Ink", 1m));
        _ = new NaiveReader(naiveStore).Get(99);

        var store = new InMemoryProductStore();
        store.Save(new Product(1, "Pen", 3m));
        var cache = new MemoryCache();
        var svc = new CacheAsideProductService(store, cache);

        var first = svc.Get(1);
        if (first?.Name != "Pen") throw new Exception("首次应从库读");
        if (store.ReadCount != 1) throw new Exception("首次 ReadCount=1");

        var second = svc.Get(1);
        if (second?.Name != "Pen") throw new Exception("缓存命中");
        if (store.ReadCount != 1) throw new Exception("命中后不应再读库");

        svc.Update(new Product(1, "Pen-v2", 4m));
        var afterWrite = svc.Get(1);
        if (afterWrite?.Name != "Pen-v2") throw new Exception("写后应回源新数据");
        if (store.ReadCount != 2) throw new Exception("失效后应再读库一次");

        Console.WriteLine("✅ CacheAside 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Cache-Aside ************\n");
        Console.WriteLine("⚠ MemoryCache 替代 Redis；生产用 StackExchange.Redis 或 IDistributedCache。");

        var svc = new CacheAsideProductService(new InMemoryProductStore(), new MemoryCache());
        svc.Update(new Product(9, "Demo", 1m));
        Console.WriteLine(svc.Get(9)?.Name);
        RunTests();
    }

    static void Main() => Bootstrap();
}