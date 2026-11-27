using System;
using System.Collections.Generic;
using System.Text;

/*
[主题]: ADR——架构决策记录可复盘
[难度]: Medium
[前置]: 19_architecture 前几课

一、主题说明
记录上下文、决策、后果、替代方案。新人 onboarding 与事故复盘依赖 ADR。

二、写法对比
1. 直观版：口头「当时就这么定的」。
2. 进阶版：结构化 AdrDocument + 状态 Superseded。

三、真实用途
- docs/architecture/decisions/adr-NNN.md 仓库内版本化。
资料: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
      https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

enum AdrStatus { Proposed, Accepted, Deprecated, Superseded }

sealed class AdrDocument
{
    public int Number { get; init; }
    public required string Title { get; init; }
    public AdrStatus Status { get; init; } = AdrStatus.Proposed;
    public required string Context { get; init; }
    public required string Decision { get; init; }
    public required IReadOnlyList<string> Consequences { get; init; }
    public IReadOnlyList<string> Alternatives { get; init; } = Array.Empty<string>();

    public string RenderMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# ADR-{Number:000} {Title}");
        sb.AppendLine($"Status: {Status}");
        sb.AppendLine("## Context").AppendLine(Context);
        sb.AppendLine("## Decision").AppendLine(Decision);
        sb.AppendLine("## Consequences");
        foreach (var c in Consequences) sb.AppendLine($"- {c}");
        sb.AppendLine("## Alternatives");
        foreach (var a in Alternatives) sb.AppendLine($"- {a}");
        return sb.ToString();
    }
}

static class Program
{
    static void RunTests()
    {
        var adr = new AdrDocument
        {
            Number = 1,
            Title = "采用 PostgreSQL 作为主库",
            Status = AdrStatus.Accepted,
            Context = "需要 JSON 与可靠事务",
            Decision = "PostgreSQL 15+",
            Consequences = new[] { "团队需学习 PG 运维", "EF Npgsql 驱动" },
            Alternatives = new[] { "SQL Server", "MySQL" }
        };

        var md = adr.RenderMarkdown();
        if (!md.Contains("ADR-001") || !md.Contains("PostgreSQL")) throw new Exception("ADR 渲染");
        if (!md.Contains("SQL Server")) throw new Exception("应列备选");

        Console.WriteLine("✅ AdrChecklist 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 ADR Checklist ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}