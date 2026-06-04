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
/// NG位置详情基础输入参数
/// </summary>
public class LkInspectionNgPositionBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }
    
    /// <summary>
    /// 检测记录ID
    /// </summary>
    [Required(ErrorMessage = "检测记录ID不能为空")]
    public virtual long? InspectionId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>
    [Required(ErrorMessage = "NG位置ID不能为空")]
    public virtual long? PositionId { get; set; }
    
    /// <summary>
    /// 图片URL
    /// </summary>
    public virtual string? ImageUrl { get; set; }
    
    /// <summary>
    /// 泄露程度ID
    /// </summary>
    public virtual long? LeakageSeverityId { get; set; }
    
}

/// <summary>
/// NG位置详情分页查询输入参数
/// </summary>
public class PageLkInspectionNgPositionInput : BasePageInput
{
    /// <summary>
    /// 检测记录ID
    /// </summary>
    public long? InspectionId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>
    public long? PositionId { get; set; }
    
    /// <summary>
    /// 图片URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// 泄露程度ID
    /// </summary>
    public long? LeakageSeverityId { get; set; }
    
    /// <summary>
    /// 选中主键列表
    /// </summary>
     public List<long> SelectKeyList { get; set; }
}

/// <summary>
/// NG位置详情增加输入参数
/// </summary>
public class AddLkInspectionNgPositionInput
{
    /// <summary>
    /// 检测记录ID
    /// </summary>
    [Required(ErrorMessage = "检测记录ID不能为空")]
    public long? InspectionId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>
    [Required(ErrorMessage = "NG位置ID不能为空")]
    public long? PositionId { get; set; }
    
    /// <summary>
    /// 图片URL
    /// </summary>
    [MaxLength(256, ErrorMessage = "图片URL字符长度不能超过256")]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// 泄露程度ID
    /// </summary>
    public long? LeakageSeverityId { get; set; }
    
}

/// <summary>
/// NG位置详情删除输入参数
/// </summary>
public class DeleteLkInspectionNgPositionInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
}

/// <summary>
/// NG位置详情更新输入参数
/// </summary>
public class UpdateLkInspectionNgPositionInput
{
    /// <summary>
    /// 主键Id
    /// </summary>    
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
    
    /// <summary>
    /// 检测记录ID
    /// </summary>    
    [Required(ErrorMessage = "检测记录ID不能为空")]
    public long? InspectionId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>    
    [Required(ErrorMessage = "NG位置ID不能为空")]
    public long? PositionId { get; set; }
    
    /// <summary>
    /// 图片URL
    /// </summary>    
    [MaxLength(256, ErrorMessage = "图片URL字符长度不能超过256")]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// 泄露程度ID
    /// </summary>    
    public long? LeakageSeverityId { get; set; }
    
}

/// <summary>
/// NG位置详情主键查询输入参数
/// </summary>
public class QueryByIdLkInspectionNgPositionInput : DeleteLkInspectionNgPositionInput
{
}

/// <summary>
/// NG位置详情数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportLkInspectionNgPositionInput : BaseImportInput
{
    /// <summary>
    /// 检测记录ID
    /// </summary>
    [ImporterHeader(Name = "*检测记录ID")]
    [ExporterHeader("*检测记录ID", Format = "", Width = 25, IsBold = true)]
    public long? InspectionId { get; set; }
    
    /// <summary>
    /// NG位置ID
    /// </summary>
    [ImporterHeader(Name = "*NG位置ID")]
    [ExporterHeader("*NG位置ID", Format = "", Width = 25, IsBold = true)]
    public long? PositionId { get; set; }
    
    /// <summary>
    /// 图片URL
    /// </summary>
    [ImporterHeader(Name = "图片URL")]
    [ExporterHeader("图片URL", Format = "", Width = 25, IsBold = true)]
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// 泄露程度ID
    /// </summary>
    [ImporterHeader(Name = "泄露程度ID")]
    [ExporterHeader("泄露程度ID", Format = "", Width = 25, IsBold = true)]
    public long? LeakageSeverityId { get; set; }
    
}
