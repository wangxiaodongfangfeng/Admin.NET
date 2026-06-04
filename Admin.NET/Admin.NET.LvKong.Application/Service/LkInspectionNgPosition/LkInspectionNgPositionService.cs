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
using Admin.NET.LvKong.Application.Const;
using Admin.NET.LvKong.Application.Entity;
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
            .Select<LkInspectionNgPositionOutput>();
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
        return ExcelHelper.ExportTemplate(new List<ExportLkInspectionNgPositionOutput>(), "NG位置详情导入模板");
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
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.InspectionId == null){
                            x.Error = "检测记录ID不能为空";
                            return false;
                        }
                        if (x.PositionId == null){
                            x.Error = "NG位置ID不能为空";
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
