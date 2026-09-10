using System.ComponentModel.DataAnnotations;

namespace Admin.NET.LvKong.Application;

/// <summary>
/// APK 版本信息输出
/// </summary>
public class LkAppVersionOutput
{
    /// <summary>文件 ID（SysFile.Id）</summary>
    public long Id { get; set; }

    /// <summary>原始文件名（含扩展名）</summary>
    public string FileName { get; set; }

    /// <summary>文件大小（KB）</summary>
    public long SizeKb { get; set; }

    /// <summary>文件大小（格式化，如 12.3 MB）</summary>
    public string SizeInfo { get; set; }

    /// <summary>上传时间</summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>相对路径，如 upload/apk/1234567890.apk</summary>
    public string Url { get; set; }

    /// <summary>完整下载 URL，包含服务器地址，可直接用于二维码生成</summary>
    public string DownloadUrl { get; set; }
}

/// <summary>
/// 删除 APK 输入
/// </summary>
public class DeleteApkInput
{
    [Required]
    public long Id { get; set; }
}
