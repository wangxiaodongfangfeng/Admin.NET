using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.LvKong.Application.Entity;

/// <summary>
/// 自定义统计定义表
/// 存储用户配置的 SQL 模板和筛选器配置，用于在首页 Widget 中动态渲染统计卡片。
/// </summary>
[Tenant("1300000000001")]
[SugarTable("Lk_CustomStatDef", "自定义统计定义表")]
public class LkCustomStatDef : EntityBase
{
    /// <summary>
    /// 统计名称，显示在 Widget 标题
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "统计名称", Length = 128)]
    public string Name { get; set; }

    /// <summary>
    /// SQL 模板，支持以下占位符（区分大小写）：
    ///   {{date_start}}        — 统计天起始日期，格式 yyyy-MM-dd
    ///   {{date_end}}          — 统计天结束日期（起始日期+1天），格式 yyyy-MM-dd
    ///   {{month_prefix}}      — 月份前缀，格式 yyyy-MM
    ///   {{shift_cutoff}}      — 班次分界时间字符串，固定 08:30:00
    ///   {{product_status_id}} — 产品状态 ID（整数）
    ///   {{user_id}}           — 用户 ID（整数）
    /// 示例：
    ///   SELECT ProductModel AS 产品型号, COUNT(*) AS 检测次数
    ///   FROM Lk_InspectionRecord
    ///   WHERE Date LIKE '{{month_prefix}}%'
    ///   GROUP BY ProductModel
    ///   ORDER BY COUNT(*) DESC
    /// </summary>
    [SugarColumn(ColumnName = "SqlTemplate", ColumnDescription = "SQL模板", ColumnDataType = "text")]
    public string SqlTemplate { get; set; }

    /// <summary>
    /// 筛选器配置（JSON 数组），决定 Widget 顶部显示哪些筛选控件。
    /// 可选值：date | month | product_status | user
    /// 示例：["date","product_status"]
    /// </summary>
    [SugarColumn(ColumnName = "FilterConfig", ColumnDescription = "筛选器配置JSON", Length = 512, IsNullable = true)]
    public string? FilterConfig { get; set; }

    /// <summary>
    /// 描述/备注
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "IsEnabled", ColumnDescription = "是否启用", DefaultValue = "True")]
    public bool IsEnabled { get; set; } = true;
}
