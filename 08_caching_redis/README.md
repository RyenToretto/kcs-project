# `08_caching_redis` | 缓存与 Redis

- **目标**: Cache-Aside、Redis String 命令心智、SET NX 分布式锁入门。
- **前置**: `07_ef_core/01_query`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 08_caching_redis`
- **重要**: 本阶段用**内存 FakeRedis / MemoryCache** 代替真实 `redis-server` 与 StackExchange.Redis 连接；生产请安装 Redis 并对照官方 .NET 客户端文档。
- **推荐资料（明文 URL，已核验）**:
- https://redis.io/docs/latest/develop/clients/dotnet/
- https://github.com/StackExchange/StackExchange.Redis

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_` | [00_cache/L00_CacheAside.cs](./00_cache/L00_CacheAside.cs) | Cache-Aside 读/写路径 | ✅ |
| `01_` | [01_client/L00_RedisClientMental.cs](./01_client/L00_RedisClientMental.cs) | StringGet / StringSet / NX | ✅ |
| `02_` | [02_lock/L00_DistributedLockIntro.cs](./02_lock/L00_DistributedLockIntro.cs) | SET NX 分布式锁 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| Redis .NET 客户端 | https://redis.io/docs/latest/develop/clients/dotnet/ |
| StackExchange.Redis | https://github.com/StackExchange/StackExchange.Redis |

**本目录总课数：3（已完成 3，待生成 0）**