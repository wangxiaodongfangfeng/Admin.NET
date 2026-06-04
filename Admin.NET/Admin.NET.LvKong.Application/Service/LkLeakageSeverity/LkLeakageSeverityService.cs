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
/// 泄露程度服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkLeakageSeverityService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkLeakageSeverity> _lkLeakageSeverityRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkLeakageSeverityService(SqlSugarRepository<LkLeakageSeverity> lkLeakageSeverityRep, ISqlSugarClient sqlSugarClient)
    {
        _lkLeakageSeverityRep = lkLeakageSeverityRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询泄露程度 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询泄露程度")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkLeakageSeverityOutput>> Page(PageLkLeakageSeverityInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkLeakageSeverityRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Name.Contains(input.Keyword) || u.Color.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Color), u => u.Color.Contains(input.Color.Trim()))
            .WhereIF(input.Level != null, u => u.Level == input.Level)
            .Select<LkLeakageSeverityOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取泄露程度详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取泄露程度详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkLeakageSeverity> Detail([FromQuery] QueryByIdLkLeakageSeverityInput input)
    {
        return await _lkLeakageSeverityRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加泄露程度 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加泄露程度")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkLeakageSeverityInput input)
    {
        var entity = input.Adapt<LkLeakageSeverity>();
        return await _lkLeakageSeverityRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新泄露程度 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新泄露程度")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkLeakageSeverityInput input)
    {
        var entity = input.Adapt<LkLeakageSeverity>();
        await _lkLeakageSeverityRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除泄露程度 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除泄露程度")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkLeakageSeverityInput input)
    {
        var entity = await _lkLeakageSeverityRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkLeakageSeverityRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除泄露程度 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除泄露程度")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkLeakageSeverityInput> input)
    {
        var exp = Expressionable.Create<LkLeakageSeverity>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkLeakageSeverityRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkLeakageSeverityRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 导出泄露程度记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出泄露程度记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkLeakageSeverityInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkLeakageSeverityOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "泄露程度导出记录");
    }
    
    /// <summary>
    /// 下载泄露程度数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载泄露程度数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkLeakageSeverityOutput>(), "泄露程度导入模板");
    }
    
    private static readonly object _lkLeakageSeverityImportLock = new object();
    /// <summary>
    /// 导入泄露程度记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入泄露程度记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkLeakageSeverityImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkLeakageSeverityInput, LkLeakageSeverity>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.Level == null){
                            x.Error = "严重等级不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<LkLeakageSeverity>>();
                    
                    var storageable = _lkLeakageSeverityRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Name), "泄露程度名称不能为空")
                        .SplitError(it => it.Item.Name?.Length > 32, "泄露程度名称长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Color), "显示颜色不能为空")
                        .SplitError(it => it.Item.Color?.Length > 16, "显示颜色长度不能超过16个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Name,
                        it.Color,
                        it.Level,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
