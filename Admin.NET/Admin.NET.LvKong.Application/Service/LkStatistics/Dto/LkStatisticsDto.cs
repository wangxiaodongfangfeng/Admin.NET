// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 用户今日检测统计 - 查询参数
/// </summary>
public class LkUserTodayStatInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
}

/// <summary>
/// 用户全量检测统计 - 查询参数
/// </summary>
public class LkUserTotalStatInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
}

/// <summary>
/// 按产品类型月度检测统计 - 查询参数
/// </summary>
public class LkMonthlyProductTypeStatInput
{
    /// <summary>
    /// 年份，例如 2025
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月份，1-12
    /// </summary>
    public int Month { get; set; }
}

/// <summary>
/// 按人员月度检测统计 - 查询参数
/// </summary>
public class LkMonthlyUserStatInput
{
    /// <summary>
    /// 年份，例如 2025
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月份，1-12
    /// </summary>
    public int Month { get; set; }
}

/// <summary>
/// 单个人员的月度检测统计结果
/// </summary>
public class LkUserMonthlyStatOutput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 统计年月，格式 yyyy-MM
    /// </summary>
    public string YearMonth { get; set; }

    /// <summary>
    /// 检测总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// NG 数量
    /// </summary>
    public int NgCount { get; set; }

    /// <summary>
    /// 合格率（保留4位小数，例如 0.9523 表示 95.23%）
    /// </summary>
    public decimal PassRate { get; set; }
}

/// <summary>
/// 单个产品类型的月度检测统计结果
/// </summary>
public class LkProductTypeMonthlyStatOutput
{
    /// <summary>
    /// 产品类型ID
    /// </summary>
    public long ProductTypeId { get; set; }

    /// <summary>
    /// 产品类型名称
    /// </summary>
    public string ProductTypeName { get; set; }

    /// <summary>
    /// 统计年月，格式 yyyy-MM
    /// </summary>
    public string YearMonth { get; set; }

    /// <summary>
    /// 检测总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// NG 数量
    /// </summary>
    public int NgCount { get; set; }

    /// <summary>
    /// 合格率（保留4位小数，例如 0.9523 表示 95.23%）
    /// </summary>
    public decimal PassRate { get; set; }
}

/// <summary>
/// 按人员每日检测统计 - 查询参数
/// </summary>
public class LkDailyUserStatInput
{
    /// <summary>
    /// 统计天的起始日期，格式 yyyy-MM-dd。
    /// 不传时由服务端按 08:30 规则自动计算当前统计天。
    /// </summary>
    public string? Date { get; set; }
}

/// <summary>
/// 单个人员的每日检测统计结果
/// </summary>
public class LkUserDailyStatOutput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 统计天起始日期，格式 yyyy-MM-dd
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// 检测总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// NG 数量
    /// </summary>
    public int NgCount { get; set; }

    /// <summary>
    /// 合格率（保留4位小数，例如 0.9523 表示 95.23%）
    /// </summary>
    public decimal PassRate { get; set; }
}

/// <summary>
/// 按产品类型每日检测统计 - 查询参数
/// </summary>
public class LkDailyProductTypeStatInput
{
    /// <summary>
    /// 统计天的起始日期，格式 yyyy-MM-dd。
    /// 不传时由服务端按 08:30 规则自动计算当前统计天。
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// 产品状态ID，不传时使用数据库中标记为默认（IsDefault=true）的产品状态。
    /// </summary>
    public long? ProductStatusId { get; set; }
}

/// <summary>
/// 单个产品类型的每日检测统计结果
/// </summary>
public class LkProductTypeDailyStatOutput
{
    /// <summary>
    /// 产品类型ID
    /// </summary>
    public long ProductTypeId { get; set; }

    /// <summary>
    /// 产品类型名称
    /// </summary>
    public string ProductTypeName { get; set; }

    /// <summary>
    /// 统计天起始日期，格式 yyyy-MM-dd
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// 产品状态ID
    /// </summary>
    public long ProductStatusId { get; set; }

    /// <summary>
    /// 产品状态名称
    /// </summary>
    public string ProductStatusName { get; set; }

    /// <summary>
    /// 检测总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// NG 数量
    /// </summary>
    public int NgCount { get; set; }

    /// <summary>
    /// 合格率（保留4位小数，例如 0.9523 表示 95.23%）
    /// </summary>
    public decimal PassRate { get; set; }
}

/// <summary>
/// 用户检测统计结果
/// </summary>
public class LkUserInspectionStatOutput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 统计日期（今日统计时有值，全量统计时为空）
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// 检测总次数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// OK 数量
    /// </summary>
    public int OkCount { get; set; }

    /// <summary>
    /// NG 数量
    /// </summary>
    public int NgCount { get; set; }
}
