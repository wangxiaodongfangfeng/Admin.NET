// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 检测记录输出参数
/// </summary>
public class LkInspectionRecordDto
{
    /// <summary>
    /// 班次
    /// </summary>
    public string ShiftIdFkColumn { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    public string ProductTypeIdFkColumn { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>
    public string ProductStatusIdFkColumn { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>
    public string PartStatusIdFkColumn { get; set; }
    
    /// <summary>
    /// Ng位置
    /// </summary>
    public string NgPositionIdFkColumn { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    public string UserIdFkColumn { get; set; }
    
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 操作员
    /// </summary>
    public string Operator { get; set; }
    
    /// <summary>
    /// 检测日期
    /// </summary>
    public string Date { get; set; }
    
    /// <summary>
    /// 检测时间
    /// </summary>
    public string Time { get; set; }
    
    /// <summary>
    /// 班次
    /// </summary>
    public long ShiftId { get; set; }
    
    /// <summary>
    /// 保压时间
    /// </summary>
    public decimal PressureHoldTime { get; set; }
    
    /// <summary>
    /// 气压值
    /// </summary>
    public decimal Pressure { get; set; }
    
    /// <summary>
    /// 产品类型
    /// </summary>
    public long ProductTypeId { get; set; }
    
    /// <summary>
    /// 产品状态
    /// </summary>
    public long ProductStatusId { get; set; }
    
    /// <summary>
    /// 零件状态
    /// </summary>
    public long PartStatusId { get; set; }
    
    /// <summary>
    /// 二码码
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
    /// Ng位置
    /// </summary>
    public long NgPositionId { get; set; }
    
    /// <summary>
    /// 泄露值
    /// </summary>
    public decimal Leakage { get; set; }
    
    /// <summary>
    /// 图片URL列表
    /// </summary>
    public string? Images { get; set; }
    
    /// <summary>
    /// 用户
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }
    
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
    
    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }
    
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
    
}
