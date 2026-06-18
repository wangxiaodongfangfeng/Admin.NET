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
/// 产品类型基础输入参数
/// </summary>
public class LkProductTypeBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 产品类型名称
    /// </summary>
    [Required(ErrorMessage = "产品类型名称不能为空")]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>
    [Required(ErrorMessage = "是否默认不能为空")]
    public virtual bool? IsDefault { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }
    
}

/// <summary>
/// 产品类型分页查询输入参数
/// </summary>
public class PageLkProductTypeInput : BasePageInput
{
    /// <summary>
    /// 产品类型名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>
    public bool? IsDefault { get; set; }
    
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
/// 产品类型增加输入参数
/// </summary>
public class AddLkProductTypeInput
{
    /// <summary>
    /// 产品类型名称
    /// </summary>
    [Required(ErrorMessage = "产品类型名称不能为空")]
    [MaxLength(64, ErrorMessage = "产品类型名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>
    [Required(ErrorMessage = "是否默认不能为空")]
    public bool? IsDefault { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(256, ErrorMessage = "描述字符长度不能超过256")]
    public string? Description { get; set; }
    
}

/// <summary>
/// 产品类型删除输入参数
/// </summary>
public class DeleteLkProductTypeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 产品类型更新输入参数
/// </summary>
public class UpdateLkProductTypeInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 产品类型名称
    /// </summary>    
    [Required(ErrorMessage = "产品类型名称不能为空")]
    [MaxLength(64, ErrorMessage = "产品类型名称字符长度不能超过64")]
    public string Name { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>    
    [Required(ErrorMessage = "是否默认不能为空")]
    public bool? IsDefault { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>    
    [MaxLength(256, ErrorMessage = "描述字符长度不能超过256")]
    public string? Description { get; set; }
    
}

/// <summary>
/// 产品类型主键查询输入参数
/// </summary>
public class QueryByIdLkProductTypeInput : DeleteLkProductTypeInput
{
}

/// <summary>
/// 产品类型数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkProductTypeInput : BaseImportInput
{
    /// <summary>
    /// 产品类型名称
    /// </summary>
    [ImporterHeader(Name = "*产品类型名称")]
    [ExporterHeader("*产品类型名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; }
    
    /// <summary>
    /// 是否默认
    /// </summary>
    [ImporterHeader(Name = "*是否默认")]
    [ExporterHeader("*是否默认", Format = "", Width = 25, IsBold = true)]
    public bool? IsDefault { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    [ImporterHeader(Name = "描述")]
    [ExporterHeader("描述", Format = "", Width = 25, IsBold = true)]
    public string? Description { get; set; }
    
}
