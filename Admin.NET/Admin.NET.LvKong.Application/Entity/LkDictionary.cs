// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.LvKong.Application.Entity;

/// <summary>
/// 绿控数据字典表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("Lk_Dictionary", "绿控数据字典表")]
[SugarIndex("Index_Dictionary_Name", nameof(DictionaryName), OrderByType.Asc)]
[SugarIndex("Index_Dictionary_IsActive", nameof(IsActive), OrderByType.Desc)]
public partial class LkDictionary : EntityBase
{
    /// <summary>
    /// 字典名称(英文标识)
    /// </summary>
    [SugarColumn(ColumnName = "DictionaryName", ColumnDescription = "字典名称", Length = 100)]
    public virtual string DictionaryName { get; set; }

    /// <summary>
    /// 显示名称(中文名称)
    /// </summary>
    [SugarColumn(ColumnName = "DisplayName", ColumnDescription = "显示名称", Length = 100)]
    public virtual string DisplayName { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    [SugarColumn(ColumnName = "ItemName", ColumnDescription = "项目名称", Length = 100)]
    public virtual string ItemName { get; set; }

    /// <summary>
    /// 项目值(可选代码)
    /// </summary>
    [SugarColumn(ColumnName = "ItemValue", ColumnDescription = "项目值", Length = 100, IsNullable = true)]
    public virtual string? ItemValue { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    [SugarColumn(ColumnName = "SortOrder", ColumnDescription = "排序顺序")]
    public virtual int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "IsActive", ColumnDescription = "是否启用")]
    public virtual bool IsActive { get; set; } = true;

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", ColumnDataType = "text", IsNullable = true)]
    public virtual string? Description { get; set; }
}
