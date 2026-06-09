// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.LvKong.Application.Entity;
using Admin.NET.LvKong.Application.Const;
namespace Admin.NET.LvKong.Application;

/// <summary>
/// NG位置详情服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkInspectionNgPositionService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkInspectionNgPosition> _lkInspectionNgPositionRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkInspectionNgPositionService(SqlSugarRepository<LkInspectionNgPosition> lkInspectionNgPositionRep, ISqlSugarClient sqlSugarClient)
    {
        _lkInspectionNgPositionRep = lkInspectionNgPositionRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询NG位置详情 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询NG位置详情")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkInspectionNgPositionOutput>> Page(PageLkInspectionNgPositionInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkInspectionNgPositionRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ImageUrl.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ImageUrl), u => u.ImageUrl.Contains(input.ImageUrl.Trim()))
            .WhereIF(input.InspectionId != null, u => u.InspectionId == input.InspectionId)
            .WhereIF(input.PositionId != null, u => u.PositionId == input.PositionId)
            .WhereIF(input.LeakageSeverityId != null, u => u.LeakageSeverityId == input.LeakageSeverityId)
            .LeftJoin<LkInspectionRecord>((u, inspection) => u.InspectionId == inspection.Id)
            .LeftJoin<LkNgPosition>((u, inspection, position) => u.PositionId == position.Id)
            .LeftJoin<LkLeakageSeverity>((u, inspection, position, leakageSeverity) => u.LeakageSeverityId == leakageSeverity.Id)
            .Select((u, inspection, position, leakageSeverity) => new LkInspectionNgPositionOutput
            {
                Id = u.Id,
                InspectionId = u.InspectionId,
                InspectionFkDisplayName = $"{inspection.SteelStamp}",
                PositionId = u.PositionId,
                PositionFkDisplayName = $"{position.Name}",
                ImageUrl = u.ImageUrl,
                LeakageSeverityId = u.LeakageSeverityId,
                LeakageSeverityFkDisplayName = $"{leakageSeverity.Name}",
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = u.UpdateUserName,
            });
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取NG位置详情详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取NG位置详情详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkInspectionNgPosition> Detail([FromQuery] QueryByIdLkInspectionNgPositionInput input)
    {
        return await _lkInspectionNgPositionRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加NG位置详情 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加NG位置详情")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkInspectionNgPositionInput input)
    {
        var entity = input.Adapt<LkInspectionNgPosition>();
        return await _lkInspectionNgPositionRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新NG位置详情 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新NG位置详情")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkInspectionNgPositionInput input)
    {
        var entity = input.Adapt<LkInspectionNgPosition>();
        await _lkInspectionNgPositionRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除NG位置详情 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除NG位置详情")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkInspectionNgPositionInput input)
    {
        var entity = await _lkInspectionNgPositionRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkInspectionNgPositionRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除NG位置详情 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除NG位置详情")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkInspectionNgPositionInput> input)
    {
        var exp = Expressionable.Create<LkInspectionNgPosition>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkInspectionNgPositionRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkInspectionNgPositionRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataLkInspectionNgPositionInput input)
    {
        var inspectionIdData = await _lkInspectionNgPositionRep.Context.Queryable<LkInspectionRecord>()
            .InnerJoinIF<LkInspectionNgPosition>(input.FromPage, (u, r) => u.Id == r.InspectionId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.SteelStamp}"
            }).ToListAsync();
        var positionIdData = await _lkInspectionNgPositionRep.Context.Queryable<LkNgPosition>()
            .InnerJoinIF<LkInspectionNgPosition>(input.FromPage, (u, r) => u.Id == r.PositionId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.Name}"
            }).ToListAsync();
        var leakageSeverityIdData = await _lkInspectionNgPositionRep.Context.Queryable<LkLeakageSeverity>()
            .InnerJoinIF<LkInspectionNgPosition>(input.FromPage, (u, r) => u.Id == r.LeakageSeverityId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.Name}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "inspectionId", inspectionIdData },
            { "positionId", positionIdData },
            { "leakageSeverityId", leakageSeverityIdData },
        };
    }
    
    /// <summary>
    /// 导出NG位置详情记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出NG位置详情记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkInspectionNgPositionInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkInspectionNgPositionOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "NG位置详情导出记录");
    }
    
    /// <summary>
    /// 下载NG位置详情数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载NG位置详情数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkInspectionNgPositionOutput>(), "NG位置详情导入模板", (_, info) =>
        {
            if (nameof(ExportLkInspectionNgPositionOutput.InspectionFkDisplayName) == info.Name) return _lkInspectionNgPositionRep.Context.Queryable<LkInspectionRecord>().Select(u => $"{u.SteelStamp}").Distinct().ToList();
            if (nameof(ExportLkInspectionNgPositionOutput.PositionFkDisplayName) == info.Name) return _lkInspectionNgPositionRep.Context.Queryable<LkNgPosition>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionNgPositionOutput.LeakageSeverityFkDisplayName) == info.Name) return _lkInspectionNgPositionRep.Context.Queryable<LkLeakageSeverity>().Select(u => $"{u.Name}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _lkInspectionNgPositionImportLock = new object();
    /// <summary>
    /// 导入NG位置详情记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入NG位置详情记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkInspectionNgPositionImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkInspectionNgPositionInput, LkInspectionNgPosition>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 检测记录
                    var inspectionIdLabelList = pageItems.Where(x => x.InspectionFkDisplayName != null).Select(x => x.InspectionFkDisplayName).Distinct().ToList();
                    if (inspectionIdLabelList.Any()) {
                        var inspectionIdLinkMap = _lkInspectionNgPositionRep.Context.Queryable<LkInspectionRecord>().Where(u => inspectionIdLabelList.Contains($"{u.SteelStamp}")).ToList().ToDictionary(u => $"{u.SteelStamp}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.InspectionId = inspectionIdLinkMap.GetValueOrDefault(e.InspectionFkDisplayName ?? "");
                            if (e.InspectionId == null) e.Error = "检测记录链接失败";
                        });
                    }
                    // 链接 NG位置
                    var positionIdLabelList = pageItems.Where(x => x.PositionFkDisplayName != null).Select(x => x.PositionFkDisplayName).Distinct().ToList();
                    if (positionIdLabelList.Any()) {
                        var positionIdLinkMap = _lkInspectionNgPositionRep.Context.Queryable<LkNgPosition>().Where(u => positionIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.PositionId = positionIdLinkMap.GetValueOrDefault(e.PositionFkDisplayName ?? "");
                            if (e.PositionId == null) e.Error = "NG位置链接失败";
                        });
                    }
                    // 链接 泄露程度
                    var leakageSeverityIdLabelList = pageItems.Where(x => x.LeakageSeverityFkDisplayName != null).Select(x => x.LeakageSeverityFkDisplayName).Distinct().ToList();
                    if (leakageSeverityIdLabelList.Any()) {
                        var leakageSeverityIdLinkMap = _lkInspectionNgPositionRep.Context.Queryable<LkLeakageSeverity>().Where(u => leakageSeverityIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.LeakageSeverityId = leakageSeverityIdLinkMap.GetValueOrDefault(e.LeakageSeverityFkDisplayName ?? "");
                            if (e.LeakageSeverityId == null) e.Error = "泄露程度链接失败";
                        });
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.InspectionId == null){
                            x.Error = "检测记录不能为空";
                            return false;
                        }
                        if (x.PositionId == null){
                            x.Error = "NG位置不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<LkInspectionNgPosition>>();
                    
                    var storageable = _lkInspectionNgPositionRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.ImageUrl?.Length > 256, "图片URL长度不能超过256个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.InspectionId,
                        it.PositionId,
                        it.ImageUrl,
                        it.LeakageSeverityId,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
