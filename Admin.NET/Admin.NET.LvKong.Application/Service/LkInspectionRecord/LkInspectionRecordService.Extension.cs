// ════════════════════════════════════════════════════════════════════════════
// ⚠  此文件为手写扩展文件，代码生成器不会覆盖，请在此文件中编写自定义业务逻辑。
// ════════════════════════════════════════════════════════════════════════════

using System.Net.Http;
using System.Text.Json;
using Furion.EventBus;
using SqlSugar;
using Admin.NET.LvKong.Application.Entity;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 检测记录服务扩展（partial class，与生成器主文件共享同一个类）
/// </summary>
public partial class LkInspectionRecordService
{
    // 如需在此添加方法或属性，直接写在这里即可。
}

// ─────────────────────────────────────────────────────────────────────────────
// 新增事件订阅者：所有"新增之后"的业务逻辑写在这里，与生成器完全隔离。
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// 检测记录新增事件订阅者
///
/// 负责：
///   1. 计算并回写试气次数（TestCount）
///   2. 泄漏值处理（受 EnableAutoUpdateLeakage 开关控制）：
///      - 钢印号为空（干试）→ 调用 MES 接口以二维码查询泄漏值
///        → 更新当前记录 + 同二维码历史中 Leakage == null 的记录
///      - 钢印号不为空（非干试）→ 直接将当前记录的 Leakage 回填历史空记录
/// </summary>
public class LkInspectionRecordEventSubscriber : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory   _httpClientFactory;

    public LkInspectionRecordEventSubscriber(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory   httpClientFactory)
    {
        _scopeFactory      = scopeFactory;
        _httpClientFactory = httpClientFactory;
    }

    // ── 事件处理入口 ──────────────────────────────────────────────────────────

    /// <summary>处理新增检测记录事件</summary>
    [EventSubscribe(LkInspectionRecordEventTypeEnum.Add)]
    public async Task OnAdd(EventHandlerExecutingContext context)
    {
        if (context.Source.Payload is not LkInspectionRecord newRecord) return;

        // 独立 Scope，避免 Singleton 持有 Scoped 服务
        await using var scope = _scopeFactory.CreateAsyncScope();
        var rep = scope.ServiceProvider
            .GetRequiredService<SqlSugarRepository<LkInspectionRecord>>();

        // 1. 计算试气次数
        await UpdateTestCountAsync(rep, newRecord);

        // 2. 泄漏值处理
        var enable = App.GetConfig<bool>("AppSettings:EnableAutoUpdateLeakage", true);
        if (!enable) return;

        if (string.IsNullOrWhiteSpace(newRecord.ProductModel)) return;

        await HandleLeakageAsync(rep, newRecord);
    }

    // ── 1. 试气次数 ────────────────────────────────────────────────────────────

    private static async Task UpdateTestCountAsync(
        SqlSugarRepository<LkInspectionRecord> rep,
        LkInspectionRecord newRecord)
    {
        int testCount = 1;
        if (!string.IsNullOrWhiteSpace(newRecord.ProductModel))
        {
            // 当前记录已入库，COUNT 含自身，即为第 N 次试气
            testCount = await rep.AsQueryable()
                .Where(r => r.ProductModel == newRecord.ProductModel
                         && r.Id <= newRecord.Id)
                .CountAsync();
        }

        if (testCount != newRecord.TestCount)
        {
            await rep.AsUpdateable(
                new LkInspectionRecord { Id = newRecord.Id, TestCount = testCount })
                .UpdateColumns(r => new { r.TestCount })
                .ExecuteCommandAsync();
            newRecord.TestCount = testCount; // 保持内存一致
        }
    }

    // ── 2. 泄漏值处理 ──────────────────────────────────────────────────────────

    private async Task HandleLeakageAsync(
        SqlSugarRepository<LkInspectionRecord> rep,
        LkInspectionRecord newRecord)
    {
        decimal? leakage;

        if (string.IsNullOrWhiteSpace(newRecord.SteelStamp))
        {
            // 干试（无钢印号）→ 从 MES 获取泄漏值
            leakage = await FetchLeakageFromMesAsync(newRecord.ProductModel!);
            if (leakage == null) return; // MES 未返回有效值，跳过

            // 更新当前记录的泄漏值
            await rep.AsUpdateable(
                new LkInspectionRecord { Id = newRecord.Id, Leakage = leakage })
                .UpdateColumns(r => new { r.Leakage })
                .ExecuteCommandAsync();
            newRecord.Leakage = leakage;
        }
        else
        {
            // 非干试：使用当前记录已有的 Leakage 值回填历史
            leakage = newRecord.Leakage;
            if (leakage == null || leakage == 0) return;
        }

        // 回填同一二维码中历史 Leakage == null 的记录（排除当前）
        await BackfillHistoryLeakageAsync(rep, newRecord.ProductModel!, leakage.Value, newRecord.Id);
    }

    // ── MES 接口调用 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 以二维码（productModel）为参数调用 MES 接口获取泄漏值。
    ///
    /// MES 接口约定（可根据实际接口格式调整）：
    ///   GET {MesApiUrl}?productModel={productModel}
    ///   响应 JSON：{ "leakage": 1.23 }  或  { "code": 0, "data": { "leakage": 1.23 } }
    ///
    /// 配置项（AppSettings）：
    ///   MesApiUrl            — 接口地址，例如 http://mes.local/api/leakage
    ///   MesApiTimeoutSeconds — 超时秒数，默认 10
    /// </summary>
    private async Task<decimal?> FetchLeakageFromMesAsync(string productModel)
    {
        var baseUrl = App.GetConfig<string>("AppSettings:MesApiUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            Console.WriteLine("[MES] MesApiUrl 未配置，跳过泄漏值查询");
            return null;
        }

        var timeout = App.GetConfig<int>("AppSettings:MesApiTimeoutSeconds") is int t && t > 0 ? t : 10;

        try
        {
            var client = _httpClientFactory.CreateClient("MES");
            client.Timeout = TimeSpan.FromSeconds(timeout);

            var url = $"{baseUrl.TrimEnd('/')}?productModel={Uri.EscapeDataString(productModel)}";
            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadAsStringAsync();
            return ParseLeakageFromResponse(body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MES] 查询泄漏值失败 (productModel={productModel}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 解析 MES 响应 JSON，尝试以下格式：
    ///   { "leakage": 1.23 }
    ///   { "data": { "leakage": 1.23 } }
    ///   { "code": 0, "data": 1.23 }
    /// </summary>
    private static decimal? ParseLeakageFromResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 格式 1：{ "leakage": 1.23 }
            if (root.TryGetProperty("leakage", out var v1) &&
                v1.TryGetDecimal(out var d1)) return d1;

            // 格式 2：{ "data": { "leakage": 1.23 } }
            if (root.TryGetProperty("data", out var data))
            {
                if (data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("leakage", out var v2) &&
                    v2.TryGetDecimal(out var d2)) return d2;

                // 格式 3：{ "data": 1.23 }
                if (data.TryGetDecimal(out var d3)) return d3;
            }

            Console.WriteLine($"[MES] 无法解析响应中的泄漏值: {json[..Math.Min(200, json.Length)]}");
            return null;
        }
        catch
        {
            return null;
        }
    }

    // ── 历史回填 ──────────────────────────────────────────────────────────────

    private static async Task BackfillHistoryLeakageAsync(
        SqlSugarRepository<LkInspectionRecord> rep,
        string productModel,
        decimal leakage,
        long excludeId)
    {
        var stale = await rep.AsQueryable()
            .Where(r => r.ProductModel == productModel
                     && r.Leakage == null
                     && r.Id != excludeId)
            .ToListAsync();

        if (stale.Count == 0) return;

        foreach (var r in stale) r.Leakage = leakage;

        await rep.AsUpdateable(stale)
            .UpdateColumns(r => new { r.Leakage })
            .ExecuteCommandAsync();
    }
}
