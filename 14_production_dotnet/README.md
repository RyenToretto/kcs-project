# `14_production_dotnet` | 生产级 .NET

- **目标**: 配置、DI、弹性、超时、错误模型
- **前置**: 13_project_capstone
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 14_production_dotnet`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
- https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
- https://learn.microsoft.com/en-us/dotnet/core/resilience/
- https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_config` | `L00_Configuration.cs` | 配置分层与覆盖 | ✅ |
| `01_di` | `L00_DiLifetimes.cs` | DI 生命周期与 Captive Dependency | ✅ |
| `02_resilience` | `L00_Resilience.cs` | 重试、退避、熔断 | ✅ |
| `03_timeout` | `L00_Timeouts.cs` | CancellationToken 超时链 | ✅ |
| `04_errors` | `L00_ProblemDetails.cs` | RFC 7807 错误体 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| 依赖注入 | https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection |
| 配置系统 | https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration |
| 弹性策略 | https://learn.microsoft.com/en-us/dotnet/core/resilience/ |
| HttpClient 工厂 | https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory |

**本目录总课数：5（已完成 5）**