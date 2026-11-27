# `04_async_await` | 异步编程

- **目标**: 理解 Task、async/await、CancellationToken，避开常见异步陷阱。
- **前置**: `03_collections_generics`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 04_async_await`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X
- https://www.manning.com/books/csharp-concurrency

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_` | [00_task/L00_TaskBasics.cs](./00_task/L00_TaskBasics.cs) | Task、WhenAll | ✅ |
| `01_` | [01_async/L00_AsyncAwait.cs](./01_async/L00_AsyncAwait.cs) | async/await | ✅ |
| `02_` | [02_cancel/L00_CancellationToken.cs](./02_cancel/L00_CancellationToken.cs) | 取消 | ✅ |
| `03_` | [03_pitfalls/L00_AsyncPitfalls.cs](./03_pitfalls/L00_AsyncPitfalls.cs) | 陷阱 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点 | 视频 / 教程 / 书籍（明文 URL） |
| --- | --- |
| 官方异步文档 | https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ |
| 并发书 | https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X ；https://www.manning.com/books/csharp-concurrency |

**本目录总课数：4（已完成 4，待生成 0）**