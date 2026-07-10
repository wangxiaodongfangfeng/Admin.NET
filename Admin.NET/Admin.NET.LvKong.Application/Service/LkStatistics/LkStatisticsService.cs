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
        var (dateStart, dateEnd) = ResolveDayBoundary(null);
        var cutTime = "08:30:00";

        var candidates = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.UserId == input.UserId &&
                        (u.Date.StartsWith(dateStart) || u.Date.StartsWith(dateEnd)))
            .Select(u => new { u.Date, u.Time, u.TestResult })
            .ToListAsync();

        var records = candidates.Where(r => IsInStatDay(r.Date, r.Time, dateStart, dateEnd, cutTime)).ToList();

        var userName = await _lkInspectionRecordRep.Context
            .Queryable<SysUser>()
            .Where(u => u.Id == input.UserId)
            .Select(u => u.RealName)
            .FirstAsync() ?? string.Empty;

        return new LkUserInspectionStatOutput
        {
            UserId   = input.UserId,
            UserName = userName,
            Date     = dateStart,
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

    /// <summary>
    /// 按人员统计每日检测汇总（总数、OK 数、NG 数、合格率）🔖
    /// <remarks>
    /// 统计天以 08:30 为起点、次日 08:30 为终点。
    /// Date 参数不传时，服务端根据当前时间自动计算当前统计天的起始日期。
    /// </remarks>
    /// </summary>
    [DisplayName("按人员统计每日检测情况")]
    [ApiDescriptionSettings(Name = "DailyUserStat"), HttpGet]
    public async Task<List<LkUserDailyStatOutput>> GetDailyUserStat([FromQuery] LkDailyUserStatInput input)
    {
        var (dateStart, dateEnd) = ResolveDayBoundary(input.Date);
        var cutTime = "08:30:00";

        var candidates = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.Date.CompareTo(dateStart) >= 0 && u.Date.CompareTo(dateEnd) <= 0)
            .LeftJoin<SysUser>((u, user) => u.UserId == user.Id)
            .Select((u, user) => new
            {
                u.UserId,
                UserName = user.RealName,
                u.Date,
                u.Time,
                u.TestResult,
            })
            .ToListAsync();

        var records = candidates.Where(r => IsInStatDay(r.Date, r.Time, dateStart, dateEnd, cutTime)).ToList();

        return records
            .GroupBy(r => new { r.UserId, r.UserName })
            .Select(g =>
            {
                var total   = g.Count();
                var okCount = g.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase));
                var ngCount = g.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase));
                return new LkUserDailyStatOutput
                {
                    UserId   = g.Key.UserId,
                    UserName = g.Key.UserName ?? string.Empty,
                    Date     = dateStart,
                    Total    = total,
                    OkCount  = okCount,
                    NgCount  = ngCount,
                    PassRate = total == 0 ? 0m : Math.Round((decimal)okCount / total, 4),
                };
            })
            .OrderBy(x => x.UserName)
            .ToList();
    }

    /// <summary>
    /// 按产品类型统计每日检测汇总（总数、OK 数、NG 数、合格率）🔖
    /// <remarks>
    /// 统计天以 08:30 为起点、次日 08:30 为终点。
    /// Date 不传时自动计算当前统计天；ProductStatusId 不传时使用 IsDefault=true 的产品状态。
    /// </remarks>
    /// </summary>
    [DisplayName("按产品类型统计每日检测情况")]
    [ApiDescriptionSettings(Name = "DailyProductTypeStat"), HttpGet]
    public async Task<List<LkProductTypeDailyStatOutput>> GetDailyProductTypeStat([FromQuery] LkDailyProductTypeStatInput input)
    {
        var (dateStart, dateEnd) = ResolveDayBoundary(input.Date);
        var cutTime = "08:30:00";

        // 解析产品状态：未传则取默认项
        long productStatusId;
        string productStatusName;
        if (input.ProductStatusId.HasValue)
        {
            var ps = await _lkInspectionRecordRep.Context
                .Queryable<LkProductStatus>()
                .Where(s => s.Id == input.ProductStatusId.Value)
                .Select(s => new { s.Id, s.Name })
                .FirstAsync();
            productStatusId   = ps?.Id ?? 0;
            productStatusName = ps?.Name ?? string.Empty;
        }
        else
        {
            var ps = await _lkInspectionRecordRep.Context
                .Queryable<LkProductStatus>()
                .Where(s => s.IsDefault)
                .Select(s => new { s.Id, s.Name })
                .FirstAsync();
            productStatusId   = ps?.Id ?? 0;
            productStatusName = ps?.Name ?? string.Empty;
        }

        var candidates = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.Date.CompareTo(dateStart) >= 0 && u.Date.CompareTo(dateEnd) <= 0
                     && u.ProductStatusId == productStatusId)
            .LeftJoin<LkProductType>((u, pt) => u.ProductTypeId == pt.Id)
            .Select((u, pt) => new
            {
                u.ProductTypeId,
                ProductTypeName = pt.Name,
                u.Date,
                u.Time,
                u.TestResult,
            })
            .ToListAsync();

        var records = candidates.Where(r => IsInStatDay(r.Date, r.Time, dateStart, dateEnd, cutTime)).ToList();

        return records
            .GroupBy(r => new { r.ProductTypeId, r.ProductTypeName })
            .Select(g =>
            {
                var total   = g.Count();
                var okCount = g.Count(r => r.TestResult.Equals("OK", StringComparison.OrdinalIgnoreCase));
                var ngCount = g.Count(r => r.TestResult.Equals("NG", StringComparison.OrdinalIgnoreCase));
                return new LkProductTypeDailyStatOutput
                {
                    ProductTypeId     = g.Key.ProductTypeId,
                    ProductTypeName   = g.Key.ProductTypeName ?? string.Empty,
                    Date              = dateStart,
                    ProductStatusId   = productStatusId,
                    ProductStatusName = productStatusName,
                    Total             = total,
                    OkCount           = okCount,
                    NgCount           = ngCount,
                    PassRate          = total == 0 ? 0m : Math.Round((decimal)okCount / total, 4),
                };
            })
            .OrderBy(x => x.ProductTypeName)
            .ToList();
    }

    // ── 私有辅助 ──────────────────────────────────────────────────────────────

    /// <summary>
    /// 根据传入日期（或当前时间）计算统计天的起止日历日期字符串。
    /// 规则：起始日期当天 08:30 ~ 次日 08:30。
    /// </summary>
    private static (string dateStart, string dateEnd) ResolveDayBoundary(string? inputDate)
    {
        DateTime statDayStart;

        if (!string.IsNullOrWhiteSpace(inputDate) && DateTime.TryParse(inputDate, out var parsed))
        {
            // 调用方指定了具体日期，直接以该日期作为统计天起点
            statDayStart = parsed.Date;
        }
        else
        {
            // 未指定日期，按当前时间和 08:30 分界自动计算
            var now        = DateTime.Now;
            var shiftStart = now.Date.AddHours(8).AddMinutes(30);
            statDayStart   = now >= shiftStart ? now.Date : now.Date.AddDays(-1);
        }

        return (
            statDayStart.ToString("yyyy-MM-dd"),
            statDayStart.AddDays(1).ToString("yyyy-MM-dd")
        );
    }

    /// <summary>
    /// 判断一条记录是否属于指定统计天（dateStart 08:30 ~ dateEnd 08:30）。
    /// </summary>
    private static bool IsInStatDay(string? date, string? time, string dateStart, string dateEnd, string cutTime)
    {
        var dateStr = date?.Length >= 10 ? date[..10] : date ?? "";
        var timeStr = time ?? "00:00:00";

        if (dateStr == dateStart)
            return string.Compare(timeStr, cutTime, StringComparison.Ordinal) >= 0;
        if (dateStr == dateEnd)
            return string.Compare(timeStr, cutTime, StringComparison.Ordinal) < 0;
        return false;
    }
}
