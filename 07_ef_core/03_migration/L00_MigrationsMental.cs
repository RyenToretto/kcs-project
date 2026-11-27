using System;
using System.Collections.Generic;
using System.Linq;

/*
[主题]: EF Core 迁移（Migration）心智——模型变更如何落到数据库
[难度]: Medium
[前置]: 07_ef_core/00_dbcontext

一、主题说明
改实体类 → 生成 Migration 脚本 → 应用到数据库。本课用「版本化 DDL 步骤列表」离线模拟，
不运行 dotnet ef 命令连真实库。

二、写法对比
1. 直观版：手动改表结构字符串。
2. 进阶版：按 MigrationId 顺序应用 Up/Down 步骤。

三、真实用途
- 团队协作、CI 部署数据库 schema；生产前要审 SQL。
资料: https://learn.microsoft.com/en-us/ef/core/
      https://github.com/dotnet/efcore
      https://learn.microsoft.com/en-us/shows/entity-framework-core-101/
*/

record MigrationStep(string Sql);

record MigrationScript(string Id, IReadOnlyList<MigrationStep> Up, IReadOnlyList<MigrationStep> Down);

/** 直观版：一条手写 ALTER。 */
static class NaiveMigration
{
    public static string AlterAddColumn() => "ALTER TABLE Users ADD NickName NVARCHAR(64);";
}

/** 进阶版：迁移历史表 + 顺序应用。 */
sealed class MigrationRunner
{
    private readonly List<string> _applied = new();
    private readonly List<string> _schemaLog = new();

    public void Apply(IEnumerable<MigrationScript> pending)
    {
        foreach (var m in pending.OrderBy(x => x.Id, StringComparer.Ordinal))
        {
            if (_applied.Contains(m.Id)) continue;
            foreach (var step in m.Up)
                _schemaLog.Add($"[{m.Id}] {step.Sql}");
            _applied.Add(m.Id);
        }
    }

    public void RevertLast(MigrationScript script)
    {
        if (!_applied.Contains(script.Id)) return;
        foreach (var step in script.Down)
            _schemaLog.Add($"[REVERT {script.Id}] {step.Sql}");
        _applied.Remove(script.Id);
    }

    public IReadOnlyList<string> Applied => _applied;
    public IReadOnlyList<string> SchemaLog => _schemaLog;
}

static class Program
{
    static void RunTests()
    {
        if (!NaiveMigration.AlterAddColumn().Contains("NickName")) throw new Exception("Naive SQL 失败");

        var migrations = new[]
        {
            new MigrationScript(
                "2026010101_Init",
                new[] { new MigrationStep("CREATE TABLE Users (Id INT PRIMARY KEY, Name NVARCHAR(64));") },
                new[] { new MigrationStep("DROP TABLE Users;") }),
            new MigrationScript(
                "2026010201_AddEmail",
                new[] { new MigrationStep("ALTER TABLE Users ADD Email NVARCHAR(128);") },
                new[] { new MigrationStep("ALTER TABLE Users DROP COLUMN Email;") }),
        };

        var runner = new MigrationRunner();
        runner.Apply(migrations);
        if (runner.Applied.Count != 2) throw new Exception("应应用 2 个迁移");
        if (!runner.SchemaLog.Any(l => l.Contains("CREATE TABLE"))) throw new Exception("Init 未记录");

        runner.RevertLast(migrations[1]);
        if (runner.Applied.Count != 1) throw new Exception("Revert 后应剩 1 个");
        if (!runner.SchemaLog.Any(l => l.Contains("DROP COLUMN Email"))) throw new Exception("Down 未执行");

        Console.WriteLine("✅ MigrationsMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Migrations 心智 ************\n");
        Console.WriteLine("⚠ 步骤列表模拟 dotnet ef migrations；真实环境连数据库执行。");

        var runner = new MigrationRunner();
        runner.Apply(new[]
        {
            new MigrationScript("demo", new[] { new MigrationStep("CREATE TABLE Demo(Id INT);") }, Array.Empty<MigrationStep>())
        });
        Console.WriteLine(string.Join("\n", runner.SchemaLog));
        RunTests();
    }

    static void Main() => Bootstrap();
}