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
/// 产品状态服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkProductStatusService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkProductStatus> _lkProductStatusRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkProductStatusService(SqlSugarRepository<LkProductStatus> lkProductStatusRep, ISqlSugarClient sqlSugarClient)
    {
        _lkProductStatusRep = lkProductStatusRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询产品状态 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询产品状态")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkProductStatusOutput>> Page(PageLkProductStatusInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkProductStatusRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Name.Contains(input.Keyword) || u.Description.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
            .Select<LkProductStatusOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取产品状态详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取产品状态详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkProductStatus> Detail([FromQuery] QueryByIdLkProductStatusInput input)
    {
        return await _lkProductStatusRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加产品状态 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加产品状态")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkProductStatusInput input)
    {
        var entity = input.Adapt<LkProductStatus>();
        return await _lkProductStatusRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新产品状态 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新产品状态")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkProductStatusInput input)
    {
        var entity = input.Adapt<LkProductStatus>();
        await _lkProductStatusRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除产品状态 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除产品状态")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkProductStatusInput input)
    {
        var entity = await _lkProductStatusRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkProductStatusRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除产品状态 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除产品状态")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkProductStatusInput> input)
    {
        var exp = Expressionable.Create<LkProductStatus>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkProductStatusRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkProductStatusRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 导出产品状态记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出产品状态记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkProductStatusInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkProductStatusOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "产品状态导出记录");
    }
    
    /// <summary>
    /// 下载产品状态数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载产品状态数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkProductStatusOutput>(), "产品状态导入模板");
    }
    
    private static readonly object _lkProductStatusImportLock = new object();
    /// <summary>
    /// 导入产品状态记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入产品状态记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkProductStatusImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkProductStatusInput, LkProductStatus>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<LkProductStatus>>();
                    
                    var storageable = _lkProductStatusRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Name), "产品状态名称不能为空")
                        .SplitError(it => it.Item.Name?.Length > 64, "产品状态名称长度不能超过64个字符")
                        .SplitError(it => it.Item.Description?.Length > 256, "描述长度不能超过256个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Name,
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
