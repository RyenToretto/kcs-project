# `06_aspnet_core` | ASP.NET Core Web API

- **目标**: Minimal API / Controller、依赖注入（DI）、中间件管道、Controller→Service→Repository 分层心智。
- **前置**: `04_async_await`、`05_dotnet_tooling`、`01_oop_csharp/02_interface`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 06_aspnet_core`
- **重要**: 本阶段全部为**离线纯 C# 心智模型**，不启动 Kestrel / 不引用 `Microsoft.AspNetCore.*` 包；对照真实 ASP.NET Core 文档学习即可。
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api
- https://github.com/dotnet/aspnetcore

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_` | [00_minimal/L00_MinimalApiMental.cs](./00_minimal/L00_MinimalApiMental.cs) | 路由表 + 处理器字典 | ✅ |
| `01_` | [01_di/L00_DependencyInjection.cs](./01_di/L00_DependencyInjection.cs) | DI 容器与 Singleton/Scoped/Transient | ✅ |
| `02_` | [02_middleware/L00_MiddlewarePipeline.cs](./02_middleware/L00_MiddlewarePipeline.cs) | 洋葱中间件管道 | ✅ |
| `03_` | [03_layers/L00_ControllerServiceRepo.cs](./03_layers/L00_ControllerServiceRepo.cs) | Controller → Service → Repository | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| ASP.NET Core 总览 | https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core |
| Minimal API | https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis |
| 首个 Web API 教程 | https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api |
| 源码仓库 | https://github.com/dotnet/aspnetcore |

**本目录总课数：4（已完成 4，待生成 0）**