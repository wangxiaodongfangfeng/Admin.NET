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
/// 班次基础输入参数
/// </summary>
public class LkShiftBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 班次名称
    /// </summary>
    [Required(ErrorMessage = "班次名称不能为空")]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public virtual string? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public virtual string? EndTime { get; set; }
    
}

/// <summary>
/// 班次分页查询输入参数
/// </summary>
public class PageLkShiftInput : BasePageInput
{
    /// <summary>
    /// 班次名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public string? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public string? EndTime { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 班次增加输入参数
/// </summary>
public class AddLkShiftInput
{
    /// <summary>
    /// 班次名称
    /// </summary>
    [Required(ErrorMessage = "班次名称不能为空")]
    [MaxLength(64, ErrorMessage = "班次名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    [MaxLength(8, ErrorMessage = "开始时间字符长度不能超过8")]
    public string? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    [MaxLength(8, ErrorMessage = "结束时间字符长度不能超过8")]
    public string? EndTime { get; set; }
    
}

/// <summary>
/// 班次删除输入参数
/// </summary>
public class DeleteLkShiftInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 班次更新输入参数
/// </summary>
public class UpdateLkShiftInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 班次名称
    /// </summary>    
    [Required(ErrorMessage = "班次名称不能为空")]
    [MaxLength(64, ErrorMessage = "班次名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>    
    [MaxLength(8, ErrorMessage = "开始时间字符长度不能超过8")]
    public string? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>    
    [MaxLength(8, ErrorMessage = "结束时间字符长度不能超过8")]
    public string? EndTime { get; set; }
    
}

/// <summary>
/// 班次主键查询输入参数
/// </summary>
public class QueryByIdLkShiftInput : DeleteLkShiftInput
{
}

/// <summary>
/// 班次数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkShiftInput : BaseImportInput
{
    /// <summary>
    /// 班次名称
    /// </summary>
    [ImporterHeader(Name = "*班次名称")]
    [ExporterHeader("*班次名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    [ImporterHeader(Name = "开始时间")]
    [ExporterHeader("开始时间", Format = "", Width = 25, IsBold = true)]
    public string? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    [ImporterHeader(Name = "结束时间")]
    [ExporterHeader("结束时间", Format = "", Width = 25, IsBold = true)]
    public string? EndTime { get; set; }
    
}
