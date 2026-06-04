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
/// 班次服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkShiftService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkShift> _lkShiftRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkShiftService(SqlSugarRepository<LkShift> lkShiftRep, ISqlSugarClient sqlSugarClient)
    {
        _lkShiftRep = lkShiftRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询班次 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询班次")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkShiftOutput>> Page(PageLkShiftInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkShiftRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Name.Contains(input.Keyword) || u.StartTime.Contains(input.Keyword) || u.EndTime.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime), u => u.StartTime.Contains(input.StartTime.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTime), u => u.EndTime.Contains(input.EndTime.Trim()))
            .Select<LkShiftOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取班次详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取班次详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkShift> Detail([FromQuery] QueryByIdLkShiftInput input)
    {
        return await _lkShiftRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加班次 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加班次")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkShiftInput input)
    {
        var entity = input.Adapt<LkShift>();
        return await _lkShiftRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新班次 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新班次")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkShiftInput input)
    {
        var entity = input.Adapt<LkShift>();
        await _lkShiftRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除班次 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除班次")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkShiftInput input)
    {
        var entity = await _lkShiftRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkShiftRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除班次 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除班次")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkShiftInput> input)
    {
        var exp = Expressionable.Create<LkShift>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkShiftRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkShiftRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 导出班次记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出班次记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkShiftInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkShiftOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "班次导出记录");
    }
    
    /// <summary>
    /// 下载班次数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载班次数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkShiftOutput>(), "班次导入模板");
    }
    
    private static readonly object _lkShiftImportLock = new object();
    /// <summary>
    /// 导入班次记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入班次记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkShiftImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkShiftInput, LkShift>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<LkShift>>();
                    
                    var storageable = _lkShiftRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Name), "班次名称不能为空")
                        .SplitError(it => it.Item.Name?.Length > 64, "班次名称长度不能超过64个字符")
                        .SplitError(it => it.Item.StartTime?.Length > 8, "开始时间长度不能超过8个字符")
                        .SplitError(it => it.Item.EndTime?.Length > 8, "结束时间长度不能超过8个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Name,
                        it.StartTime,
                        it.EndTime,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
