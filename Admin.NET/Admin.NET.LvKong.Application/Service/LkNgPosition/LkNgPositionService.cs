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
/// NG位置服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkNgPositionService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkNgPosition> _lkNgPositionRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkNgPositionService(SqlSugarRepository<LkNgPosition> lkNgPositionRep, ISqlSugarClient sqlSugarClient)
    {
        _lkNgPositionRep = lkNgPositionRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询NG位置 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询NG位置")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkNgPositionOutput>> Page(PageLkNgPositionInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkNgPositionRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Name.Contains(input.Keyword) || u.Description.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
            .WhereIF(input.IsDefault.HasValue, u => u.IsDefault == input.IsDefault)
            .Select<LkNgPositionOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取NG位置详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取NG位置详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkNgPosition> Detail([FromQuery] QueryByIdLkNgPositionInput input)
    {
        return await _lkNgPositionRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加NG位置 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加NG位置")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkNgPositionInput input)
    {
        var entity = input.Adapt<LkNgPosition>();
        return await _lkNgPositionRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新NG位置 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新NG位置")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkNgPositionInput input)
    {
        var entity = input.Adapt<LkNgPosition>();
        await _lkNgPositionRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除NG位置 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除NG位置")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkNgPositionInput input)
    {
        var entity = await _lkNgPositionRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkNgPositionRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除NG位置 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除NG位置")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkNgPositionInput> input)
    {
        var exp = Expressionable.Create<LkNgPosition>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkNgPositionRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkNgPositionRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 导出NG位置记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出NG位置记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkNgPositionInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkNgPositionOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "NG位置导出记录");
    }
    
    /// <summary>
    /// 下载NG位置数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载NG位置数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkNgPositionOutput>(), "NG位置导入模板");
    }
    
    private static readonly object _lkNgPositionImportLock = new object();
    /// <summary>
    /// 导入NG位置记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入NG位置记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkNgPositionImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkNgPositionInput, LkNgPosition>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.IsDefault == null){
                            x.Error = "是否默认不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<LkNgPosition>>();
                    
                    var storageable = _lkNgPositionRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Name), "NG位置名称不能为空")
                        .SplitError(it => it.Item.Name?.Length > 64, "NG位置名称长度不能超过64个字符")
                        .SplitError(it => it.Item.Description?.Length > 256, "描述长度不能超过256个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Name,
                        it.IsDefault,
                        it.Description,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
