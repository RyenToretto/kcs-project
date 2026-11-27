using System;
using System.Collections.Generic;

/*
[主题]: CQRS——命令改状态、查询只读
[难度]: Hard
[前置]: 19_architecture/00_ddd

一、主题说明
写模型保证不变式；读模型可投影/反范式优化 UI。
陷阱：为小型 CRUD 强行双库双模型徒增复杂度。

二、写法对比
1. 直观版：同一 Service 又写又复杂 Join 读。
2. 进阶版：ICommandHandler + 独立 IQuery 返回 DTO。

三、真实用途
- 高读 QPS 报表；与 Event Sourcing 可选组合。
资料: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
*/

record CreateOrderCommand(Guid Id, decimal Total);
record OrderSummaryDto(Guid Id, decimal Total);

interface ICommandHandler<TCommand>
{
    void Handle(TCommand command);
}

interface IQueryHandler<TQuery, TResult>
{
    TResult Handle(TQuery query);
}

sealed class NaiveCrud
{
    private readonly Dictionary<Guid, decimal> _db = new();
    public void Save(Guid id, decimal total) => _db[id] = total;
    public decimal Get(Guid id) => _db[id];
}

sealed class OrderStore
{
    private readonly Dictionary<Guid, decimal> _write = new();
    public void Apply(CreateOrderCommand cmd) => _write[cmd.Id] = cmd.Total;
    public OrderSummaryDto? Query(Guid id) =>
        _write.TryGetValue(id, out var t) ? new OrderSummaryDto(id, t) : null;
}

sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly OrderStore _store;
    public CreateOrderHandler(OrderStore store) => _store = store;
    public void Handle(CreateOrderCommand command) => _store.Apply(command);
}

sealed class GetOrderHandler : IQueryHandler<Guid, OrderSummaryDto?>
{
    private readonly OrderStore _store;
    public GetOrderHandler(OrderStore store) => _store = store;
    public OrderSummaryDto? Handle(Guid query) => _store.Query(query);
}

static class Program
{
    static void RunTests()
    {
        _ = new NaiveCrud();

        var store = new OrderStore();
        var cmd = new CreateOrderHandler(store);
        var qry = new GetOrderHandler(store);

        var id = Guid.NewGuid();
        cmd.Handle(new CreateOrderCommand(id, 12m));
        var dto = qry.Handle(id);
        if (dto is null || dto.Total != 12m) throw new Exception("CQRS 读写");

        Console.WriteLine("✅ CqrsSplit 所有测试用例通过！");
    }

    static void Bootstrap()
    {
        Console.WriteLine("\n\n************ L00 CQRS Split ************\n");
        RunTests();
    }

    static void Main() => Bootstrap();
}