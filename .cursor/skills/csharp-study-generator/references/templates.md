# `kcs-project` 学习文件标准模板

## 1. 单课 `.cs` + `.csproj` 模板

`L00_TopicName.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

`L00_TopicName.cs`：

```csharp
using System;

/*
[主题]: 一句话标题
[难度]: Easy / Medium / Hard
[前置]: 无 / 00_csharp_basics/...

一、主题说明
...

二、写法对比
1. 直观版: ...
2. 进阶版: ...

三、真实用途
- ...
资料: https://...
*/
static class Program
{
    static int AddNaive(int left, int right) => left + right;
    static int AddBetter(int left, int right) => left + right;

    static void RunTests()
    {
        if (AddNaive(1, 2) != 3) throw new Exception("失败");
        Console.WriteLine("✅ Topic 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ Topic ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}
```

## 2. 阶段 `README.md` 模板

必须含：目标/前置/环境、推荐资料明文 URL、章节进度表、本阶段知识点资料包、总课数。

## 3. 命名

- 目录：`00_hello/`、`01_control/`…
- 文件：`L00_HelloAndTypes.cs` + `L00_HelloAndTypes.csproj`
