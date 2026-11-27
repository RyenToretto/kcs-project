using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: NuGet 心智 —— 包版本、依赖、锁定
[难度]: Easy
[前置]: 05_dotnet_tooling/01_csproj

一、主题说明
NuGet 是 .NET 包仓库；项目通过 PackageReference 引用。
还原（restore）下载到本地缓存；发布时注意传递依赖与版本冲突。

二、写法对比
1. 直观版：最新版一把梭（可能破坏构建）。
2. 进阶版：固定主版本、读 release notes、在 csproj 或 Central Package Management 统一版本。

三、真实用途
- 引入 JSON、日志、测试框架。
- CI：`dotnet restore` + 私有源（本课离线用虚构元数据）。
资料: https://www.nuget.org/
      https://learn.microsoft.com/en-us/dotnet/core/tools/
      https://dotnet.microsoft.com/en-us/download
*/
static class Program
{
    record PackageMeta(string Id, string Version, string[] Dependencies);

    static readonly PackageMeta[] FakeCatalog =
    {
        new("Contoso.Util", "1.0.0", Array.Empty<string>()),
        new("Contoso.Util", "1.1.0", new[] { "Contoso.Abstractions:1.0.0" }),
        new("Contoso.Abstractions", "1.0.0", Array.Empty<string>()),
    };

    /** 直观版：遍历找精确 id+version。 */
    static PackageMeta? FindNaive(string id, string version)
    {
        foreach (var p in FakeCatalog)
        {
            if (p.Id == id && p.Version == version)
            {
                return p;
            }
        }
        return null;
    }

    /** 进阶版：同 id 取最新（简化语义版本比较）。 */
    static PackageMeta? FindLatestBetter(string id)
    {
        return FakeCatalog
            .Where(p => p.Id == id)
            .OrderByDescending(p => p.Version, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    /** 展开一层传递依赖（教学用）。 */
    static List<string> ResolveDirectDeps(PackageMeta pkg)
    {
        return pkg.Dependencies.ToList();
    }

    static void RunTests()
    {
        var exact = FindNaive("Contoso.Util", "1.0.0");
        if (exact is null || exact.Version != "1.0.0") throw new Exception("精确查找");

        var latest = FindLatestBetter("Contoso.Util");
        if (latest is null || latest.Version != "1.1.0") throw new Exception("最新应为 1.1.0");

        var deps = ResolveDirectDeps(latest);
        if (deps.Count != 1 || deps[0] != "Contoso.Abstractions:1.0.0") throw new Exception("传递依赖");

        Console.WriteLine("✅ NugetMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 NugetMental ************\n");
        var latest = FindLatestBetter("Contoso.Util");
        Console.WriteLine($"包 {latest?.Id} 最新虚构版本 {latest?.Version}");
        if (latest is not null)
        {
            Console.WriteLine($"依赖: {string.Join(", ", ResolveDirectDeps(latest))}");
        }
        RunTests();
    }

    static void Main() => Bootstrap();
}