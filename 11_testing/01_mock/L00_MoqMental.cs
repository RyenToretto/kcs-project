using System;
using System.Collections.Generic;

/*
[主题]: Mock / Fake 心智——手写测试替身，对照 Moq 的 Setup/Verify
[难度]: Medium
[前置]: 01_oop_csharp/02_interface、11_testing/00_xunit

一、主题说明
单元测试要隔离「被测单元」与数据库、HTTP 等外部依赖，常用 **接口 + 假实现（Fake/Mock）**。
Moq 用动态代理生成替身；本课手写可记录调用次数的假对象，理解 Setup 与 Verify 在测什么。

二、写法对比
1. 直观版：固定返回值的 Stub。
2. 进阶版：记录每次调用参数，支持 Verify 是否按预期被调用。

三、真实用途
- 测应用服务时替换 IEmailSender、IRepository；生产用 Moq/NSubstitute。
资料: https://github.com/moq/moq
      https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test
*/

interface IWeatherClient
{
    int GetCelsius(string city);
}

/** 直观版：永远返回常量的桩（Stub）。 */
sealed class ConstantWeatherStub : IWeatherClient
{
    private readonly int _c;

    public ConstantWeatherStub(int celsius) => _c = celsius;

    public int GetCelsius(string city) => _c;
}

/** 进阶版：可配置返回值 + 记录调用（Mock 心智）。 */
sealed class RecordingWeatherMock : IWeatherClient
{
    public int ReturnValue { get; set; } = 25;
    public List<string> CalledCities { get; } = [];

    public int GetCelsius(string city)
    {
        CalledCities.Add(city);
        return ReturnValue;
    }

    public void VerifyCalledOnce(string city)
    {
        var count = CalledCities.FindAll(c => c == city).Count;
        if (count != 1) throw new Exception($"期望对 {city} 调用 1 次，实际 {count} 次");
    }
}

sealed class TravelAdviceService
{
    private readonly IWeatherClient _weather;

    public TravelAdviceService(IWeatherClient weather) => _weather = weather;

    public string Advise(string city)
    {
        var c = _weather.GetCelsius(city);
        return c >= 30 ? "带遮阳帽" : c <= 5 ? "穿羽绒服" : "舒适出行";
    }
}

static class Program
{
    static void RunTests()
    {
        var stub = new ConstantWeatherStub(35);
        var svcStub = new TravelAdviceService(stub);
        if (svcStub.Advise("上海") != "带遮阳帽") throw new Exception("Stub 高温建议失败");

        var mock = new RecordingWeatherMock { ReturnValue = 2 };
        var svcMock = new TravelAdviceService(mock);
        var tip = svcMock.Advise("哈尔滨");
        if (tip != "穿羽绒服") throw new Exception("Mock 低温建议失败");
        mock.VerifyCalledOnce("哈尔滨");

        mock.ReturnValue = 20;
        mock.CalledCities.Clear();
        _ = svcMock.Advise("北京");
        if (mock.CalledCities.Count != 1 || mock.CalledCities[0] != "北京")
            throw new Exception("应记录北京调用");

        Console.WriteLine("✅ MoqMental 所有测试用例通过！");
        Console.WriteLine("ℹ 生产可用 Moq: mock.Setup(x => x.GetCelsius(\"x\")).Returns(1);");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 Mock 心智（手写替身） ************\n");

        var mock = new RecordingWeatherMock { ReturnValue = 28 };
        var svc = new TravelAdviceService(mock);
        Console.WriteLine($"哈尔滨建议: {svc.Advise("哈尔滨")}（已记录 {mock.CalledCities.Count} 次调用）");

        RunTests();
    }

    static void Main() => Bootstrap();
}