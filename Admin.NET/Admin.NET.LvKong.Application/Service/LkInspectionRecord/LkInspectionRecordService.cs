// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Furion.DatabaseAccessor;
using Furion.EventBus;
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
    private readonly IEventPublisher _eventPublisher;

    public LkInspectionRecordService(
        SqlSugarRepository<LkInspectionRecord> lkInspectionRecordRep,
        ISqlSugarClient sqlSugarClient,
        IEventPublisher eventPublisher)
    {
        _lkInspectionRecordRep = lkInspectionRecordRep;
        _sqlSugarClient        = sqlSugarClient;
        _eventPublisher        = eventPublisher;
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
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.Operator.Contains(input.Keyword) || u.Date.Contains(input.Keyword) || u.Time.Contains(input.Keyword) || u.ProductModel.Contains(input.Keyword) || u.SteelStamp.Contains(input.Keyword) || u.TestResult.Contains(input.Keyword) || u.Images.Contains(input.Keyword) || u.Remarks.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Operator), u => u.Operator.Contains(input.Operator.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Date), u => u.Date.Contains(input.Date.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Time), u => u.Time.Contains(input.Time.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ProductModel), u => u.ProductModel.Contains(input.ProductModel.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SteelStamp), u => u.SteelStamp.Contains(input.SteelStamp.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.TestResult), u => u.TestResult.Contains(input.TestResult.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Images), u => u.Images.Contains(input.Images.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remarks), u => u.Remarks.Contains(input.Remarks.Trim()))
            .WhereIF(input.ShiftId != null, u => u.ShiftId == input.ShiftId)
            .WhereIF(input.ProductTypeId != null, u => u.ProductTypeId == input.ProductTypeId)
            .WhereIF(input.ProductStatusId != null, u => u.ProductStatusId == input.ProductStatusId)
            .WhereIF(input.PartStatusId != null, u => u.PartStatusId == input.PartStatusId)
            .WhereIF(input.NgPositionId != null, u => u.NgPositionId == input.NgPositionId)
            .WhereIF(input.UserId != null, u => u.UserId == input.UserId)
            .LeftJoin<LkShift>((u, shift) => u.ShiftId == shift.Id)
            .LeftJoin<LkProductType>((u, shift, productType) => u.ProductTypeId == productType.Id)
            .LeftJoin<LkProductStatus>((u, shift, productType, productStatus) => u.ProductStatusId == productStatus.Id)
            .LeftJoin<LkPartStatus>((u, shift, productType, productStatus, partStatus) => u.PartStatusId == partStatus.Id)
            .LeftJoin<LkNgPosition>((u, shift, productType, productStatus, partStatus, ngPosition) => u.NgPositionId == ngPosition.Id)
            .LeftJoin<SysUser>((u, shift, productType, productStatus, partStatus, ngPosition, user) => u.UserId == user.Id)
            .Select((u, shift, productType, productStatus, partStatus, ngPosition, user) => new LkInspectionRecordOutput
            {
                Id = u.Id,
                Operator = u.Operator,
                Date = u.Date,
                Time = u.Time,
                ShiftId = u.ShiftId,
                ShiftFkDisplayName = $"{shift.Name}",
                PressureHoldTime = u.PressureHoldTime,
                Pressure = u.Pressure,
                ProductTypeId = u.ProductTypeId,
                ProductTypeFkDisplayName = $"{productType.Name}",
                ProductStatusId = u.ProductStatusId,
                ProductStatusFkDisplayName = $"{productStatus.Name}",
                PartStatusId = u.PartStatusId,
                PartStatusFkDisplayName = $"{partStatus.Name}",
                ProductModel = u.ProductModel,
                SteelStamp = u.SteelStamp,
                TestResult = u.TestResult,
                NgPositionId = u.NgPositionId,
                NgPositionFkDisplayName = $"{ngPosition.Name}",
                Leakage = u.Leakage,
                TestCount = u.TestCount,
                Images = u.Images,
                UserId = u.UserId,
                UserFkDisplayName = $"{user.Account}",
                Remarks = u.Remarks,
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
        var ok = await _lkInspectionRecordRep.InsertAsync(entity);
        if (!ok) return 0;

        // 发布新增事件，由扩展订阅者处理业务逻辑（TestCount 计算、Leakage 回填等）
        await _eventPublisher.PublishAsync(LkInspectionRecordEventTypeEnum.Add, entity);

        return entity.Id;
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
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataLkInspectionRecordInput input)
    {
        // 字典表本身数据量极小（几十行），直接全量查即可。
        // FromPage 参数保留以兼容前端调用，逻辑上不再按检测记录过滤：
        //   - 字典表全量查一次性加载，前端按需显示即可
        //   - 避免对 Lk_InspectionRecord 大表做任何关联查询
        var shiftIdData = await _lkInspectionRecordRep.Context
            .Queryable<LkShift>()
            .Select(u => new { Value = u.Id, Label = u.Name })
            .ToListAsync();

        var productTypeIdData = await _lkInspectionRecordRep.Context
            .Queryable<LkProductType>()
            .Select(u => new { Value = u.Id, Label = u.Name })
            .ToListAsync();

        var productStatusIdData = await _lkInspectionRecordRep.Context
            .Queryable<LkProductStatus>()
            .Select(u => new { Value = u.Id, Label = u.Name })
            .ToListAsync();

        var partStatusIdData = await _lkInspectionRecordRep.Context
            .Queryable<LkPartStatus>()
            .Select(u => new { Value = u.Id, Label = u.Name })
            .ToListAsync();

        var ngPositionIdData = await _lkInspectionRecordRep.Context
            .Queryable<LkNgPosition>()
            .Select(u => new { Value = u.Id, Label = u.Name })
            .ToListAsync();

        var userIdData = await _lkInspectionRecordRep.Context
            .Queryable<SysUser>()
            .Select(u => new { Value = u.Id, Label = u.Account })
            .ToListAsync();

        return new Dictionary<string, dynamic>
        {
            { "shiftId",         shiftIdData },
            { "productTypeId",   productTypeIdData },
            { "productStatusId", productStatusIdData },
            { "partStatusId",    partStatusIdData },
            { "ngPositionId",    ngPositionIdData },
            { "userId",          userIdData },
        };
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
        return ExcelHelper.ExportTemplate(new List<ExportLkInspectionRecordOutput>(), "检测记录导入模板", (_, info) =>
        {
            if (nameof(ExportLkInspectionRecordOutput.ShiftFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<LkShift>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionRecordOutput.ProductTypeFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<LkProductType>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionRecordOutput.ProductStatusFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<LkProductStatus>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionRecordOutput.PartStatusFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<LkPartStatus>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionRecordOutput.NgPositionFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<LkNgPosition>().Select(u => $"{u.Name}").Distinct().ToList();
            if (nameof(ExportLkInspectionRecordOutput.UserFkDisplayName) == info.Name) return _lkInspectionRecordRep.Context.Queryable<SysUser>().Select(u => $"{u.Account}").Distinct().ToList();
            return null;
        });
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
                    // 链接 班次
                    var shiftIdLabelList = pageItems.Where(x => x.ShiftFkDisplayName != null).Select(x => x.ShiftFkDisplayName).Distinct().ToList();
                    if (shiftIdLabelList.Any()) {
                        var shiftIdLinkMap = _lkInspectionRecordRep.Context.Queryable<LkShift>().Where(u => shiftIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.ShiftId = shiftIdLinkMap.GetValueOrDefault(e.ShiftFkDisplayName ?? "");
                            if (e.ShiftId == null) e.Error = "班次链接失败";
                        });
                    }
                    // 链接 产品类型
                    var productTypeIdLabelList = pageItems.Where(x => x.ProductTypeFkDisplayName != null).Select(x => x.ProductTypeFkDisplayName).Distinct().ToList();
                    if (productTypeIdLabelList.Any()) {
                        var productTypeIdLinkMap = _lkInspectionRecordRep.Context.Queryable<LkProductType>().Where(u => productTypeIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.ProductTypeId = productTypeIdLinkMap.GetValueOrDefault(e.ProductTypeFkDisplayName ?? "");
                            if (e.ProductTypeId == null) e.Error = "产品类型链接失败";
                        });
                    }
                    // 链接 产品状态
                    var productStatusIdLabelList = pageItems.Where(x => x.ProductStatusFkDisplayName != null).Select(x => x.ProductStatusFkDisplayName).Distinct().ToList();
                    if (productStatusIdLabelList.Any()) {
                        var productStatusIdLinkMap = _lkInspectionRecordRep.Context.Queryable<LkProductStatus>().Where(u => productStatusIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.ProductStatusId = productStatusIdLinkMap.GetValueOrDefault(e.ProductStatusFkDisplayName ?? "");
                            if (e.ProductStatusId == null) e.Error = "产品状态链接失败";
                        });
                    }
                    // 链接 零件状态
                    var partStatusIdLabelList = pageItems.Where(x => x.PartStatusFkDisplayName != null).Select(x => x.PartStatusFkDisplayName).Distinct().ToList();
                    if (partStatusIdLabelList.Any()) {
                        var partStatusIdLinkMap = _lkInspectionRecordRep.Context.Queryable<LkPartStatus>().Where(u => partStatusIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.PartStatusId = partStatusIdLinkMap.GetValueOrDefault(e.PartStatusFkDisplayName ?? "");
                            if (e.PartStatusId == null) e.Error = "零件状态链接失败";
                        });
                    }
                    // 链接 Ng位置
                    var ngPositionIdLabelList = pageItems.Where(x => x.NgPositionFkDisplayName != null).Select(x => x.NgPositionFkDisplayName).Distinct().ToList();
                    if (ngPositionIdLabelList.Any()) {
                        var ngPositionIdLinkMap = _lkInspectionRecordRep.Context.Queryable<LkNgPosition>().Where(u => ngPositionIdLabelList.Contains($"{u.Name}")).ToList().ToDictionary(u => $"{u.Name}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.NgPositionId = ngPositionIdLinkMap.GetValueOrDefault(e.NgPositionFkDisplayName ?? "");
                            if (e.NgPositionId == null) e.Error = "Ng位置链接失败";
                        });
                    }
                    // 链接 用户
                    var userIdLabelList = pageItems.Where(x => x.UserFkDisplayName != null).Select(x => x.UserFkDisplayName).Distinct().ToList();
                    if (userIdLabelList.Any()) {
                        var userIdLinkMap = _lkInspectionRecordRep.Context.Queryable<SysUser>().Where(u => userIdLabelList.Contains($"{u.Account}")).ToList().ToDictionary(u => $"{u.Account}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.UserId = userIdLinkMap.GetValueOrDefault(e.UserFkDisplayName ?? "");
                            if (e.UserId == null) e.Error = "用户链接失败";
                        });
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.ShiftId == null){
                            x.Error = "班次不能为空";
                            return false;
                        }
                        if (x.ProductTypeId == null){
                            x.Error = "产品类型不能为空";
                            return false;
                        }
                        if (x.ProductStatusId == null){
                            x.Error = "产品状态不能为空";
                            return false;
                        }
                        if (x.PartStatusId == null){
                            x.Error = "零件状态不能为空";
                            return false;
                        }
                        if (x.UserId == null){
                            x.Error = "用户不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<LkInspectionRecord>>();
                    
                    var storageable = _lkInspectionRecordRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Operator), "操作员不能为空")
                        .SplitError(it => it.Item.Operator?.Length > 32, "操作员长度不能超过32个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Date), "检测日期不能为空")
                        .SplitError(it => it.Item.Date?.Length > 26, "检测日期长度不能超过26个字符")
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.Time), "检测时间不能为空")
                        .SplitError(it => it.Item.Time?.Length > 26, "检测时间长度不能超过26个字符")
                        .SplitError(it => it.Item.ProductModel?.Length > 64, "二码码长度不能超过64个字符")
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
                        it.Time,
                        it.ShiftId,
                        it.PressureHoldTime,
                        it.Pressure,
                        it.ProductTypeId,
                        it.ProductStatusId,
                        it.PartStatusId,
                        it.ProductModel,
                        it.SteelStamp,
                        it.TestResult,
                        it.NgPositionId,
                        it.Leakage,
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
