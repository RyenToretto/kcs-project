# `07_ef_core` | Entity Framework Core

- **目标**: DbContext / ChangeTracker、LINQ 查询延迟执行、事务 Commit/Rollback、迁移步骤心智。
- **前置**: `02_modern_csharp/00_linq`、`06_aspnet_core/01_di`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 07_ef_core`
- **重要**: 本阶段为**内存假 DbContext**，不连接 SQL Server / PostgreSQL，不运行 `dotnet ef`；对照 EF Core 官方文档映射概念。
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/ef/core/
- https://github.com/dotnet/efcore
- https://learn.microsoft.com/en-us/shows/entity-framework-core-101/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_` | [00_dbcontext/L00_DbContextMental.cs](./00_dbcontext/L00_DbContextMental.cs) | Unit of Work + ChangeTracker | ✅ |
| `01_` | [01_query/L00_LinqToEntities.cs](./01_query/L00_LinqToEntities.cs) | IQueryable vs IEnumerable | ✅ |
| `02_` | [02_tx/L00_Transactions.cs](./02_tx/L00_Transactions.cs) | 事务 Commit / Rollback | ✅ |
| `03_` | [03_migration/L00_MigrationsMental.cs](./03_migration/L00_MigrationsMental.cs) | Migration Up/Down 步骤 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| EF Core 文档 | https://learn.microsoft.com/en-us/ef/core/ |
| EF Core 源码 | https://github.com/dotnet/efcore |
| EF Core 101 视频 | https://learn.microsoft.com/en-us/shows/entity-framework-core-101/ |

**本目录总课数：4（已完成 4，待生成 0）**