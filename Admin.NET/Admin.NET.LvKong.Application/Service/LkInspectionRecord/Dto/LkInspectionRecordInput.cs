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
    /// 压力值
    /// </summary>
    [Required(ErrorMessage = "压力值不能为空")]
    public virtual decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    [Required(ErrorMessage = "产品类型不能为空")]
    public virtual long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品型号
    /// </summary>
    public virtual string? ProductModel { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    [Required(ErrorMessage = "批次号不能为空")]
    public virtual string BatchNumber { get; set; }
    
    /// <summary>
    /// 规格
    /// </summary>
    [Required(ErrorMessage = "规格不能为空")]
    public virtual string Specification { get; set; }
    
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
    /// 图片
    /// </summary>
    public virtual string? Images { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    [Required(ErrorMessage = "用户不能为空")]
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
    /// 压力值
    /// </summary>
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品型号
    /// </summary>
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    public string BatchNumber { get; set; }
    
    /// <summary>
    /// 规格
    /// </summary>
    public string Specification { get; set; }
    
    /// <summary>
    /// 钢印号
    /// </summary>
    public string? SteelStamp { get; set; }
    
    /// <summary>
    /// 检测结果
    /// </summary>
    public string TestResult { get; set; }
    
    /// <summary>
    /// 用户
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
    /// 压力值
    /// </summary>
    [Required(ErrorMessage = "压力值不能为空")]
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    [Required(ErrorMessage = "产品类型不能为空")]
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品型号
    /// </summary>
    [MaxLength(64, ErrorMessage = "产品型号字符长度不能超过64")]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    [Required(ErrorMessage = "批次号不能为空")]
    [MaxLength(64, ErrorMessage = "批次号字符长度不能超过64")]
    public string BatchNumber { get; set; }
    
    /// <summary>
    /// 规格
    /// </summary>
    [Required(ErrorMessage = "规格不能为空")]
    [MaxLength(64, ErrorMessage = "规格字符长度不能超过64")]
    public string Specification { get; set; }
    
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
    /// 图片
    /// </summary>
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    [Required(ErrorMessage = "用户不能为空")]
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
    /// 压力值
    /// </summary>    
    [Required(ErrorMessage = "压力值不能为空")]
    public decimal? Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>    
    [Required(ErrorMessage = "产品类型不能为空")]
    public long? ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品型号
    /// </summary>    
    [MaxLength(64, ErrorMessage = "产品型号字符长度不能超过64")]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>    
    [Required(ErrorMessage = "批次号不能为空")]
    [MaxLength(64, ErrorMessage = "批次号字符长度不能超过64")]
    public string BatchNumber { get; set; }
    
    /// <summary>
    /// 规格
    /// </summary>    
    [Required(ErrorMessage = "规格不能为空")]
    [MaxLength(64, ErrorMessage = "规格字符长度不能超过64")]
    public string Specification { get; set; }
    
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
    /// 图片
    /// </summary>    
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>    
    [Required(ErrorMessage = "用户不能为空")]
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
    /// 压力值
    /// </summary>
    [ImporterHeader(Name = "*压力值")]
    [ExporterHeader("*压力值", Format = "", Width = 25, IsBold = true)]
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
    /// 产品型号
    /// </summary>
    [ImporterHeader(Name = "产品型号")]
    [ExporterHeader("产品型号", Format = "", Width = 25, IsBold = true)]
    public string? ProductModel { get; set; }
    
    /// <summary>
    /// 批次号
    /// </summary>
    [ImporterHeader(Name = "*批次号")]
    [ExporterHeader("*批次号", Format = "", Width = 25, IsBold = true)]
    public string BatchNumber { get; set; }
    
    /// <summary>
    /// 规格
    /// </summary>
    [ImporterHeader(Name = "*规格")]
    [ExporterHeader("*规格", Format = "", Width = 25, IsBold = true)]
    public string Specification { get; set; }
    
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
    /// 图片
    /// </summary>
    [ImporterHeader(Name = "图片")]
    [ExporterHeader("图片", Format = "", Width = 25, IsBold = true)]
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户 关联值
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public long? UserId { get; set; }
    
    /// <summary>
    /// 用户 文本
    /// </summary>
    [ImporterHeader(Name = "*用户")]
    [ExporterHeader("*用户", Format = "", Width = 25, IsBold = true)]
    public string UserFkDisplayName { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [ImporterHeader(Name = "备注")]
    [ExporterHeader("备注", Format = "", Width = 25, IsBold = true)]
    public string? Remarks { get; set; }
    
}
