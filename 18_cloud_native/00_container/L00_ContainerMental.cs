using System;
using System.IO;

/*
[主题]: 容器心智——只读根文件系统、非 root、优雅退出
[难度]: Hard
[前置]: 14_production_dotnet

一、主题说明
镜像分层；进程 PID 1 应处理 SIGTERM；配置用环境变量注入。
陷阱：容器里写本地磁盘当持久化；未设 ASPNETCORE_URLS 绑定 0.0.0.0。

二、写法对比
1. 直观版：假设当前目录永远可写无限空间。
2. 进阶版：检查环境变量、模拟只读 FS 降级到 /tmp。

三、真实用途
- dotnet publish + Dockerfile；K8s requests/limits。
资料: https://learn.microsoft.com/en-us/dotnet/core/docker/introduction
      https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
*/

static class Program
{
    static string PickDataDirNaive() => Directory.GetCurrentDirectory();

    static string PickDataDirBetter()
    {
        var env = Environment.GetEnvironmentVariable("KCS_DATA_DIR");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var tmp = Path.GetTempPath();
        return Path.Combine(tmp, "kcs-app-data");
    }

    static void RunTests()
    {
        _ = PickDataDirNaive();

        var dir = PickDataDirBetter();
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "probe.txt");
        File.WriteAllText(file, "ok");
        if (File.ReadAllText(file) != "ok") throw new Exception("写入探测");

        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:8080";
        if (!urls.Contains("0.0.0.0", StringComparison.Ordinal) && !urls.Contains("[::]", StringComparison.Ordinal))
            Console.WriteLine("提示: 容器内应监听 0.0.0.0");

        var inK8s = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST") is not null;
        if (inK8s) Console.WriteLine("检测到 K8s 环境变量");

        Console.WriteLine("✅ ContainerMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Container Mental ************\n");
        Console.WriteLine($"数据目录: {PickDataDirBetter()}");
        RunTests();
    }

    static void Main() => Bootstrap();
}