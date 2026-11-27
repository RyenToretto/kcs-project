# `16_data_infra` | 数据与中间件深挖

- **目标**: 索引选择性、事务隔离、Redis 坑、MQ 投递语义
- **前置**: 07_ef_core、08_caching_redis、09_messaging
- **环境**: `./scripts/verify_modules.sh 16_data_infra`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/ef/core/
- https://redis.io/docs/latest/develop/clients/dotnet/
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_index` | `L00_SqlIndexExplain.cs` | 索引选择性 | ✅ |
| `01_isolation` | `L00_IsolationLevels.cs` | 隔离级别 | ✅ |
| `02_redis` | `L00_RedisPitfalls.cs` | 缓存三兄弟 | ✅ |
| `03_mq` | `L00_MqDeliverySemantics.cs` | 至少一次 + 幂等 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| EF Core | https://learn.microsoft.com/en-us/ef/core/ |
| Redis .NET 客户端 | https://redis.io/docs/latest/develop/clients/dotnet/ |
| 微服务架构 | https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ |

**本目录总课数：4（已完成 4）**