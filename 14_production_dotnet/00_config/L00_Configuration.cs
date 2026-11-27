using System.Collections;

/*
[主题]: 配置分层与优先级——环境变量覆盖 JSON
[难度]: Hard
[前置]: 06_aspnet_core/00_minimal、05_dotnet_tooling

一、主题说明
生产配置来自多层：appsettings.json、环境变量、Key Vault、命令行。
后注册/高优先级源覆盖前者。陷阱：在容器里仍用 Development 连接串。

二、写法对比
1. 直观版：硬编码 connectionString。
2. 进阶版：IConfiguration 风格键路径 + 环境变量覆盖。

三、真实用途
- ASP.NET Core 默认 ConfigurationBuilder；12-Factor 要求配置进环境。
资料: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
      https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
*/

sealed class NaiveDbSettings
{
    public string Host { get; } = "prod-db.internal"; // 改环境要重新编译——生产大忌
}

/** 迷你配置源：模拟 JSON + 环境变量覆盖。 */
interface IConfigSource
{
    bool TryGet(string key, out string? value);
}

sealed class DictionaryConfigSource : IConfigSource
{
    private readonly Dictionary<string, string> _data;
    public DictionaryConfigSource(Dictionary<string, string> data) => _data = data;
    public bool TryGet(string key, out string? value) => _data.TryGetValue(key, out value);
}

sealed class MiniConfiguration
{
    private readonly List<IConfigSource> _sources = new();

    public void AddSource(IConfigSource source) => _sources.Add(source);

    public string GetRequired(string key)
    {
        for (var i = _sources.Count - 1; i >= 0; i--)
            if (_sources[i].TryGet(key, out var v) && v is not null)
                return v;
        throw new InvalidOperationException($"缺少配置键: {key}");
    }

    public string? Get(string key)
    {
        for (var i = _sources.Count - 1; i >= 0; i--)
            if (_sources[i].TryGet(key, out var v) && v is not null)
                return v;
        return null;
    }
}

static class Program
{
    static MiniConfiguration BuildBetter()
    {
        var json = new DictionaryConfigSource(new()
        {
            ["ConnectionStrings:Main"] = "Server=localhost;Database=Dev",
            ["Logging:Level"] = "Information"
        });
        var env = new DictionaryConfigSource(new()
        {
            ["ConnectionStrings:Main"] = "Server=prod-sql;Database=Orders"
        });
        var cfg = new MiniConfiguration();
        cfg.AddSource(json);   // 先低优先级
        cfg.AddSource(env);    // 环境变量后加载，覆盖
        return cfg;
    }

    static void RunTests()
    {
        _ = new NaiveDbSettings().Host;

        var cfg = BuildBetter();
        if (cfg.GetRequired("ConnectionStrings:Main") != "Server=prod-sql;Database=Orders")
            throw new Exception("环境变量应覆盖 JSON");

        if (cfg.Get("Logging:Level") != "Information")
            throw new Exception("未被覆盖的键应保留 JSON 值");

        if (cfg.Get("Missing:Key") is not null)
            throw new Exception("不存在键应返回 null");

        Console.WriteLine("✅ Configuration 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Configuration ************\n");
        Console.WriteLine("⚠ 生产用 Microsoft.Extensions.Configuration；本课为离线迷你实现。");
        var cfg = BuildBetter();
        Console.WriteLine($"连接串: {cfg.GetRequired("ConnectionStrings:Main")}");
        RunTests();
    }

    static void Main() => Bootstrap();
}