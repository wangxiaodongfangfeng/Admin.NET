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
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("统计用户今日检测情况")]
    [ApiDescriptionSettings(Name = "UserTodayStat"), HttpGet]
    public async Task<LkUserInspectionStatOutput> GetUserTodayStat([FromQuery] LkUserTodayStatInput input)
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        var records = await _lkInspectionRecordRep.AsQueryable()
            .Where(u => u.UserId == input.UserId && u.Date.StartsWith(today))
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
            Date     = today,
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
}
