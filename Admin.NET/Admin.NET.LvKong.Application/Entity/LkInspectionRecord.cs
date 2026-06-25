// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using SqlSugar;

namespace Admin.NET.LvKong.Application.Entity;

/// <summary>
/// 绿控检测记录表
/// </summary>
[Tenant("1300000000001")]
[SugarTable("Lk_InspectionRecord", "绿控检测记录表")]
[SugarIndex("Index_InspectionRecord_Date", nameof(Date), OrderByType.Asc)]
[SugarIndex("Index_InspectionRecord_UserId", nameof(UserId), OrderByType.Asc)]
[SugarIndex("Index_InspectionRecord_TestResult", nameof(TestResult), OrderByType.Asc)]
[SugarIndex("Index_InspectionRecord_ProductTypeId", nameof(ProductTypeId), OrderByType.Asc)]
public partial class LkInspectionRecord : EntityBase
{
    /// <summary>
    /// 操作员
    /// </summary>
    [SugarColumn(ColumnName = "Operator", ColumnDescription = "操作员", Length = 32)]
    public virtual string Operator { get; set; }

    /// <summary>
    /// 检测日期
    /// </summary>
    [SugarColumn(ColumnName = "Date", ColumnDescription = "检测日期", Length = 16)]
    public virtual string Date { get; set; }

    /// <summary>
    /// 班次ID
    /// </summary>
    [SugarColumn(ColumnName = "ShiftId", ColumnDescription = "班次ID")]
    public virtual long ShiftId { get; set; }

    /// <summary>
    /// 保压时间(Min)
    /// </summary>
    [SugarColumn(ColumnName = "PressureHoldTime", ColumnDescription = "保压时间", DecimalDigits = 2)]
    public virtual decimal PressureHoldTime { get; set; }

    /// <summary>
    /// 压力值(MPa)
    /// </summary>
    [SugarColumn(ColumnName = "Pressure", ColumnDescription = "气压值", DecimalDigits = 2)]
    public virtual decimal Pressure { get; set; }
    
    /// <summary>
    /// 产品类型ID
    /// </summary>
    [SugarColumn(ColumnName = "ProductTypeId", ColumnDescription = "产品类型ID")]
    public virtual long ProductTypeId { get; set; }

    /// <summary>
    /// 产品类型ID
    /// </summary>
    [SugarColumn(ColumnName = "ProductStatusId", ColumnDescription = "产品状态ID")]
    public virtual long ProductStatusId { get; set; }
    
    /// <summary>
    /// 产品类型ID
    /// </summary>
    [SugarColumn(ColumnName = "PartStatusId", ColumnDescription = "零件状态ID")]
    public virtual long PartStatusId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>
    [SugarColumn(ColumnName = "NgPositionId", ColumnDescription = "Ng位置ID")]
    public virtual long NgPositionId { get; set; } 

    
    /// <summary>
    /// 产品型号
    /// </summary>
    [SugarColumn(ColumnName = "ProductModel", ColumnDescription = "产品型号", Length = 64, IsNullable = true)]
    public virtual string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    [SugarColumn(ColumnName = "SteelStamp", ColumnDescription = "钢印号", Length = 64, IsNullable = true)]
    public virtual string? SteelStamp { get; set; }

    /// <summary>
    /// 检测结果(OK/NG)
    /// </summary>
    [SugarColumn(ColumnName = "TestResult", ColumnDescription = "检测结果", Length = 8)]
    public virtual string TestResult { get; set; }

    /// <summary>
    /// 泄露值(ml/min)
    /// </summary>
    [SugarColumn(ColumnName = "Leakage", ColumnDescription = "泄露值", DecimalDigits = 2)]
    public virtual decimal Leakage { get; set; }

    /// <summary>
    /// 图片URL列表(JSON数组)
    /// </summary>
    [SugarColumn(ColumnName = "Images", ColumnDescription = "图片URL列表", ColumnDataType = "text", IsNullable = true)]
    public virtual string? Images { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnName = "UserId", ColumnDescription = "用户ID")]
    public virtual long UserId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remarks", ColumnDescription = "备注", Length = 512, IsNullable = true)]
    public virtual string? Remarks { get; set; }

    #region 导航属性

    /// <summary>
    /// 班次信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ShiftId))]
    public virtual LkShift Shift { get; set; }

    /// <summary>
    /// 产品类型信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProductTypeId))]
    public virtual LkProductType ProductType { get; set; }

    /// <summary>
    /// 产品状态信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ProductStatusId))]
    public virtual LkProductStatus ProductStatus { get; set; }
    
    /// <summary>
    /// 零件状态信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PartStatusId))]
    public virtual LkPartStatus PartStatus { get; set; }
    /// <summary>
    /// NG位置信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(NgPositionId))]
    public virtual LkNgPosition LkNgPosition { get; set; }
    #endregion
}
