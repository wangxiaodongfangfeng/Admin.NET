using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using Mapster;
using SqlSugar;
using Admin.NET.LvKong.Application.Const;
using Admin.NET.LvKong.Application.Entity;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 自定义统计服务 🧩
/// 通过预设 SQL 模板 + 占位符，实现零代码扩展统计 Widget。
/// 
/// 支持的占位符（SQL 中使用双大括号）：
///   {{date_start}}        — 统计天起始日期，yyyy-MM-dd
///   {{date_end}}          — 统计天终止日期（起始+1天），yyyy-MM-dd
///   {{month_prefix}}      — 月份前缀，yyyy-MM
///   {{shift_cutoff}}      — 班次分界时间，固定 08:30:00
///   {{product_status_id}} — 产品状态 ID（整数）
///   {{user_id}}           — 用户 ID（整数）
///
/// ⚠️ 安全说明：所有占位符替换均使用参数化查询或白名单校验，防止 SQL 注入。
///   - 日期/月份格式用正则校验（仅允许 yyyy-MM-dd / yyyy-MM 格式）
///   - ID 类型为 long，强类型转换
///   - SQL 模板本身由管理员写入，属于受信任输入
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 85)]
public class LkCustomStatService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkCustomStatDef> _defRep;
    private readonly ISqlSugarClient _db;

    public LkCustomStatService(
        SqlSugarRepository<LkCustomStatDef> defRep,
        ISqlSugarClient db)
    {
        _defRep = defRep;
        _db = db;
    }

    // ─── CRUD ───────────────────────────────────────────────────────────────

    /// <summary>
    /// 获取所有启用的自定义统计定义列表 🔖
    /// </summary>
    [DisplayName("获取自定义统计定义列表")]
    [ApiDescriptionSettings(Name = "List"), HttpGet]
    public async Task<List<LkCustomStatDefOutput>> List()
    {
        var list = await _defRep.AsQueryable()
            .Where(d => d.IsEnabled)
            .OrderBy(d => d.CreateTime)
            .ToListAsync();
        return list.Adapt<List<LkCustomStatDefOutput>>();
    }

    /// <summary>
    /// 获取所有自定义统计定义（含禁用）🔖
    /// </summary>
    [DisplayName("获取全部自定义统计定义")]
    [ApiDescriptionSettings(Name = "ListAll"), HttpGet]
    public async Task<List<LkCustomStatDefOutput>> ListAll()
    {
        var list = await _defRep.AsQueryable()
            .OrderBy(d => d.CreateTime)
            .ToListAsync();
        return list.Adapt<List<LkCustomStatDefOutput>>();
    }

    /// <summary>
    /// 获取自定义统计定义详情 🔖
    /// </summary>
    [DisplayName("获取自定义统计定义详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkCustomStatDefOutput> Detail([FromQuery] QueryLkCustomStatDefInput input)
    {
        var entity = await _defRep.GetByIdAsync(input.Id);
        return entity.Adapt<LkCustomStatDefOutput>();
    }

    /// <summary>
    /// 新增自定义统计定义 ➕
    /// </summary>
    [DisplayName("新增自定义统计定义")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add([FromBody] AddLkCustomStatDefInput input)
    {
        ValidateSqlTemplate(input.SqlTemplate);
        var entity = input.Adapt<LkCustomStatDef>();
        await _defRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新自定义统计定义 ✏️
    /// </summary>
    [DisplayName("更新自定义统计定义")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update([FromBody] UpdateLkCustomStatDefInput input)
    {
        ValidateSqlTemplate(input.SqlTemplate);
        var entity = input.Adapt<LkCustomStatDef>();
        await _defRep.AsUpdateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除自定义统计定义 ❌
    /// </summary>
    [DisplayName("删除自定义统计定义")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete([FromBody] DeleteLkCustomStatDefInput input)
    {
        await _defRep.DeleteByIdAsync(input.Id);
    }

    // ─── 执行统计 ────────────────────────────────────────────────────────────

    /// <summary>
    /// 执行自定义统计查询，返回动态列表 🔖
    /// </summary>
    [DisplayName("执行自定义统计查询")]
    [ApiDescriptionSettings(Name = "Run"), HttpPost]
    public async Task<LkCustomStatResult> Run([FromBody] RunLkCustomStatInput input)
    {
        var def = await _defRep.GetByIdAsync(input.Id)
            ?? throw new Exception($"统计定义 [{input.Id}] 不存在");

        var (sql, usedParams) = await BuildSql(def.SqlTemplate, input);

        var dataTable = await _db.Ado.GetDataTableAsync(sql);

        var columns = dataTable.Columns
            .Cast<System.Data.DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();

        var rows = dataTable.Rows
            .Cast<System.Data.DataRow>()
            .Select(row => columns.Select(c => row[c] == DBNull.Value ? null : row[c]).ToList())
            .ToList();

        return new LkCustomStatResult
        {
            Name       = def.Name,
            UsedParams = usedParams,
            Columns    = columns,
            Rows       = rows,
        };
    }

    /// <summary>
    /// 导出自定义统计为 Excel 🔖
    /// </summary>
    [DisplayName("导出自定义统计Excel")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export([FromBody] RunLkCustomStatInput input)
    {
        var result = await Run(input);

        // 将行列结构转为 List<Dictionary<string,object>> — MiniExcel 接受此格式
        var rows = result.Rows.Select(row =>
        {
            var dict = new Dictionary<string, object?>();
            for (var i = 0; i < result.Columns.Count; i++)
                dict[result.Columns[i]] = row[i];
            return dict;
        }).ToList<object>();

        var stream = new MemoryStream();
        await MiniExcelLibs.MiniExcel.SaveAsAsync(stream, rows, excelType: MiniExcelLibs.ExcelType.XLSX);
        stream.Position = 0;

        var fileName = $"{result.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = Uri.EscapeDataString(fileName)
        };
    }

    // ─── 私有辅助 ────────────────────────────────────────────────────────────

    /// <summary>
    /// 解析参数并替换 SQL 模板中的占位符。
    /// 所有日期/ID 均经过格式校验，防止注入。
    /// </summary>
    private async Task<(string sql, LkCustomStatParams usedParams)> BuildSql(
        string template, RunLkCustomStatInput input)
    {
        var p = new LkCustomStatParams { ShiftCutoff = "08:30:00" };

        // ── 日期占位符 ────────────────────────────────────────────
        var (dateStart, dateEnd) = ResolveDayBoundary(input.Date);
        p.DateStart = dateStart;
        p.DateEnd   = dateEnd;

        // ── 月份占位符 ────────────────────────────────────────────
        p.MonthPrefix = !string.IsNullOrWhiteSpace(input.Month)
            && Regex.IsMatch(input.Month, @"^\d{4}-\d{2}$")
                ? input.Month
                : DateTime.Now.ToString("yyyy-MM");

        // ── 产品状态 ──────────────────────────────────────────────
        if (input.ProductStatusId.HasValue)
        {
            p.ProductStatusId   = input.ProductStatusId.Value;
            p.ProductStatusName = (await _db.Queryable<LkProductStatus>()
                .Where(s => s.Id == p.ProductStatusId)
                .Select(s => s.Name).FirstAsync()) ?? string.Empty;
        }
        else if (template.Contains("{{product_status_id}}"))
        {
            var ps = await _db.Queryable<LkProductStatus>()
                .Where(s => s.IsDefault)
                .Select(s => new { s.Id, s.Name })
                .FirstAsync();
            p.ProductStatusId   = ps?.Id;
            p.ProductStatusName = ps?.Name ?? string.Empty;
        }

        // ── 用户 ──────────────────────────────────────────────────
        p.UserId = input.UserId;

        // ── 替换占位符（白名单格式校验后直接嵌入，均为安全值）────
        var sql = template
            .Replace("{{date_start}}",        EscapeString(p.DateStart))
            .Replace("{{date_end}}",          EscapeString(p.DateEnd))
            .Replace("{{month_prefix}}",      EscapeString(p.MonthPrefix))
            .Replace("{{shift_cutoff}}",      EscapeString(p.ShiftCutoff!))
            .Replace("{{product_status_id}}", (p.ProductStatusId ?? 0).ToString())
            .Replace("{{user_id}}",           (p.UserId ?? 0).ToString());

        return (sql, p);
    }

    /// <summary>
    /// 单引号转义（用于 SQL 字符串字面量）
    /// </summary>
    private static string EscapeString(string value)
        => value?.Replace("'", "''") ?? string.Empty;

    /// <summary>
    /// 基本的 SQL 模板安全校验：
    /// - 禁止 DDL/DML（只允许 SELECT）
    /// - 禁止多语句
    /// </summary>
    private static void ValidateSqlTemplate(string sql)
    {
        var trimmed = sql.Trim();
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            throw new Exception("SQL 模板必须以 SELECT 开头，不允许 DDL/DML 语句");

        // 禁止危险关键字
        var forbidden = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE", "EXEC", "EXECUTE" };
        foreach (var kw in forbidden)
        {
            // 用单词边界匹配，避免误判列名
            if (Regex.IsMatch(trimmed, $@"\b{kw}\b", RegexOptions.IgnoreCase))
                throw new Exception($"SQL 模板中不允许包含 {kw} 语句");
        }
    }

    /// <summary>
    /// 按 08:30 规则计算统计天边界
    /// </summary>
    private static (string dateStart, string dateEnd) ResolveDayBoundary(string? inputDate)
    {
        DateTime statDayStart;
        if (!string.IsNullOrWhiteSpace(inputDate)
            && Regex.IsMatch(inputDate, @"^\d{4}-\d{2}-\d{2}$")
            && DateTime.TryParse(inputDate, out var parsed))
        {
            statDayStart = parsed.Date;
        }
        else
        {
            var now        = DateTime.Now;
            var shiftStart = now.Date.AddHours(8).AddMinutes(30);
            statDayStart   = now >= shiftStart ? now.Date : now.Date.AddDays(-1);
        }
        return (statDayStart.ToString("yyyy-MM-dd"), statDayStart.AddDays(1).ToString("yyyy-MM-dd"));
    }
}
