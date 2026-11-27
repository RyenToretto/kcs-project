using System;
using System.Collections.Generic;
using System.Text;

/*
[主题]: gRPC 心智——契约消息 + 一元 RPC 的内存模拟（不开真实 gRPC 服务器）
[难度]: Medium
[前置]: 10_microservices_grpc/00_split、06_aspnet_core

一、主题说明
gRPC 基于 HTTP/2 + Protocol Buffers：客户端按**契约**发请求，服务端返回强类型响应。
本课用 C# record 代替 .proto，用字典模拟「服务方法表」，理解 Unary 调用流程；
上线后再对照 Grpc.AspNetCore 与 .proto 文件。

二、写法对比
1. 直观版：直接调 C# 方法，无「序列化/路由」概念。
2. 进阶版：用字节载荷 + 方法名路由，模拟 on-wire 的一元 RPC。

三、真实用途
- 服务间高性能 RPC、流式推送、多语言互操作。
资料: https://learn.microsoft.com/en-us/aspnet/core/grpc/
      https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/
*/

record GreeterRequest(string Name);

record GreeterReply(string Message);

/** 直观版：普通接口调用，等价于「同进程、无网络」的 gRPC 语义。 */
interface IGreeterNaive
{
    GreeterReply SayHello(GreeterRequest req);
}

sealed class GreeterServiceNaive : IGreeterNaive
{
    public GreeterReply SayHello(GreeterRequest req) =>
        new($"你好, {req.Name}!");
}

/** 进阶版：模拟 gRPC 服务端「注册方法 + 处理序列化载荷」。 */
sealed class FakeGrpcServer
{
    private readonly Dictionary<string, Func<byte[], byte[]>> _methods = new(StringComparer.Ordinal);

    public void RegisterUnary<TReq, TRes>(
        string fullName,
        Func<TReq, TRes> handler,
        Func<TReq, byte[]> encodeReq,
        Func<byte[], TReq> decodeReq,
        Func<TRes, byte[]> encodeRes,
        Func<byte[], TRes> decodeRes)
    {
        _methods[fullName] = payload =>
        {
            var req = decodeReq(payload);
            var res = handler(req);
            return encodeRes(res);
        };
    }

    public byte[] Invoke(string fullName, byte[] requestPayload)
    {
        if (!_methods.TryGetValue(fullName, out var fn))
            throw new InvalidOperationException($"未注册方法: {fullName}");
        return fn(requestPayload);
    }
}

sealed class FakeGrpcClient
{
    private readonly FakeGrpcServer _server;
    private readonly string _service;

    public FakeGrpcClient(FakeGrpcServer server, string service) =>
        (_server, _service) = (server, service);

    public GreeterReply SayHello(GreeterRequest req)
    {
        var payload = Utf8Payload.Encode(req.Name);
        var responseBytes = _server.Invoke($"{_service}/SayHello", payload);
        var name = Utf8Payload.Decode(responseBytes);
        return new GreeterReply(name);
    }
}

static class Utf8Payload
{
    public static byte[] Encode(string text) => Encoding.UTF8.GetBytes(text);
    public static string Decode(byte[] bytes) => Encoding.UTF8.GetString(bytes);
}

static class Program
{
    const string GreeterService = "Greeter";

    static void RunTests()
    {
        var naive = new GreeterServiceNaive();
        var r1 = naive.SayHello(new GreeterRequest("小白"));
        if (r1.Message != "你好, 小白!") throw new Exception("直观版 SayHello 失败");

        var server = new FakeGrpcServer();
        server.RegisterUnary<GreeterRequest, GreeterReply>(
            $"{GreeterService}/SayHello",
            req => new GreeterReply($"你好, {req.Name}!"),
            r => Utf8Payload.Encode(r.Name),
            bytes => new GreeterRequest(Utf8Payload.Decode(bytes)),
            res => Utf8Payload.Encode(res.Message),
            bytes => new GreeterReply(Utf8Payload.Decode(bytes)));

        var client = new FakeGrpcClient(server, GreeterService);
        var r2 = client.SayHello(new GreeterRequest("进阶"));
        if (r2.Message != "你好, 进阶!") throw new Exception("模拟 gRPC 客户端失败");

        var raw = server.Invoke($"{GreeterService}/SayHello", Utf8Payload.Encode("直连"));
        if (Utf8Payload.Decode(raw) != "你好, 直连!") throw new Exception("服务端 Invoke 失败");

        Console.WriteLine("✅ GrpcMental 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 gRPC 心智（离线模拟） ************\n");
        Console.WriteLine("⚠ 本课不启动 Grpc.AspNetCore；仅理解 Unary 请求/响应与契约。");

        var server = new FakeGrpcServer();
        server.RegisterUnary<GreeterRequest, GreeterReply>(
            $"{GreeterService}/SayHello",
            req => new GreeterReply($"你好, {req.Name}!"),
            r => Utf8Payload.Encode(r.Name),
            bytes => new GreeterRequest(Utf8Payload.Decode(bytes)),
            res => Utf8Payload.Encode(res.Message),
            bytes => new GreeterReply(Utf8Payload.Decode(bytes)));

        var client = new FakeGrpcClient(server, GreeterService);
        Console.WriteLine($"模拟 RPC: {client.SayHello(new GreeterRequest("kcs")).Message}");

        RunTests();
    }

    static void Main() => Bootstrap();
}