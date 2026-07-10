// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using SqlSugar;
using System.ComponentModel;
using Admin.NET.LvKong.Application.Const;
using Admin.NET.LvKong.Application.Entity;
using System.Globalization;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// 检测统计服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 90)]
public class LkStatisticsService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkInspectionRecord> _lkInspectionRecordRep;

    public LkStatisticsService(SqlSugarRepository<LkInspectionRecord> lkInspectionRecordRep)
    {
        _lkInspectionRecordRep = lkInspectionRecordRep;
    }

    /// <summary>
    /// 统计用户今日检测情况（总数、OK 数、NG 数）🔖
    /// <remarks>
    /// 统计天以当日 08:30 为起点、次日 08:30 为终点（即一个自然工作日跨越两个日历日）。
    /// 使用 Time 字段（格式 HH:mm:ss）与 Date 字段组合判断记录是否属于当前统计天。
    /// </remarks>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("统计用户今日检测情况")]
    [ApiDescriptionSettings(Name = "UserTodayStat"), HttpGet]
    public async Task<LkUserInspectionStatOutput> GetUserTodayStat([FromQuery] LkUserTodayStatInput input)
    {
        // 计算当前统计天的起止边界
        // 规则：每天 08:30 开始，到次日 08:30 结束
        var now = DateTime.Now;
        var shiftStart = now.Date.AddHours(8).AddMinutes(30); // 今日 08:30
        DateTime statDayStart, statDayEnd;

        if (now >= shiftStart)
        {
            // 当前时间在 08:30 之后 → 统计天从今日 08:30 到明日 08:30
            statDayStart = shiftStart;
            statDayEnd   = shiftStart.AddDays(1);
        }
        else
        {
            // 当前时间在 08:30 之前 → 统计天从昨日 08:30 到今日 08:30
            statDayStart = shiftStart.AddDays(-1);
            statDayEnd   = shiftStart;
        }

        // 统计天覆盖的日历日期字符串（可能跨两天）
        var dateStart = statDayStart.ToString("yyyy-MM-dd");
        var dateEnd   = statDayEnd.ToString("yyyy-MM-dd");
        var cutTime   = "08:30:00"; // 每天的分界时间

        // 拉取可能属于本统计天的记录（Date 在 dateStart 或 dateEnd 当天）
        var candidates = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.UserId == input.UserId &&
                        (u.Date.StartsWith(dateStart) || u.Date.StartsWith(dateEnd)))
            .Select(u => new { u.Date, u.Time, u.TestResult })
            .ToListAsync();

        // 在内存中按 08:30 边界精确过滤
        // dateStart 当天：Time >= 08:30:00
        // dateEnd 当天（若跨天）：Time < 08:30:00
        var records = candidates.Where(r =>
        {
            var dateStr = r.Date?.Length >= 10 ? r.Date[..10] : r.Date ?? "";
            var timeStr = r.Time ?? "00:00:00";
            if (dateStr == dateStart)
                return string.Compare(timeStr, cutTime, StringComparison.Ordinal) >= 0;
            if (dateStr == dateEnd)
                return string.Compare(timeStr, cutTime, StringComparison.Ordinal) < 0;
            return false;
        }).ToList();

        var userName = await _lkInspectionRecordRep.Context
            .Queryable<SysUser>()
            .Where(u => u.Id == input.UserId)
            .Select(u => u.RealName)
            .FirstAsync() ?? string.Empty;

        return new LkUserInspectionStatOutput
        {
            UserId   = input.UserId,
            UserName = userName,
            Date     = dateStart, // 统计天以起始日期标识
            Total    = records.Count,
            OkCount  = records.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase)),
            NgCount  = records.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase)),
        };
    }

    /// <summary>
    /// 统计用户全量检测情况（总数、OK 数、NG 数）🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("统计用户全量检测情况")]
    [ApiDescriptionSettings(Name = "UserTotalStat"), HttpGet]
    public async Task<LkUserInspectionStatOutput> GetUserTotalStat([FromQuery] LkUserTotalStatInput input)
    {
        var records = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.UserId == input.UserId)
            .Select(u => new { u.TestResult })
            .ToListAsync();

        var userName = await _lkInspectionRecordRep.Context
            .Queryable<SysUser>()
            .Where(u => u.Id == input.UserId)
            .Select(u => u.RealName)
            .FirstAsync() ?? string.Empty;

        return new LkUserInspectionStatOutput
        {
            UserId   = input.UserId,
            UserName = userName,
            Date     = null,
            Total    = records.Count,
            OkCount  = records.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase)),
            NgCount  = records.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase)),
        };
    }

    /// <summary>
    /// 按产品类型统计指定年月的检测总数、OK 数、NG 数及合格率 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("按产品类型统计月度检测情况")]
    [ApiDescriptionSettings(Name = "MonthlyProductTypeStat"), HttpGet]
    public async Task<List<LkProductTypeMonthlyStatOutput>> GetMonthlyProductTypeStat([FromQuery] LkMonthlyProductTypeStatInput input)
    {
        // 构造月份前缀，例如 "2025-06"，与 Date 字段 "yyyy-MM-dd" 格式匹配
        var monthPrefix = $"{input.Year:D4}-{input.Month:D2}";

        // 查询该月所有记录，仅取需要的字段
        var records = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.Date.StartsWith(monthPrefix))
            .LeftJoin<LkProductType>((u, pt) => u.ProductTypeId == pt.Id)
            .Select((u, pt) => new
            {
                u.ProductTypeId,
                ProductTypeName = pt.Name,
                u.TestResult,
            })
            .ToListAsync();

        // 按产品类型分组汇总
        var result = records
            .GroupBy(r => new { r.ProductTypeId, r.ProductTypeName })
            .Select(g =>
            {
                var total   = g.Count();
                var okCount = g.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase));
                var ngCount = g.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase));
                return new LkProductTypeMonthlyStatOutput
                {
                    ProductTypeId   = g.Key.ProductTypeId,
                    ProductTypeName = g.Key.ProductTypeName ?? string.Empty,
                    YearMonth       = monthPrefix,
                    Total           = total,
                    OkCount         = okCount,
                    NgCount         = ngCount,
                    PassRate        = total == 0 ? 0m : Math.Round((decimal)okCount / total, 4),
                };
            })
            .OrderBy(x => x.ProductTypeName)
            .ToList();

        return result;
    }

    /// <summary>
    /// 按人员统计指定年月的检测总数、OK 数、NG 数及合格率 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("按人员统计月度检测情况")]
    [ApiDescriptionSettings(Name = "MonthlyUserStat"), HttpGet]
    public async Task<List<LkUserMonthlyStatOutput>> GetMonthlyUserStat([FromQuery] LkMonthlyUserStatInput input)
    {
        var monthPrefix = $"{input.Year:D4}-{input.Month:D2}";

        // 直接按自然月过滤，Date 字段格式为 yyyy-MM-dd，StartsWith 即可精确匹配
        var records = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.Date.StartsWith(monthPrefix))
            .LeftJoin<SysUser>((u, user) => u.UserId == user.Id)
            .Select((u, user) => new
            {
                u.UserId,
                UserName = user.RealName,
                u.TestResult,
            })
            .ToListAsync();

        var result = records
            .GroupBy(r => new { r.UserId, r.UserName })
            .Select(g =>
            {
                var total   = g.Count();
                var okCount = g.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase));
                var ngCount = g.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase));
                return new LkUserMonthlyStatOutput
                {
                    UserId    = g.Key.UserId,
                    UserName  = g.Key.UserName ?? string.Empty,
                    YearMonth = monthPrefix,
                    Total     = total,
                    OkCount   = okCount,
                    NgCount   = ngCount,
                    PassRate  = total == 0 ? 0m : Math.Round((decimal)okCount / total, 4),
                };
            })
            .OrderBy(x => x.UserName)
            .ToList();

        return result;
    }
}
