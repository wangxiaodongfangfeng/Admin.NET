using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.Core.Service;
using Admin.NET.LvKong.Application.Const;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// Android App 版本管理服务 🧩
/// APK 文件统一存放在 wwwroot/upload/apk/ 目录下。
/// 上传不受全局 Upload.MaxSize 限制（APK 通常 > 50MB）。
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 88)]
public class LkAppVersionService : IDynamicApiController, ITransient
{
    private const string ApkPath = "upload/apk";

    private readonly SqlSugarRepository<SysFile> _sysFileRep;
    private readonly ICustomFileProvider         _fileProvider;
    private readonly IHttpContextAccessor        _httpContextAccessor;

    public LkAppVersionService(
        SqlSugarRepository<SysFile> sysFileRep,
        INamedServiceProvider<ICustomFileProvider> namedProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _sysFileRep          = sysFileRep;
        _httpContextAccessor = httpContextAccessor;
        // 直接使用本地存储提供者（绕过 OSSProvider 判断，APK 始终本地存储）
        _fileProvider = namedProvider.GetService<ITransient>(nameof(DefaultFileProvider));
    }

    // ── 列表 ──────────────────────────────────────────────────────────────────

    /// <summary>获取 APK 版本列表 🔖</summary>
    [DisplayName("获取APK版本列表")]
    [ApiDescriptionSettings(Name = "List"), HttpGet]
    public async Task<List<LkAppVersionOutput>> List()
    {
        var files = await _sysFileRep.AsQueryable()
            .ClearFilter()  // 忽略租户过滤，所有人共享 APK 列表
            .Where(f => f.FilePath == ApkPath && f.Suffix == ".apk")
            .OrderBy(f => f.CreateTime, OrderByType.Desc)
            .ToListAsync();

        var baseUrl = GetBaseUrl();
        return files.Select(f => ToOutput(f, baseUrl)).ToList();
    }

    // ── 上传 ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// 上传 APK ➕
    /// 不受全局 MaxSize（50MB）限制，文件存储在 upload/apk/。
    /// </summary>
    [DisplayName("上传APK")]
    [ApiDescriptionSettings(Name = "Upload"), HttpPost, NonUnify]
    [RequestSizeLimit(524_288_000)]           // 500 MB — ASP.NET Core 层
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    public async Task<LkAppVersionOutput> Upload([Required] IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw Oops.Oh("请选择要上传的 APK 文件");

        if (!file.FileName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            throw Oops.Oh("只允许上传 .apk 文件");

        // 构建 SysFile 记录
        var fileId   = YitIdHelper.NextId();
        var sizeKb   = file.Length / 1024;
        var baseName = Path.GetFileNameWithoutExtension(file.FileName);

        var sysFile = new SysFile
        {
            Id         = fileId,
            FileName   = baseName,
            Suffix     = ".apk",
            FilePath   = ApkPath,
            SizeKb     = sizeKb,
            SizeInfo   = FormatSize(sizeKb),
            BucketName = "Local",
            Provider   = "",
            IsPublic   = true,
        };

        // 存储文件（直接调底层，跳过 MaxSize 校验）
        var finalName = $"{fileId}.apk";
        sysFile = await _fileProvider.UploadFileAsync(file, sysFile, ApkPath, finalName);

        // 写入数据库
        await _sysFileRep.AsInsertable(sysFile).ExecuteCommandAsync();

        return ToOutput(sysFile, GetBaseUrl());
    }

    // ── 删除 ──────────────────────────────────────────────────────────────────

    /// <summary>删除 APK ❌</summary>
    [DisplayName("删除APK")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete([FromBody] DeleteApkInput input)
    {
        var file = await _sysFileRep.GetByIdAsync(input.Id)
                   ?? throw Oops.Oh("文件不存在");

        await _sysFileRep.DeleteAsync(file);
        await _fileProvider.DeleteFileAsync(file);
    }

    // ── 下载 ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// 下载 APK ⬇️（无需登录，供 Android 设备直接下载）
    /// </summary>
    [DisplayName("下载APK")]
    [ApiDescriptionSettings(Name = "Download"), HttpGet, NonUnify]
    [AllowAnonymous]  // APK 下载不需要认证，扫码直接下载
    public async Task<IActionResult> Download([FromQuery] long id)
    {
        // ClearFilter 避免租户过滤导致找不到记录
        var file = await _sysFileRep.AsQueryable().ClearFilter()
                       .FirstAsync(f => f.Id == id)
                   ?? throw Oops.Oh("文件不存在");

        var fileName = System.Web.HttpUtility.UrlEncode(
            file.FileName, System.Text.Encoding.UTF8);
        return await _fileProvider.GetFileStreamResultAsync(file, fileName);
    }

    // ── 私有辅助 ──────────────────────────────────────────────────────────────

    private LkAppVersionOutput ToOutput(SysFile f, string baseUrl) => new()
    {
        Id          = f.Id,
        FileName    = f.FileName + f.Suffix,
        SizeKb      = f.SizeKb,
        SizeInfo    = f.SizeInfo ?? FormatSize(f.SizeKb),
        CreateTime  = f.CreateTime,
        Url         = f.Url,
        DownloadUrl = string.IsNullOrEmpty(f.Url) ? "" : $"{baseUrl}/{f.Url}",
    };

    private string GetBaseUrl()
    {
        var req = _httpContextAccessor.HttpContext?.Request;
        if (req == null) return string.Empty;
        return $"{req.Scheme}://{req.Host}";
    }

    private static string FormatSize(long kb)
    {
        if (kb < 1024) return $"{kb} KB";
        return $"{kb / 1024.0:F1} MB";
    }
}
