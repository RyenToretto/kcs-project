# kcs-project

C# / .NET **从零到顶尖精通**的个人学习仓库。前提：你是 **C# 小白**（可以有其他语言基础，但 C# 从语法讲起）。

姊妹项目（同级目录）：`kjv-project`（Java）、`kgo-project`（Go）、`kpy_project`（Python）、`kts-project`（TypeScript）。编号骨架可对照，但 **C# 请严格按本仓库阶段顺序**；精通线顺序按 .NET 特性定制。

> 说明：阶段目录用 `NN_slug` 方便 Cursor 全自动生成与进度管理，**不是**死板照搬别的语言课表；课表可按推进增补，但学习顺序不要乱跳。

## 目录（规划 / 已生成）

| 路径 | 阶段 | 内容 | 状态 |
| --- | --- | --- | --- |
| `00_csharp_basics/` | 阶段 1 | C# 语法入门 | ✅ 已生成 |
| `01_oop_csharp/` | 阶段 2 | 面向对象与类型系统 | ✅ 已生成 |
| `02_modern_csharp/` | 阶段 2 | 现代 C#（LINQ/可空/模式/record） | ✅ 已生成 |
| `03_collections_generics/` | 阶段 2 | 集合与泛型 | ✅ 已生成 |
| `04_async_await/` | 阶段 3 | 异步编程 | ✅ 已生成 |
| `05_dotnet_tooling/` | 阶段 3 | .NET SDK / CLI / NuGet | ✅ 已生成 |
| `06_aspnet_core/` | 阶段 4 | ASP.NET Core Web API | ✅ 已生成 |
| `07_ef_core/` | 阶段 4 | Entity Framework Core | ✅ 已生成 |
| `08_caching_redis/` | 阶段 5 | 缓存与 Redis | ✅ 已生成 |
| `09_messaging/` | 阶段 5 | 消息与异步解耦 | ✅ 已生成 |
| `10_microservices_grpc/` | 阶段 6 | 微服务与 gRPC 心智 | ✅ 已生成 |
| `11_testing/` | 穿插 | 单元测试与 Mock | ✅ 已生成 |
| `12_observability/` | 阶段 6 | 可观测性 | ✅ 已生成 |
| `13_project_capstone/` | 毕业 | 综合小项目 | ✅ 已生成 |
| `14_production_dotnet/` | 精通 | 生产级 .NET 工程 | 📝 待生成 |
| `15_concurrency_advanced/` | 精通 | 并发深挖 | 📝 待生成 |
| `16_data_infra/` | 精通 | 数据与中间件深挖 | 📝 待生成 |
| `17_clr_runtime/` | 精通 | CLR / GC / 内存 | 📝 待生成 |
| `18_cloud_native/` | 精通 | 云原生 | 📝 待生成 |
| `19_architecture/` | 精通 | 架构与 SRE | 📝 待生成 |

## 环境

- **.NET SDK 最新稳定版**（建议 ≥ 8；生成课按当前稳定 SDK，不用过时 Framework-only 写法）。
- 安装：https://dotnet.microsoft.com/en-us/download
- 安装文档：https://learn.microsoft.com/en-us/dotnet/core/install/
- 验证种子课：

```bash
cd kcs-project
chmod +x scripts/verify_modules.sh
./scripts/verify_modules.sh 00_csharp_basics
```

## 一、C# 小白到精通：还该学什么（推荐度从高到低）

下表按**推荐度**从高到低；**目录编号按学习顺序**。推荐度 ≠ 学习顺序（例如 ASP.NET Core 很值钱，但仍必须先学语法与 async）。

判定：必须/强相关就业或生产；可以系统学；有必要（投入产出高于「只刷语法题」）。

**外链一律明文完整 URL（已 HTTP 核验），不要只看锚文字。**

