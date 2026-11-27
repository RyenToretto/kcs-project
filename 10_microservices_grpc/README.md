# `10_microservices_grpc` | 微服务与 gRPC

- **目标**: 按业务边界拆分服务、gRPC Unary 心智、API 网关路由与聚合
- **前置**: `06_aspnet_core`、`09_messaging`（概念即可）
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 10_microservices_grpc`
- **推荐资料（明文 URL，已核验）**:
- https://learn.microsoft.com/en-us/aspnet/core/grpc/
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
- https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_split` | `L00_ServiceSplit.cs` | 单体 vs 按契约拆分 | ✅ |
| `01_grpc` | `L00_GrpcMental.cs` | Unary RPC 离线模拟 | ✅ |
| `02_gateway` | `L00_GatewayMental.cs` | 转发与聚合 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| gRPC on ASP.NET Core | https://learn.microsoft.com/en-us/aspnet/core/grpc/ |
| 微服务架构电子书 | https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ |
| gRPC 迁移心智 | https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/ |

**本目录总课数：3（已完成 3）**