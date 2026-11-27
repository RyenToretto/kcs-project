using System;
using System.Linq;
using System.Text.RegularExpressions;

/*
[主题]: .NET SDK 与 CLI 心智 —— 运行时、SDK、常用命令
[难度]: Easy
[前置]: 00_csharp_basics/00_hello

一、主题说明
- **Runtime**：运行已编译程序。
- **SDK**：含编译器、dotnet CLI、模板；开发机装 SDK 即可。
- 本课用「虚构命令输出」做离线解析练习（不依赖真实 shell）。

二、写法对比
1. 直观版：死记硬背命令字符串。
2. 进阶版：理解每个命令产出什么（Restore/Build/Run/Test）。

三、真实用途
- 新人 onboard：`dotnet --version`、`dotnet new`、`dotnet run`。
- CI：`dotnet restore`、`dotnet test --no-restore`。
资料: https://learn.microsoft.com/en-us/dotnet/core/tools/
      https://dotnet.microsoft.com/en-us/download
      https://www.nuget.org/
*/
static class Program
{
    /** 虚构 `dotnet --list-sdks` 输出（教学用）。 */
    const string FakeSdkList = """
        8.0.403 [/usr/local/share/dotnet/sdk]
        9.0.100 [/usr/local/share/dotnet/sdk]
        """;

    /** 直观版：手动查找是否包含版本前缀。 */
    static bool HasSdkNaive(string listOutput, string majorMinor)
    {
        foreach (var line in listOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith(majorMinor, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /** 进阶版：正则抓主版本号。 */
    static string[] ParseSdkVersions(string listOutput)
    {
        var rx = new Regex(@"^(\d+\.\d+\.\d+)", RegexOptions.Multiline);
        return rx.Matches(listOutput).Select(m => m.Groups[1].Value).ToArray();
    }

    /** 虚构 `dotnet build` 最后一行。 */
    const string FakeBuildSummary = "Build succeeded. 0 Warning(s) 0 Error(s)";

    static bool BuildSucceeded(string summary) =>
        summary.Contains("Build succeeded", StringComparison.OrdinalIgnoreCase)
        && !summary.Contains("Error(s) 1", StringComparison.Ordinal);

    static void RunTests()
    {
        if (!HasSdkNaive(FakeSdkList, "8.0.")) throw new Exception("应找到 8.0 SDK");
        if (HasSdkNaive(FakeSdkList, "7.0.")) throw new Exception("不应有 7.0");

        var versions = ParseSdkVersions(FakeSdkList);
        if (versions.Length != 2 || versions[0] != "8.0.403") throw new Exception("解析 SDK 版本");

        if (!BuildSucceeded(FakeBuildSummary)) throw new Exception("构建应成功");

        Console.WriteLine("✅ SdkAndCli 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 SdkAndCli ************\n");
        Console.WriteLine("SDK 列表（虚构）:");
        Console.WriteLine(FakeSdkList);
        Console.WriteLine($"已安装版本: {string.Join(", ", ParseSdkVersions(FakeSdkList))}");
        RunTests();
    }

    static void Main() => Bootstrap();
}