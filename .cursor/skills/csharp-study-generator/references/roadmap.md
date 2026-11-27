# kcs-project 阶段路线

## 学习者画像

- **起点**：C# 小白（可有其他语言基础，但按零基础讲 C#）。
- **目标**：先达到**可就业 C#/.NET 后端**，再沿 `14→15→16→17→18→19` 冲刺各方向**顶尖精通**。
- **工具链**：.NET 最新稳定 SDK（建议 ≥ 8）。
- **资料**：每知识点资料包见 [resources.md](resources.md)。

## 阶段编号（学习顺序）

| 编号 | 目录 | 内容 |
| --- | --- | --- |
| `00_` | `00_csharp_basics` | 安装、语法、类型、控制流、方法、字符串 |
| `01_` | `01_oop_csharp` | 类、继承、接口、属性、封装 |
| `02_` | `02_modern_csharp` | LINQ、可空、模式匹配、record、新语法 |
| `03_` | `03_collections_generics` | 集合、泛型、Span 直觉 |
| `04_` | `04_async_await` | Task、async/await、CancellationToken |
| `05_` | `05_dotnet_tooling` | SDK、CLI、csproj、NuGet |
| `06_` | `06_aspnet_core` | Minimal API / Controller、DI、中间件 |
| `07_` | `07_ef_core` | DbContext、迁移、查询、事务心智 |
| `08_` | `08_caching_redis` | 缓存、Redis 客户端心智、锁入门 |
| `09_` | `09_messaging` | 异步解耦、至少一次与幂等 |
| `10_` | `10_microservices_grpc` | 拆分、gRPC、网关心智 |
| `11_` | `11_testing` | xUnit、Moq、集成测试心智 |
| `12_` | `12_observability` | 日志、指标、追踪、诊断 |
| `13_` | `13_project_capstone` | 鉴权+CRUD+缓存综合 |
| `14_` | `14_production_dotnet` | 配置、DI、弹性、超时、错误模型 |
| `15_` | `15_concurrency_advanced` | 并行、Channel、同步、陷阱 |
| `16_` | `16_data_infra` | 索引/隔离、缓存坑、MQ 语义、池 |
| `17_` | `17_clr_runtime` | GC、Span、诊断、性能 |
| `18_` | `18_cloud_native` | 容器、健康检查、配置密钥 |
| `19_` | `19_architecture` | DDD、事件、CQRS、ADR、SRE |

精通推荐学习顺序：`14 → 15 → 16 → 17 → 18 → 19`。

## 课表明细（生成时按此落盘；可增不可随意删核心课）

### `00_csharp_basics`（4+）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_hello` | `L00_HelloAndTypes.cs` | Main、基本类型、自测结构 |
| `01_control` | `L00_ControlFlow.cs` | if / switch / for / foreach |
| `02_methods` | `L00_MethodsAndParams.cs` | 方法、参数、返回值 |
| `03_strings` | `L00_StringsAndArrays.cs` | 字符串、数组 |

### `01_oop_csharp`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_class` | `L00_ClassAndObject.cs` | 类与对象、属性 |
| `01_inheritance` | `L00_InheritancePolymorphism.cs` | 继承与多态 |
| `02_interface` | `L00_Interfaces.cs` | 接口 |
| `03_encapsulation` | `L00_Encapsulation.cs` | 封装与访问修饰符 |

### `02_modern_csharp`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_linq` | `L00_LinqBasics.cs` | LINQ |
| `01_nullable` | `L00_NullableReference.cs` | 可空引用 |
| `02_pattern` | `L00_PatternMatching.cs` | 模式匹配 |
| `03_records` | `L00_RecordsAndWith.cs` | record / with |

### `03_collections_generics`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_list_dict` | `L00_ListAndDictionary.cs` | List / Dictionary |
| `01_generics` | `L00_Generics.cs` | 泛型 |
| `02_span` | `L00_SpanIntro.cs` | Span 直觉 |

### `04_async_await`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_task` | `L00_TaskBasics.cs` | Task |
| `01_async` | `L00_AsyncAwait.cs` | async/await |
| `02_cancel` | `L00_CancellationToken.cs` | 取消 |
| `03_pitfalls` | `L00_AsyncPitfalls.cs` | 常见异步陷阱 |

### `05_dotnet_tooling`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_sdk` | `L00_SdkAndCli.cs` | SDK / CLI 心智 |
| `01_csproj` | `L00_CsprojMental.cs` | 项目文件 |
| `02_nuget` | `L00_NugetMental.cs` | NuGet |

