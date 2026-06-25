// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core;
using System.ComponentModel.DataAnnotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 检测记录基础输入参数
/// </summary>
public class LkInspectionRecordBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 操作员
    /// </summary>
    [Required(ErrorMessage = "操作员不能为空")]
    public virtual string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>
    [Required(ErrorMessage = "检测日期不能为空")]
    public virtual string Date { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    [Required(ErrorMessage = "班次不能为空")]
    public virtual long? ShiftId { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>
    [Required(ErrorMessage = "保压时间不能为空")]
    public virtual decimal? PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>
    [Required(ErrorMessage = "气压值不能为空")]
    public virtual decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    [Required(ErrorMessage = "产品类型不能为空")]
    public virtual long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>
    [Required(ErrorMessage = "产品状态不能为空")]
    public virtual long? ProductStatusId { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>
    [Required(ErrorMessage = "零件状态不能为空")]
    public virtual long? PartStatusId { get; set; }
    
    /// <summary>
    /// Ng位置
    /// </summary>
    public virtual long? NgPositionId { get; set; }
    
    /// <summary>
    /// 二维码
    /// </summary>
    public virtual string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    public virtual string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>
    [Required(ErrorMessage = "检测结果不能为空")]
    public virtual string TestResult { get; set; }
    
    /// <summary>
    /// 泄露值
    /// </summary>
    public virtual decimal? Leakage { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>
    public virtual string? Images { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public virtual long? UserId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remarks { get; set; }
    
}

/// <summary>
/// 检测记录分页查询输入参数
/// </summary>
public class PageLkInspectionRecordInput : BasePageInput
{
    /// <summary>
    /// 操作员
    /// </summary>
    public string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>
    public string Date { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    public long? ShiftId { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>
    public decimal? PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>
    public long? ProductStatusId { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>
    public long? PartStatusId { get; set; }
    
    /// <summary>
    /// Ng位置
    /// </summary>
    public long? NgPositionId { get; set; }
    
    /// <summary>
    /// 二维码
    /// </summary>
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    public string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>
    public string TestResult { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 检测记录增加输入参数
/// </summary>
public class AddLkInspectionRecordInput
{
    /// <summary>
    /// 操作员
    /// </summary>
    [Required(ErrorMessage = "操作员不能为空")]
    [MaxLength(32, ErrorMessage = "操作员字符长度不能超过32")]
    public string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>
    [Required(ErrorMessage = "检测日期不能为空")]
    [MaxLength(16, ErrorMessage = "检测日期字符长度不能超过16")]
    public string Date { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    [Required(ErrorMessage = "班次不能为空")]
    public long? ShiftId { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>
    [Required(ErrorMessage = "保压时间不能为空")]
    public decimal? PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>
    [Required(ErrorMessage = "气压值不能为空")]
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    [Required(ErrorMessage = "产品类型不能为空")]
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>
    [Required(ErrorMessage = "产品状态不能为空")]
    public long? ProductStatusId { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>
    [Required(ErrorMessage = "零件状态不能为空")]
    public long? PartStatusId { get; set; }
    
    /// <summary>
    /// Ng位置
    /// </summary>
    public long? NgPositionId { get; set; }
    
    /// <summary>
    /// 二维码
    /// </summary>
    [MaxLength(64, ErrorMessage = "二维码字符长度不能超过64")]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    [MaxLength(64, ErrorMessage = "钢印号字符长度不能超过64")]
    public string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>
    [Required(ErrorMessage = "检测结果不能为空")]
    [MaxLength(8, ErrorMessage = "检测结果字符长度不能超过8")]
    public string TestResult { get; set; }
    
    /// <summary>
    /// 泄露值
    /// </summary>
    public decimal? Leakage { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(512, ErrorMessage = "备注字符长度不能超过512")]
    public string? Remarks { get; set; }
    
}

/// <summary>
/// 检测记录删除输入参数
/// </summary>
public class DeleteLkInspectionRecordInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 检测记录更新输入参数
/// </summary>
public class UpdateLkInspectionRecordInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 操作员
    /// </summary>    
    [Required(ErrorMessage = "操作员不能为空")]
    [MaxLength(32, ErrorMessage = "操作员字符长度不能超过32")]
    public string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>    
    [Required(ErrorMessage = "检测日期不能为空")]
    [MaxLength(16, ErrorMessage = "检测日期字符长度不能超过16")]
    public string Date { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>    
    [Required(ErrorMessage = "班次不能为空")]
    public long? ShiftId { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>    
    [Required(ErrorMessage = "保压时间不能为空")]
    public decimal? PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>    
    [Required(ErrorMessage = "气压值不能为空")]
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>    
    [Required(ErrorMessage = "产品类型不能为空")]
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>    
    [Required(ErrorMessage = "产品状态不能为空")]
    public long? ProductStatusId { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>    
    [Required(ErrorMessage = "零件状态不能为空")]
    public long? PartStatusId { get; set; }
    
    /// <summary>
    /// Ng位置
    /// </summary>    
    public long? NgPositionId { get; set; }
    
    /// <summary>
    /// 二维码
    /// </summary>    
    [MaxLength(64, ErrorMessage = "二维码字符长度不能超过64")]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>    
    [MaxLength(64, ErrorMessage = "钢印号字符长度不能超过64")]
    public string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>    
    [Required(ErrorMessage = "检测结果不能为空")]
    [MaxLength(8, ErrorMessage = "检测结果字符长度不能超过8")]
    public string TestResult { get; set; }
    
    /// <summary>
    /// 泄露值
    /// </summary>    
    public decimal? Leakage { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>    
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>    
    [Required(ErrorMessage = "用户ID不能为空")]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>    
    [MaxLength(512, ErrorMessage = "备注字符长度不能超过512")]
    public string? Remarks { get; set; }
    
}

/// <summary>
/// 检测记录主键查询输入参数
/// </summary>
public class QueryByIdLkInspectionRecordInput : DeleteLkInspectionRecordInput
{
}

/// <summary>
/// 下拉数据输入参数
/// </summary>
public class DropdownDataLkInspectionRecordInput
{
    /// <summary>
    /// 是否用于分页查询
    /// </summary>
    public bool FromPage { get; set; }
}

/// <summary>
/// 检测记录数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkInspectionRecordInput : BaseImportInput
{
    /// <summary>
    /// 操作员
    /// </summary>
    [ImporterHeader(Name = "*操作员")]
    [ExporterHeader("*操作员", Format = "", Width = 25, IsBold = true)]
    public string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>
    [ImporterHeader(Name = "*检测日期")]
    [ExporterHeader("*检测日期", Format = "", Width = 25, IsBold = true)]
    public string Date { get; set; }
    
    /// <summary>
    /// 班次 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ShiftId { get; set; }
    
    /// <summary>
    /// 班次 文本
    /// </summary>
    [ImporterHeader(Name = "*班次")]
    [ExporterHeader("*班次", Format = "", Width = 25, IsBold = true)]
    public string ShiftFkDisplayName { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>
    [ImporterHeader(Name = "*保压时间")]
    [ExporterHeader("*保压时间", Format = "", Width = 25, IsBold = true)]
    public decimal? PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>
    [ImporterHeader(Name = "*气压值")]
    [ExporterHeader("*气压值", Format = "", Width = 25, IsBold = true)]
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品类型 文本
    /// </summary>
    [ImporterHeader(Name = "*产品类型")]
    [ExporterHeader("*产品类型", Format = "", Width = 25, IsBold = true)]
    public string ProductTypeFkDisplayName { get; set; }
    
    /// <summary>
    /// 产品状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? ProductStatusId { get; set; }
    
    /// <summary>
    /// 产品状态 文本
    /// </summary>
    [ImporterHeader(Name = "*产品状态")]
    [ExporterHeader("*产品状态", Format = "", Width = 25, IsBold = true)]
    public string ProductStatusFkDisplayName { get; set; }
    
    /// <summary>
    /// 零件状态 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? PartStatusId { get; set; }
    
    /// <summary>
    /// 零件状态 文本
    /// </summary>
    [ImporterHeader(Name = "*零件状态")]
    [ExporterHeader("*零件状态", Format = "", Width = 25, IsBold = true)]
    public string PartStatusFkDisplayName { get; set; }
    
    /// <summary>
    /// Ng位置 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? NgPositionId { get; set; }
    
    /// <summary>
    /// Ng位置 文本
    /// </summary>
    [ImporterHeader(Name = "Ng位置")]
    [ExporterHeader("Ng位置", Format = "", Width = 25, IsBold = true)]
    public string NgPositionFkDisplayName { get; set; }
    
    /// <summary>
    /// 二维码
    /// </summary>
    [ImporterHeader(Name = "二维码")]
    [ExporterHeader("二维码", Format = "", Width = 25, IsBold = true)]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    [ImporterHeader(Name = "钢印号")]
    [ExporterHeader("钢印号", Format = "", Width = 25, IsBold = true)]
    public string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>
    [ImporterHeader(Name = "*检测结果")]
    [ExporterHeader("*检测结果", Format = "", Width = 25, IsBold = true)]
    public string TestResult { get; set; }
    
    /// <summary>
    /// 泄露值
    /// </summary>
    [ImporterHeader(Name = "泄露值")]
    [ExporterHeader("泄露值", Format = "", Width = 25, IsBold = true)]
    public decimal? Leakage { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>
    [ImporterHeader(Name = "图片URL列表")]
    [ExporterHeader("图片URL列表", Format = "", Width = 25, IsBold = true)]
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户ID 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 用户ID 文本
    /// </summary>
    [ImporterHeader(Name = "*用户ID")]
    [ExporterHeader("*用户ID", Format = "", Width = 25, IsBold = true)]
    public string UserFkDisplayName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remarks { get; set; }
    
}
