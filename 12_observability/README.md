# `12_observability` | 可观测性

- **目标**: 结构化日志、Counter/Histogram 指标、分布式追踪 Span 树
- **前置**: `06_aspnet_core`、`11_testing`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 12_observability`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/core/diagnostics/
- https://learn.microsoft.com/en-us/dotnet/core/extensions/logging
- https://opentelemetry.io/docs/languages/net/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_logging` | `L00_LoggingMental.cs` | 级别与 Scope | ✅ |
| `01_metrics` | `L00_MetricsMental.cs` | Counter / Histogram | ✅ |
| `02_trace` | `L00_TraceOtelMental.cs` | TraceId / 父子 Span | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| .NET 诊断 | https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ |
| 日志扩展 | https://learn.microsoft.com/en-us/dotnet/core/extensions/logging |
| OpenTelemetry .NET | https://opentelemetry.io/docs/languages/net/ |

**本目录总课数：3（已完成 3）**