### `06_aspnet_core`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_minimal` | `L00_MinimalApiMental.cs` | Minimal API 心智 |
| `01_di` | `L00_DependencyInjection.cs` | DI |
| `02_middleware` | `L00_MiddlewarePipeline.cs` | 中间件 |
| `03_layers` | `L00_ControllerServiceRepo.cs` | 分层 |

### `07_ef_core`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_dbcontext` | `L00_DbContextMental.cs` | DbContext |
| `01_query` | `L00_LinqToEntities.cs` | 查询 |
| `02_tx` | `L00_Transactions.cs` | 事务 |
| `03_migration` | `L00_MigrationsMental.cs` | 迁移心智 |

### `08_caching_redis`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_cache` | `L00_CacheAside.cs` | Cache-Aside |
| `01_client` | `L00_RedisClientMental.cs` | 客户端心智 |
| `02_lock` | `L00_DistributedLockIntro.cs` | 锁入门 |

### `09_messaging`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_async` | `L00_AsyncDecouple.cs` | 异步解耦 |
| `01_idempotent` | `L00_AtLeastOnceIdempotent.cs` | 幂等 |
| `02_broker` | `L00_BrokerMentalModel.cs` | Broker 心智 |

### `10_microservices_grpc`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_split` | `L00_ServiceSplit.cs` | 拆分 |
| `01_grpc` | `L00_GrpcMental.cs` | gRPC |
| `02_gateway` | `L00_GatewayMental.cs` | 网关 |

### `11_testing`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_xunit` | `L00_XunitTableDriven.cs` | 表驱动 |
| `01_mock` | `L00_MoqMental.cs` | Mock |
| `02_integration` | `L00_IntegrationTestMental.cs` | 集成测试心智 |

### `12_observability`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_logging` | `L00_LoggingMental.cs` | 日志 |
| `01_metrics` | `L00_MetricsMental.cs` | 指标 |
| `02_trace` | `L00_TraceOtelMental.cs` | 追踪 |

### `13_project_capstone`（1+）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_app` | `L00_MiniApiCapstone.cs` | 鉴权+CRUD+缓存 |

### `14_production_dotnet`（5）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_config` | `L00_Configuration.cs` | 配置 |
| `01_di` | `L00_DiLifetimes.cs` | 生命周期 |
| `02_resilience` | `L00_Resilience.cs` | 弹性 |
| `03_timeout` | `L00_Timeouts.cs` | 超时 |
| `04_errors` | `L00_ProblemDetails.cs` | 错误模型 |

### `15_concurrency_advanced`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_parallel` | `L00_ParallelAndPlinq.cs` | 并行 |
| `01_channel` | `L00_SystemThreadingChannels.cs` | Channel |
| `02_sync` | `L00_Synchronization.cs` | 同步原语 |
| `03_traps` | `L00_ConcurrencyTraps.cs` | 陷阱 |

### `16_data_infra`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_index` | `L00_SqlIndexExplain.cs` | 索引 |
| `01_isolation` | `L00_IsolationLevels.cs` | 隔离 |
| `02_redis` | `L00_RedisPitfalls.cs` | 缓存坑 |
| `03_mq` | `L00_MqDeliverySemantics.cs` | 投递语义 |

### `17_clr_runtime`（4）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_gc` | `L00_GarbageCollector.cs` | GC |
| `01_span` | `L00_SpanMemory.cs` | Span/Memory |
| `02_diag` | `L00_Diagnostics.cs` | 诊断 |
| `03_perf` | `L00_PerfHabits.cs` | 性能习惯 |

### `18_cloud_native`（3）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_container` | `L00_ContainerMental.cs` | 容器 |
| `01_health` | `L00_HealthChecks.cs` | 健康检查 |
| `02_config` | `L00_ConfigAndSecrets.cs` | 配置密钥 |

### `19_architecture`（5）
| 目录 | 文件 | 核心点 |
| --- | --- | --- |
| `00_ddd` | `L00_DddTactical.cs` | DDD |
| `01_events` | `L00_DomainEvents.cs` | 事件 |
| `02_cqrs` | `L00_CqrsSplit.cs` | CQRS |
| `03_adr` | `L00_AdrChecklist.cs` | ADR |
| `04_sre` | `L00_SreChecklist.cs` | SRE |

## 全自动生成批次

1. 一次只做一个 `NN_xxx`。
2. 写课 → 更新阶段 README → 更新根 README 状态 → verify → 单独 commit。
3. 用户要求再 push。
4. 生成顺序：`00`→`13`，再 `14→15→16→17→18→19`。
