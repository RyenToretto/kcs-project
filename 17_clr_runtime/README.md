# `17_clr_runtime` | CLR / GC / 内存

- **目标**: GC、Span、诊断、性能习惯
- **前置**: 15_concurrency_advanced
- **环境**: `./scripts/verify_modules.sh 17_clr_runtime`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/
- https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/
- https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
- https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454
- https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_gc` | `L00_GarbageCollector.cs` | 世代与分配 | ✅ |
| `01_span` | `L00_SpanMemory.cs` | Span / ArrayPool | ✅ |
| `02_diag` | `L00_Diagnostics.cs` | Activity 追踪 | ✅ |
| `03_perf` | `L00_PerfHabits.cs` | 先度量后优化 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| GC | https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/ |
| Span 与 Memory | https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/ |
| 诊断 | https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ |
| CLR via C# | https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454 |
| 内存管理书检索 | https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa |

**本目录总课数：4（已完成 4）**