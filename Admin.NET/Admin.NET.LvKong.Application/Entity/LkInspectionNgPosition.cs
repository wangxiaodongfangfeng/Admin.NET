// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.LvKong.Application.Entity;

/// <summary>
/// 绿控检测NG位置详情表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("Lk_InspectionNgPosition", "绿控检测NG位置详情表")]
[SugarIndex("Index_InspectionNgPosition_InspectionId", nameof(InspectionId), OrderByType.Asc)]
public partial class LkInspectionNgPosition : EntityBase
{
    /// <summary>
    /// 检测记录ID
    /// </summary>
    [SugarColumn(ColumnName = "InspectionId", ColumnDescription = "检测记录ID")]
    public virtual long InspectionId { get; set; }

    /// <summary>
    /// NG位置ID
    /// </summary>
    [SugarColumn(ColumnName = "PositionId", ColumnDescription = "NG位置ID")]
    public virtual long PositionId { get; set; }

    /// <summary>
    /// 图片URL
    /// </summary>
    [SugarColumn(ColumnName = "ImageUrl", ColumnDescription = "图片URL", Length = 256, IsNullable = true)]
    public virtual string? ImageUrl { get; set; }

    /// <summary>
    /// 泄露程度ID
    /// </summary>
    [SugarColumn(ColumnName = "LeakageSeverityId", ColumnDescription = "泄露程度ID", IsNullable = true)]
    public virtual long? LeakageSeverityId { get; set; }

    #region 导航属性

    /// <summary>
    /// 检测记录
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(InspectionId))]
    public virtual LkInspectionRecord InspectionRecord { get; set; }

    /// <summary>
    /// NG位置信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PositionId))]
    public virtual LkNgPosition Position { get; set; }

    /// <summary>
    /// 泄露程度信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(LeakageSeverityId))]
    public virtual LkLeakageSeverity LeakageSeverity { get; set; }

    #endregion
}
