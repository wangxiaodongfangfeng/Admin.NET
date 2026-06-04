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
/// 检测记录服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class LkInspectionRecordService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<LkInspectionRecord> _lkInspectionRecordRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public LkInspectionRecordService(SqlSugarRepository<LkInspectionRecord> lkInspectionRecordRep, ISqlSugarClient sqlSugarClient)
    {
        _lkInspectionRecordRep = lkInspectionRecordRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询检测记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询检测记录")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<LkInspectionRecordOutput>> Page(PageLkInspectionRecordInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _lkInspectionRecordRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Operator.Contains(input.Keyword) || u.Date.Contains(input.Keyword) || u.ProductModel.Contains(input.Keyword) || u.BatchNumber.Contains(input.Keyword) || u.Specification.Contains(input.Keyword) || u.SteelStamp.Contains(input.Keyword) || u.TestResult.Contains(input.Keyword) || u.Images.Contains(input.Keyword) || u.Remarks.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Operator), u => u.Operator.Contains(input.Operator.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Date), u => u.Date.Contains(input.Date.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ProductModel), u => u.ProductModel.Contains(input.ProductModel.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BatchNumber), u => u.BatchNumber.Contains(input.BatchNumber.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Specification), u => u.Specification.Contains(input.Specification.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SteelStamp), u => u.SteelStamp.Contains(input.SteelStamp.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TestResult), u => u.TestResult.Contains(input.TestResult.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Images), u => u.Images.Contains(input.Images.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remarks), u => u.Remarks.Contains(input.Remarks.Trim()))
            .WhereIF(input.ShiftId != null, u => u.ShiftId == input.ShiftId)
            .WhereIF(input.ProductTypeId != null, u => u.ProductTypeId == input.ProductTypeId)
            .WhereIF(input.UserId != null, u => u.UserId == input.UserId)
            .Select<LkInspectionRecordOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取检测记录详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取检测记录详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<LkInspectionRecord> Detail([FromQuery] QueryByIdLkInspectionRecordInput input)
    {
        return await _lkInspectionRecordRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加检测记录 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加检测记录")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddLkInspectionRecordInput input)
    {
        var entity = input.Adapt<LkInspectionRecord>();
        return await _lkInspectionRecordRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新检测记录 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新检测记录")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateLkInspectionRecordInput input)
    {
        var entity = input.Adapt<LkInspectionRecord>();
        await _lkInspectionRecordRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除检测记录 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除检测记录")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteLkInspectionRecordInput input)
    {
        var entity = await _lkInspectionRecordRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002); 
        await _lkInspectionRecordRep.DeleteAsync(entity);   //真删除 
    }

    /// <summary>
    /// 批量删除检测记录 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除检测记录")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteLkInspectionRecordInput> input)
    {
        var exp = Expressionable.Create<LkInspectionRecord>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _lkInspectionRecordRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
        return await _lkInspectionRecordRep.Context.Deleteable(list).ExecuteCommandAsync();   //真删除--返回受影响的行数
    }
    
    /// <summary>
    /// 导出检测记录记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出检测记录记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageLkInspectionRecordInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportLkInspectionRecordOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "检测记录导出记录");
    }
    
    /// <summary>
    /// 下载检测记录数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载检测记录数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportLkInspectionRecordOutput>(), "检测记录导入模板");
    }
    
    private static readonly object _lkInspectionRecordImportLock = new object();
    /// <summary>
    /// 导入检测记录记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入检测记录记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_lkInspectionRecordImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportLkInspectionRecordInput, LkInspectionRecord>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.ShiftId == null){
                            x.Error = "班次ID不能为空";
                            return false;
                        }
                        if (x.ProductTypeId == null){
                            x.Error = "产品类型ID不能为空";
                            return false;
                        }
                        if (x.UserId == null){
                            x.Error = "用户ID不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<LkInspectionRecord>>();
                    
                    var storageable = _lkInspectionRecordRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Operator), "操作员不能为空")
                        .SplitError(it => it.Item.Operator?.Length > 32, "操作员长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Date), "检测日期不能为空")
                        .SplitError(it => it.Item.Date?.Length > 16, "检测日期长度不能超过16个字符")
                        .SplitError(it => it.Item.ProductModel?.Length > 64, "产品型号长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.BatchNumber), "批次号不能为空")
                        .SplitError(it => it.Item.BatchNumber?.Length > 64, "批次号长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Specification), "规格不能为空")
                        .SplitError(it => it.Item.Specification?.Length > 64, "规格长度不能超过64个字符")
                        .SplitError(it => it.Item.SteelStamp?.Length > 64, "钢印号长度不能超过64个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.TestResult), "检测结果不能为空")
                        .SplitError(it => it.Item.TestResult?.Length > 8, "检测结果长度不能超过8个字符")
                        .SplitError(it => it.Item.Remarks?.Length > 512, "备注长度不能超过512个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.Operator,
                        it.Date,
                        it.ShiftId,
                        it.Pressure,
                        it.ProductTypeId,
                        it.ProductModel,
                        it.BatchNumber,
                        it.Specification,
                        it.SteelStamp,
                        it.TestResult,
                        it.Images,
                        it.UserId,
                        it.Remarks,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