| 排名 | 推荐度 | 主题 / 代表资料（明文 URL） | 目录 | 核心内容 | 为什么有必要 |
| --- | --- | --- | --- | --- | --- |
| 1 | ⭐⭐⭐⭐⭐+ | ASP.NET Core：https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core ；Minimal API：https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis ；首个 Web API：https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api | `06_aspnet_core` | REST、DI、中间件、分层 | **C# 后端岗基本盘**；不要零基础直接跳这里。 |
| 2 | ⭐⭐⭐⭐⭐ | 官方入门：https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/ ；学习路径：https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/ ；视频：https://learn.microsoft.com/en-us/shows/csharp-for-beginners/ ；合集：https://learn.microsoft.com/en-us/collections/yz26f8y64n7k07 | `00_csharp_basics` | 类型、控制流、方法 | **地基**。不会这个，后面全是抄作业。 |
| 3 | ⭐⭐⭐⭐⭐ | 异步：https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ ；书：https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X | `04_async_await` | Task、async/await、取消 | .NET 后端几乎处处异步；面试与性能分水岭。 |
| 4 | ⭐⭐⭐⭐⭐ | EF Core：https://learn.microsoft.com/en-us/ef/core/ ；仓库：https://github.com/dotnet/efcore ；视频：https://learn.microsoft.com/en-us/shows/entity-framework-core-101/ | `07_ef_core` | DbContext、迁移、查询 | 业务系统几乎都落库。 |
| 5 | ⭐⭐⭐⭐☆ | OOP：https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop ；编程指南：https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/ | `01_oop_csharp` | 类、接口、封装 | 写出「像 C#」的领域模型。 |
| 6 | ⭐⭐⭐⭐☆ | 现代语法：https://learn.microsoft.com/en-us/dotnet/csharp/linq/ ；新特性：https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/ | `02_modern_csharp` | LINQ、可空、模式、record | 当代 C# 日常写法。 |
| 7 | ⭐⭐⭐⭐☆ | 工具链：https://learn.microsoft.com/en-us/dotnet/core/tools/ ；NuGet：https://www.nuget.org/ ；下载：https://dotnet.microsoft.com/en-us/download | `05_dotnet_tooling` | SDK、CLI、包 | 现代 .NET 项目的日常。 |
| 8 | ⭐⭐⭐⭐☆ | Redis：https://redis.io/docs/latest/develop/clients/dotnet/ ；https://github.com/StackExchange/StackExchange.Redis | `08_caching_redis` | 缓存、过期、锁入门 | 性能与会话/秒杀标配。 |
| 9 | ⭐⭐⭐⭐ | 测试：https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test ；https://xunit.net/ ；https://github.com/moq/moq | `11_testing` | xUnit、Mock | 没有测试谈不上精通与协作。 |
| 10 | ⭐⭐⭐⭐ | 可观测：https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ ；日志：https://learn.microsoft.com/en-us/dotnet/core/extensions/logging ；OTel：https://opentelemetry.io/docs/languages/net/ | `12_observability` | 日志、指标、追踪 | 线上排障与 SRE 基本功。 |
| 11 | ⭐⭐⭐☆ | 微服务：https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ ；gRPC：https://learn.microsoft.com/en-us/aspnet/core/grpc/ | `10_microservices_grpc` | 拆分、RPC | 大厂常见；先懂心智。 |
| 12 | ⭐⭐⭐☆ | 消息：https://github.com/MassTransit/MassTransit ；架构指引：https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ | `09_messaging` | 异步、幂等 | 业务复杂后必遇。 |
| 13 | ⭐⭐⭐☆ | 集合与泛型：https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/ ；Span：https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/ | `03_collections_generics` | 集合选用、泛型 | 日写最高频。 |
| 14 | ⭐⭐⭐ | 综合项目 + 认证：https://aka.ms/csharp-certification ；安全：https://learn.microsoft.com/en-us/aspnet/core/security/ | `13_project_capstone` | 鉴权+CRUD+缓存 | **作品集**。 |
| 15 | ⭐⭐⭐ | 语言参考 / API：https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/ ；https://learn.microsoft.com/en-us/dotnet/api/ ；运行时：https://github.com/dotnet/runtime | 穿插查阅 | 规范、API、源码 | 精通期对照用。 |
| 16 | ⭐⭐ | 视频频道：https://www.youtube.com/dotnet ；入门秀：https://learn.microsoft.com/en-us/shows/csharp-for-beginners/ | 穿插 | 官方演讲与专题 | 辅助理解；主线仍以手敲课为准。 |
| 17 | ⭐⭐ | 过早追新框架皮毛 / 无基础直接上 K8s 运维细节 | 不单独建目录 | — | 机会成本高；主线完成前不要分心。 |

