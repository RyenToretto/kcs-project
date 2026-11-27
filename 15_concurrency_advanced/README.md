# `15_concurrency_advanced` | 并发深挖

- **目标**: 并行、Channel、同步原语、并发陷阱
- **前置**: 04_async_await、14_production_dotnet
- **环境**: `./scripts/verify_modules.sh 15_concurrency_advanced`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
- https://www.manning.com/books/csharp-concurrency

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_parallel` | `L00_ParallelAndPlinq.cs` | Parallel / PLINQ | ✅ |
| `01_channel` | `L00_SystemThreadingChannels.cs` | Channel 背压 | ✅ |
| `02_sync` | `L00_Synchronization.cs` | lock / RWLock / SemaphoreSlim | ✅ |
| `03_traps` | `L00_ConcurrencyTraps.cs` | async 陷阱 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| 异步编程 | https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ |
| Concurrency in .NET（书） | https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X |
| C# Concurrency（Manning） | https://www.manning.com/books/csharp-concurrency |

**本目录总课数：4（已完成 4）**