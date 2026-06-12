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
/// 零件状态基础输入参数
/// </summary>
public class LkPartStatusBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 零件状态名称
    /// </summary>
    [Required(ErrorMessage = "零件状态名称不能为空")]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }
    
}

/// <summary>
/// 零件状态分页查询输入参数
/// </summary>
public class PageLkPartStatusInput : BasePageInput
{
    /// <summary>
    /// 零件状态名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 零件状态增加输入参数
/// </summary>
public class AddLkPartStatusInput
{
    /// <summary>
    /// 零件状态名称
    /// </summary>
    [Required(ErrorMessage = "零件状态名称不能为空")]
    [MaxLength(64, ErrorMessage = "零件状态名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(256, ErrorMessage = "描述字符长度不能超过256")]
    public string? Description { get; set; }
    
}

/// <summary>
/// 零件状态删除输入参数
/// </summary>
public class DeleteLkPartStatusInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 零件状态更新输入参数
/// </summary>
public class UpdateLkPartStatusInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 零件状态名称
    /// </summary>    
    [Required(ErrorMessage = "零件状态名称不能为空")]
    [MaxLength(64, ErrorMessage = "零件状态名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>    
    [MaxLength(256, ErrorMessage = "描述字符长度不能超过256")]
    public string? Description { get; set; }
    
}

/// <summary>
/// 零件状态主键查询输入参数
/// </summary>
public class QueryByIdLkPartStatusInput : DeleteLkPartStatusInput
{
}

/// <summary>
/// 零件状态数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkPartStatusInput : BaseImportInput
{
    /// <summary>
    /// 零件状态名称
    /// </summary>
    [ImporterHeader(Name = "*零件状态名称")]
    [ExporterHeader("*零件状态名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    [ImporterHeader(Name = "描述")]
    [ExporterHeader("描述", Format = "", Width = 25, IsBold = true)]
    public string? Description { get; set; }
    
}