### 各知识点「资料包」速查（阶段内还会再细化）

| 知识点簇 | 明文资料 URL |
| --- | --- |
| 安装与第一课 | https://dotnet.microsoft.com/en-us/download ；https://learn.microsoft.com/en-us/dotnet/core/install/ ；https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/ |
| 官方学习路径 | https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/ ；https://learn.microsoft.com/en-us/training/paths/build-dotnet-applications-csharp/ ；https://learn.microsoft.com/en-us/collections/yz26f8y64n7k07 |
| 视频 | https://learn.microsoft.com/en-us/shows/csharp-for-beginners/ ；https://www.youtube.com/dotnet ；https://learn.microsoft.com/en-us/shows/entity-framework-core-101/ |
| 异步与并发书 | https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ ；https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X ；https://www.manning.com/books/csharp-concurrency |
| Web | https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core ；https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis |
| 数据库 | https://learn.microsoft.com/en-us/ef/core/ ；https://github.com/dotnet/efcore |
| Redis | https://redis.io/docs/latest/develop/clients/dotnet/ ；https://github.com/StackExchange/StackExchange.Redis |
| 测试 | https://xunit.net/ ；https://github.com/moq/moq ；https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test |
| 诊断与监控 | https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ ；https://opentelemetry.io/docs/languages/net/ |
| CLR / 内存书 | https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454 ；https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa ；https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/ |
| 现代 C# 书检索 | https://www.amazon.com/s?k=C%23+12+and+.NET+8+Mark+J+Price |
| 云原生容器 | https://learn.microsoft.com/en-us/dotnet/core/docker/introduction ；https://learn.microsoft.com/en-us/dotnet/core/docker/build-container |
| 设计指南 | https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/ |
| SRE | https://sre.google/books/ |
| 官方认证入口 | https://aka.ms/csharp-certification |

## 二、阶段路线（学习顺序）

| 阶段 | 建议周期 | 目录 | 做什么 | 验收标准 |
| --- | --- | --- | --- | --- |
| 1. 语法 | 2～4 周 | `00_csharp_basics` | 每天 1～2 课，手敲 + verify | 能独立写小程序 |
| 2. OOP 与现代语法 | 2～4 周 | `01_`、`02_`、`03_` | OOP + LINQ + 集合 | 会接口与 LINQ；说清集合选用 |
| 3. 异步与工具 | 2～3 周 | `04_`、`05_`、`11_`（开始） | async + SDK/CLI + 测试 | 会写 async 方法；会 `dotnet test` |
| 4. Web 与存储 | 1～2 月 | `06_`、`07_` | ASP.NET Core + EF Core | 独立完成带校验的 REST + DB |
| 5. 缓存与消息 | 2～4 周 | `08_`、`09_` | 接到上阶段项目 | 会缓存查询；理解异步与幂等 |
| 6. 微服务与观测 | 1～2 月 | `10_`、`12_`、`13_` | gRPC 心智 + 可观测 + 毕业项目 | 作品集可演示 |
| 7. 顶尖精通 | 持续 | `14→15→16→17→18→19`（C#/.NET 专用） | 生产 → 并发深挖 → 中间件 → CLR → 云原生 → 架构 | 各方向能讲清原理并对照真实组件 |

### 为何精通线是 `14→15→16→17→18→19`？

