# `09_messaging` | 消息与异步解耦

- **目标**: 异步解耦、至少一次投递与消费者幂等、Broker 路由心智。
- **前置**: `04_async_await/01_async`
- **环境**: .NET 最新稳定 SDK（建议 ≥ 8）；`./scripts/verify_modules.sh 09_messaging`
- **重要**: 本阶段为**内存 InMemoryMessageBus / MiniBroker**，不连接 RabbitMQ / Kafka；对照 MassTransit 与微服务架构文档学习拓扑与语义。
- **推荐资料（明文 URL，已核验）**:
- https://github.com/MassTransit/MassTransit
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/

## 章节进度

| 编号 | 文件 | 核心点 | 状态 |
| --- | --- | --- | --- |
| `00_` | [00_async/L00_AsyncDecouple.cs](./00_async/L00_AsyncDecouple.cs) | 发布/订阅异步解耦 | ✅ |
| `01_` | [01_idempotent/L00_AtLeastOnceIdempotent.cs](./01_idempotent/L00_AtLeastOnceIdempotent.cs) | 至少一次 + 幂等消费 | ✅ |
| `02_` | [02_broker/L00_BrokerMentalModel.cs](./02_broker/L00_BrokerMentalModel.cs) | Exchange / Queue 路由 | ✅ |

## 本阶段知识点资料包（明文 URL）

| 知识点簇 | 明文 URL |
| --- | --- |
| MassTransit | https://github.com/MassTransit/MassTransit |
| .NET 微服务架构 | https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ |

**本目录总课数：3（已完成 3，待生成 0）**