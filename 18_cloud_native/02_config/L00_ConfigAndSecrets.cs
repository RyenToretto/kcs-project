using System;
using System.Collections.Generic;

/*
[主题]: 云配置与密钥——绝不出现在镜像里
[难度]: Hard
[前置]: 14_production_dotnet/00_config

一、主题说明
密钥进 Key Vault / K8s Secret / 托管身份，运行时注入。
陷阱：把 appsettings.Production.json 含密码提交 Git。

二、写法对比
1. 直观版：常量 API_KEY。
2. 进阶版：环境变量 + 缺失即启动失败（fail fast）。

三、真实用途
- Azure Key Vault configuration provider；Docker secret mount。
资料: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
      https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
*/

sealed class NaiveSecrets
{
    public const string ApiKey = "hardcoded-key"; // 泄漏风险
}

sealed class CloudConfig
{
    private readonly Dictionary<string, string> _env;

    public CloudConfig(Dictionary<string, string>? env = null)
    {
        _env = env ?? new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry e in Environment.GetEnvironmentVariables())
        {
            if (e.Key is string k && e.Value is string v)
                _env[k] = v;
        }
    }

    public string Require(string key)
    {
        if (_env.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v))
            return v;
        throw new InvalidOperationException($"缺少密钥配置: {key}");
    }

    public bool IsPlaceholder(string value) =>
        value is "hardcoded-key" or "changeme" or "TODO";
}

static class Program
{
    static void RunTests()
    {
        _ = NaiveSecrets.ApiKey;

        var cfg = new CloudConfig(new Dictionary<string, string>
        {
            ["OPENAI_API_KEY"] = "from-secret-store"
        });

        if (cfg.Require("OPENAI_API_KEY") != "from-secret-store")
            throw new Exception("应读到密钥");

        try
        {
            cfg.Require("MISSING_KEY");
            throw new Exception("应 fail fast");
        }
        catch (InvalidOperationException) { }

        if (!cfg.IsPlaceholder(NaiveSecrets.ApiKey))
            throw new Exception("应识别占位/硬编码");

        Console.WriteLine("✅ ConfigAndSecrets 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Config & Secrets ************\n");
        Console.WriteLine("⚠ 生产用 User Secrets 仅本地；集群用 Secret + RBAC。");
        RunTests();
    }

    static void Main() => Bootstrap();
}