# `11_testing` | 测试

- **目标**: 表驱动单测、Mock 替身、集成测试心智（正式项目用 xUnit + `dotnet test`）
- **前置**: `05_dotnet_tooling`（可穿插于 Web 阶段）
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 11_testing`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test
- https://xunit.net/
- https://github.com/moq/moq
- https://fluentassertions.com/

> **说明**: 本阶段课内用自研 `MiniAssert` / 手写 Mock，**不引用** xUnit NuGet，以便 `dotnet run` 单文件即学。上线项目请执行 `dotnet new xunit` 并参照 https://xunit.net/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_xunit` | `L00_XunitTableDriven.cs` | 表驱动 + 迷你断言 | ✅ |
| `01_mock` | `L00_MoqMental.cs` | 手写 Mock / Verify | ✅ |
| `02_integration` | `L00_IntegrationTestMental.cs` | 内存宿主集成链路 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| dotnet test 官方 | https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test |
| xUnit | https://xunit.net/ |
| Moq | https://github.com/moq/moq |
| FluentAssertions | https://fluentassertions.com/ |

**本目录总课数：3（已完成 3）**