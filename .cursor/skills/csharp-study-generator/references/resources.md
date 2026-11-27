# 已核验资料包（明文 URL）

> 规则：写入 README / 课内注释的外链必须是本表或同次 HTTP 核验通过的完整 URL；禁止 Markdown 仅锚文字、禁止未核验假链接。  
> 核验方式：`curl -sL -o /dev/null -w "%{http_code}" -A "Mozilla/5.0" <url>`，目标优先 **200**。  
> 更新日：2026-03-27；建议 .NET SDK ≥ 8。

## 总览

| 用途 | 明文 URL |
| --- | --- |
| SDK 下载 | https://dotnet.microsoft.com/en-us/download |
| 安装文档 | https://learn.microsoft.com/en-us/dotnet/core/install/ |
| C# 导览 | https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/ |
| C# 文档中心 | https://learn.microsoft.com/en-us/dotnet/csharp/ |
| 语言参考 | https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/ |
| API 浏览器 | https://learn.microsoft.com/en-us/dotnet/api/ |
| 入门路径 | https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/ |
| 构建应用路径 | https://learn.microsoft.com/en-us/training/paths/build-dotnet-applications-csharp/ |
| Learn C# 合集 | https://learn.microsoft.com/en-us/collections/yz26f8y64n7k07 |
| 入门视频 | https://learn.microsoft.com/en-us/shows/csharp-for-beginners/ |
| .NET YouTube | https://www.youtube.com/dotnet |
| 认证入口 | https://aka.ms/csharp-certification |
| NuGet | https://www.nuget.org/ |
| runtime 源码 | https://github.com/dotnet/runtime |
| CLR via C#（书） | https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454 |
| Concurrency in .NET（书） | https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X |
| C# Concurrency（Manning） | https://www.manning.com/books/csharp-concurrency |
| C#/.NET 书检索 | https://www.amazon.com/s?k=C%23+12+and+.NET+8+Mark+J+Price |
| 内存管理书检索 | https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa |
| SRE 书籍 | https://sre.google/books/ |

## 按阶段

### `00_csharp_basics`
https://learn.microsoft.com/en-us/dotnet/core/install/ ；https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/ ；https://learn.microsoft.com/en-us/training/paths/csharp-first-steps/ ；https://learn.microsoft.com/en-us/shows/csharp-for-beginners/

### `01_oop_csharp`
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop ；https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/

### `02_modern_csharp`
https://learn.microsoft.com/en-us/dotnet/csharp/linq/ ；https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/ ；https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/

### `03_collections_generics`
https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/ ；https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/

### `04_async_await`
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ ；https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X ；https://www.manning.com/books/csharp-concurrency

### `05_dotnet_tooling`
https://learn.microsoft.com/en-us/dotnet/core/tools/ ；https://www.nuget.org/ ；https://dotnet.microsoft.com/en-us/download

### `06_aspnet_core`
https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core ；https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis ；https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api ；https://github.com/dotnet/aspnetcore

### `07_ef_core`
https://learn.microsoft.com/en-us/ef/core/ ；https://github.com/dotnet/efcore ；https://learn.microsoft.com/en-us/shows/entity-framework-core-101/

### `08_caching_redis`
https://redis.io/docs/latest/develop/clients/dotnet/ ；https://github.com/StackExchange/StackExchange.Redis

### `09_messaging`
https://github.com/MassTransit/MassTransit ；https://learn.microsoft.com/en-us/dotnet/architecture/microservices/

### `10_microservices_grpc`
https://learn.microsoft.com/en-us/aspnet/core/grpc/ ；https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ ；https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/

### `11_testing`
https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test ；https://xunit.net/ ；https://github.com/moq/moq ；https://nunit.org/ ；https://fluentassertions.com/

### `12_observability`
https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ ；https://learn.microsoft.com/en-us/dotnet/core/extensions/logging ；https://opentelemetry.io/docs/languages/net/

### `13_project_capstone`
https://learn.microsoft.com/en-us/aspnet/core/security/ ；https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api ；https://aka.ms/csharp-certification

### `14_production_dotnet`
https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection ；https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration ；https://learn.microsoft.com/en-us/dotnet/core/resilience/ ；https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory

### `15_concurrency_advanced`
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ ；https://www.amazon.com/Concurrency-NET-CookBook-Asynchronous-Multithreaded/dp/149205450X ；https://www.manning.com/books/csharp-concurrency

### `16_data_infra`
https://learn.microsoft.com/en-us/ef/core/ ；https://redis.io/docs/latest/develop/clients/dotnet/ ；https://learn.microsoft.com/en-us/dotnet/architecture/microservices/

### `17_clr_runtime`
https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/ ；https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/ ；https://learn.microsoft.com/en-us/dotnet/core/diagnostics/ ；https://www.amazon.com/CLR-via-C-Developer-Reference/dp/0735667454 ；https://www.amazon.com/s?k=Pro+.NET+Memory+Management+Konrad+Kokosa

### `18_cloud_native`
https://learn.microsoft.com/en-us/dotnet/core/docker/introduction ；https://learn.microsoft.com/en-us/dotnet/core/docker/build-container ；https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration

### `19_architecture`
https://learn.microsoft.com/en-us/dotnet/architecture/microservices/ ；https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/ ；https://sre.google/books/
