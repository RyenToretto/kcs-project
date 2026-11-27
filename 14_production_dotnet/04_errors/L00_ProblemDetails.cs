using System;
using System.Collections.Generic;
using System.Text.Json;

/*
[主题]: RFC 7807 ProblemDetails——统一 API 错误响应
[难度]: Medium
[前置]: 06_aspnet_core

一、主题说明
客户端需要稳定字段：type、title、status、detail、errors（验证）。
陷阱：catch 后返回 200 + { success:false }，监控与网关无法按状态码分流。

二、写法对比
1. 直观版：任意 JSON 字符串 message。
2. 进阶版：ProblemDetails + 扩展字典 + 与 ValidationProblemDetails 对齐。

三、真实用途
- ASP.NET Core UseExceptionHandler / IProblemDetailsService。
资料: https://learn.microsoft.com/en-us/aspnet/core/web-api/#problem-details
      https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
*/

sealed class NaiveErrorResponse
{
    public bool Ok { get; init; }
    public string? Message { get; init; }
}

/** 进阶：与 ASP.NET Core 的 ProblemDetails 字段对齐的迷你模型。 */
sealed class ProblemDetailsDto
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int? Status { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}

static class ProblemFactory
{
    public static ProblemDetailsDto Validation(string detail, Dictionary<string, string[]> errors) =>
        new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = detail,
            Errors = errors
        };

    public static ProblemDetailsDto NotFound(string resource) =>
        new()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = 404,
            Detail = $"{resource} 不存在"
        };
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveErrorResponse { Ok = false, Message = "错了" };

        var p = ProblemFactory.Validation("输入无效", new()
        {
            ["Email"] = ["邮箱格式不正确"]
        });
        if (p.Status != 400 || p.Errors is null || p.Errors["Email"].Length != 1)
            throw new Exception("Validation ProblemDetails 字段不全");

        var json = JsonSerializer.Serialize(p);
        if (!json.Contains("\"Status\":400")) throw new Exception("应序列化 Status");

        var nf = ProblemFactory.NotFound("Order:99");
        if (nf.Status != 404 || nf.Title != "Not Found")
            throw new Exception("404 ProblemDetails 失败");

        Console.WriteLine("✅ ProblemDetails 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 ProblemDetails ************\n");
        var sample = ProblemFactory.NotFound("User:1");
        Console.WriteLine(JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true }));
        RunTests();
    }

    static void Main() => Bootstrap();
}