| 顺序 | 目录 | 为何排在这里 |
| --- | --- | --- |
| 1 | `14_production_dotnet` | 先能稳定上线（配置、DI、弹性、超时） |
| 2 | `15_concurrency_advanced` | async 之后深挖并行/Channel/陷阱 |
| 3 | `16_data_infra` | 数据/缓存/MQ 事故高发区 |
| 4 | `17_clr_runtime` | GC/Span：写过大量异步与并发后再学更有体感 |
| 5 | `18_cloud_native` | 容器与部署协同 |
| 6 | `19_architecture` | 架构与 SRE 收官 |

## 三、用 Cursor 全自动生成（rules + skill）

| 类型 | 路径 | 作用 |
| --- | --- | --- |
| Rule | `.cursor/rules/kcs-learning-standards.mdc` | 编号、`L00_`、小白友好、verify、明文链接、顶尖精通终点 |
| Skill | `.cursor/skills/csharp-study-generator/SKILL.md` | 按阶段生成课、模板、分批提交 |
| 课表 | `.cursor/skills/csharp-study-generator/references/roadmap.md` | 各阶段课表明细与精通顺序 |
| 资料包 | `.cursor/skills/csharp-study-generator/references/resources.md` | 已 HTTP 核验的明文 URL |

**用法：** Cursor 打开 **`kcs-project` 根目录**，粘贴下面「全自动 Prompt」。

### 推荐粘贴的「全自动」Prompt（复制整段）

```text
请严格按 csharp-study-generator skill 与 .cursor/rules/kcs-learning-standards.mdc 执行。

我的起点：C# 小白（可能有一点其他语言基础，但请按零基础讲 C#）。
工具链：使用 .NET 最新稳定 SDK（建议 ≥ 8）；生成的代码与讲解按当前稳定版，不要用过时 .NET Framework-only 写法。
目标：按 README「阶段路线」顺序，把尚未生成的目录全部自动生成完，使我按顺序学完后达到可就业的 C#/.NET 后端水平，并继续精通各方向（语法/异步扎实、会 ASP.NET Core + EF Core、会 Redis/消息入门、会测试与可观测、有综合小项目，再完成 14→19）。

生成顺序（跳过已完成的课，补齐缺课）：
00_csharp_basics →
01_oop_csharp →
02_modern_csharp →
03_collections_generics →
04_async_await →
05_dotnet_tooling →
06_aspnet_core →
07_ef_core →
08_caching_redis →
09_messaging →
10_microservices_grpc →
11_testing →
12_observability →
13_project_capstone →
（精通线）
14_production_dotnet →
15_concurrency_advanced →
16_data_infra →
17_clr_runtime →
18_cloud_native →
19_architecture

硬性要求：
1. 一次只做一个 NN_xxx；写完更新该阶段 README 与根 README「已生成」表。
2. 每课：L00_Xxx.cs + 同目录 L00_Xxx.csproj、中文注释、直观版+进阶版（有价值时）、RunTests()+Bootstrap()+Main、可用 ./scripts/verify_modules.sh 跑通。
3. 每阶段 verify 全绿后单独 git commit；全部完成后 git push。
4. 内容必须由浅入深，禁止默认我已会 ASP.NET Core / EF Core / gRPC / K8s。
5. 不要无关重构；不要 force push。
6. README 外链必须明文完整 URL，禁止未验证假链接。

现在从下一个未完成的阶段开始全自动执行，直到全部完成。
```

### 精通线追加 Prompt

```text
请帮我全自动推进 14 → 15 → 16 → 17 → 18 → 19，保证全面，细节完美。
（这是 C#/.NET 专用精通顺序：生产 → 并发深挖 → 数据中间件 → CLR → 云原生 → 架构。）
严格按 csharp-study-generator 与 kcs-learning-standards：每阶段 verify 全绿后单独 commit，全部完成再 push，不要 force push。
```

## 四、和姊妹仓的关系

- `kjv-project`：Java 后端
- `kgo-project`：Go 后端
- `kcs-project`：C# / .NET 后端（本仓库）
- `kpy_project`：Python / AI
- `kts-project`：TypeScript / 前端

不要一上来只抄 ASP.NET 教程；请按本 README 阶段顺序推进。
