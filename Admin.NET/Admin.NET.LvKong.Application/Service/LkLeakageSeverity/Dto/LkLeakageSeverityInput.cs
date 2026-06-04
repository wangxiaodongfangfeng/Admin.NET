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
/// 泄露程度基础输入参数
/// </summary>
public class LkLeakageSeverityBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 泄露程度名称
    /// </summary>
    [Required(ErrorMessage = "泄露程度名称不能为空")]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 显示颜色
    /// </summary>
    [Required(ErrorMessage = "显示颜色不能为空")]
    public virtual string Color { get; set; }
    
    /// <summary>
    /// 严重等级
    /// </summary>
    [Required(ErrorMessage = "严重等级不能为空")]
    public virtual int? Level { get; set; }
    
}

/// <summary>
/// 泄露程度分页查询输入参数
/// </summary>
public class PageLkLeakageSeverityInput : BasePageInput
{
    /// <summary>
    /// 泄露程度名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 显示颜色
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// 严重等级
    /// </summary>
    public int? Level { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// 泄露程度增加输入参数
/// </summary>
public class AddLkLeakageSeverityInput
{
    /// <summary>
    /// 泄露程度名称
    /// </summary>
    [Required(ErrorMessage = "泄露程度名称不能为空")]
    [MaxLength(32, ErrorMessage = "泄露程度名称字符长度不能超过32")]
    public string Name { get; set; }
    
    /// <summary>
    /// 显示颜色
    /// </summary>
    [Required(ErrorMessage = "显示颜色不能为空")]
    [MaxLength(16, ErrorMessage = "显示颜色字符长度不能超过16")]
    public string Color { get; set; }
    
    /// <summary>
    /// 严重等级
    /// </summary>
    [Required(ErrorMessage = "严重等级不能为空")]
    public int? Level { get; set; }
    
}

/// <summary>
/// 泄露程度删除输入参数
/// </summary>
public class DeleteLkLeakageSeverityInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// 泄露程度更新输入参数
/// </summary>
public class UpdateLkLeakageSeverityInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 泄露程度名称
    /// </summary>    
    [Required(ErrorMessage = "泄露程度名称不能为空")]
    [MaxLength(32, ErrorMessage = "泄露程度名称字符长度不能超过32")]
    public string Name { get; set; }
    
    /// <summary>
    /// 显示颜色
    /// </summary>    
    [Required(ErrorMessage = "显示颜色不能为空")]
    [MaxLength(16, ErrorMessage = "显示颜色字符长度不能超过16")]
    public string Color { get; set; }
    
    /// <summary>
    /// 严重等级
    /// </summary>    
    [Required(ErrorMessage = "严重等级不能为空")]
    public int? Level { get; set; }
    
}

/// <summary>
/// 泄露程度主键查询输入参数
/// </summary>
public class QueryByIdLkLeakageSeverityInput : DeleteLkLeakageSeverityInput
{
}

/// <summary>
/// 泄露程度数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkLeakageSeverityInput : BaseImportInput
{
    /// <summary>
    /// 泄露程度名称
    /// </summary>
    [ImporterHeader(Name = "*泄露程度名称")]
    [ExporterHeader("*泄露程度名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; }
    
    /// <summary>
    /// 显示颜色
    /// </summary>
    [ImporterHeader(Name = "*显示颜色")]
    [ExporterHeader("*显示颜色", Format = "", Width = 25, IsBold = true)]
    public string Color { get; set; }
    
    /// <summary>
    /// 严重等级
    /// </summary>
    [ImporterHeader(Name = "*严重等级")]
    [ExporterHeader("*严重等级", Format = "", Width = 25, IsBold = true)]
    public int? Level { get; set; }
    
}
