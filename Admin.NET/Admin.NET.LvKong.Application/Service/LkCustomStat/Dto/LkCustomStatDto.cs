using System.ComponentModel.DataAnnotations;

namespace Admin.NET.LvKong.Application;

// ─── CRUD ────────────────────────────────────────────────────────────────────

/// <summary>
/// 自定义统计定义 - 新增
/// </summary>
public class AddLkCustomStatDefInput
{
    /// <summary>统计名称</summary>
    [Required(ErrorMessage = "统计名称不能为空")]
    [MaxLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// SQL 模板，支持占位符：
    /// {{date_start}} {{date_end}} {{month_prefix}} {{shift_cutoff}} {{product_status_id}} {{user_id}}
    /// </summary>
    [Required(ErrorMessage = "SQL模板不能为空")]
    public string SqlTemplate { get; set; }

    /// <summary>筛选器配置 JSON，如 ["date","product_status"]</summary>
    public string? FilterConfig { get; set; }

    /// <summary>描述</summary>
    [MaxLength(512)]
    public string? Description { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 自定义统计定义 - 更新
/// </summary>
public class UpdateLkCustomStatDefInput : AddLkCustomStatDefInput
{
    [Required]
    public long Id { get; set; }
}

/// <summary>
/// 自定义统计定义 - 查询 by ID
/// </summary>
public class QueryLkCustomStatDefInput
{
    [Required]
    public long Id { get; set; }
}

/// <summary>
/// 自定义统计定义 - 删除
/// </summary>
public class DeleteLkCustomStatDefInput
{
    [Required]
    public long Id { get; set; }
}

/// <summary>
/// 自定义统计定义 - 输出
/// </summary>
public class LkCustomStatDefOutput
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string SqlTemplate { get; set; }
    public string? FilterConfig { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

// ─── 执行查询 ─────────────────────────────────────────────────────────────────

/// <summary>
/// 执行自定义统计 - 入参
/// </summary>
public class RunLkCustomStatInput
{
    /// <summary>统计定义 ID</summary>
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// 统计天起始日期（yyyy-MM-dd）。
    /// 不传时按 08:30 规则自动计算当前统计天。
    /// 对应占位符 {{date_start}} / {{date_end}}。
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// 月份（yyyy-MM）。
    /// 对应占位符 {{month_prefix}}。
    /// 不传时取当前年月。
    /// </summary>
    public string? Month { get; set; }

    /// <summary>
    /// 产品状态 ID。
    /// 对应占位符 {{product_status_id}}。
    /// 不传时使用 IsDefault=true 的产品状态 ID。
    /// </summary>
    public long? ProductStatusId { get; set; }

    /// <summary>
    /// 用户 ID。
    /// 对应占位符 {{user_id}}。
    /// </summary>
    public long? UserId { get; set; }
}

/// <summary>
/// 执行自定义统计 - 输出（每行是列名→值的字典）
/// </summary>
public class LkCustomStatResult
{
    /// <summary>统计定义名称</summary>
    public string Name { get; set; }

    /// <summary>实际使用的参数快照（用于前端展示）</summary>
    public LkCustomStatParams UsedParams { get; set; }

    /// <summary>列名列表（按 SQL SELECT 顺序）</summary>
    public List<string> Columns { get; set; }

    /// <summary>数据行，每行是 List&lt;object?&gt;，与 Columns 顺序对应</summary>
    public List<List<object?>> Rows { get; set; }
}

/// <summary>
/// 实际生效的参数快照
/// </summary>
public class LkCustomStatParams
{
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? MonthPrefix { get; set; }
    public string? ShiftCutoff { get; set; }
    public long? ProductStatusId { get; set; }
    public string? ProductStatusName { get; set; }
    public long? UserId { get; set; }
}
