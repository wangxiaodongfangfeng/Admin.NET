// ════════════════════════════════════════════════════════════════════════════
// ⚠  此文件为手写扩展文件，代码生成器不会覆盖，请在此文件中编写自定义业务逻辑。
// ════════════════════════════════════════════════════════════════════════════

using Furion.EventBus;
using SqlSugar;
using Admin.NET.LvKong.Application.Entity;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 检测记录服务扩展
/// 通过 partial class 与生成器主文件共享同一个类，
/// 本文件永远不会被代码生成器覆盖。
/// </summary>
public partial class LkInspectionRecordService
{
    // 此处可注入额外依赖，或声明本文件专用的字段。
    // 构造函数在主文件中，如需新依赖请修改主文件（或在下次生成后手动补回）。
}

// ─────────────────────────────────────────────────────────────────────────────
// EventSubscriber：监听 LkInspectionRecordEventTypeEnum.Add 事件
// 所有"新增之后"的业务逻辑写在这里，与生成器完全隔离。
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// 检测记录新增事件订阅者
/// 负责：
///   1. 计算并回写试气次数（TestCount）
///   2. 将新泄漏值回填到同一二维码的历史空记录（受 EnableAutoUpdateLeakage 开关控制）
/// </summary>
public class LkInspectionRecordEventSubscriber : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LkInspectionRecordEventSubscriber(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 处理新增检测记录事件
    /// </summary>
    [EventSubscribe(LkInspectionRecordEventTypeEnum.Add)]
    public async Task OnAdd(EventHandlerExecutingContext context)
    {
        if (context.Source.Payload is not LkInspectionRecord newRecord) return;

        // 使用独立 Scope，避免 Singleton 持有 Scoped 服务
        await using var scope = _scopeFactory.CreateAsyncScope();
        var rep = scope.ServiceProvider
            .GetRequiredService<SqlSugarRepository<LkInspectionRecord>>();

        // ── 1. 计算试气次数并更新 ──────────────────────────────────────────
        // 计算该二维码在此记录插入后的序号
        // （+1 是因为当前记录已经入库，COUNT 已含它自己）
        int testCount = 1;
        if (!string.IsNullOrWhiteSpace(newRecord.ProductModel))
        {
            testCount = await rep.AsQueryable()
                .Where(r => r.ProductModel == newRecord.ProductModel
                         && r.Id <= newRecord.Id)
                .CountAsync();
        }

        if (testCount != newRecord.TestCount)
        {
            await rep.AsUpdateable(new LkInspectionRecord { Id = newRecord.Id, TestCount = testCount })
            .UpdateColumns(r => new { r.TestCount })
            .ExecuteCommandAsync();
        }

        // ── 2. Leakage 回填（受开关控制） ──────────────────────────────────
        var enableAutoUpdate = App.GetConfig<bool>("AppSettings:EnableAutoUpdateLeakage", true);
        if (!enableAutoUpdate) return;

        if (string.IsNullOrWhiteSpace(newRecord.ProductModel)
            || newRecord.Leakage == null
            || newRecord.Leakage == 0)
            return;

        var stale = await rep.AsQueryable()
            .Where(r => r.ProductModel == newRecord.ProductModel
                     && r.Leakage == null
                     && r.Id != newRecord.Id)
            .ToListAsync();

        if (stale.Count == 0) return;

        foreach (var r in stale) r.Leakage = newRecord.Leakage;

        await rep.AsUpdateable(stale)
            .UpdateColumns(r => new { r.Leakage })
            .ExecuteCommandAsync();
    }
}
