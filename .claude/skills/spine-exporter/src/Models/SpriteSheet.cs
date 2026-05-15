using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SpineExporter.Models;

/// <summary>
/// 表示一个精灵表
/// </summary>
public sealed class SpriteSheet
{
    /// <summary>
    /// 精灵表索引（用于多张拆分）
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// 精灵表图像
    /// </summary>
    public Image<Rgba32> Image { get; private set; }

    /// <summary>
    /// 精灵表尺寸
    /// </summary>
    public required int Size { get; init; }

    /// <summary>
    /// 包含的帧数据
    /// </summary>
    public List<AnimationFrame> Frames { get; } = new();

    public SpriteSheet()
    {
        Image = null!;
    }

    /// <summary>
    /// 初始化图像
    /// </summary>
    public void InitializeImage()
    {
        Image = new Image<Rgba32>(Size, Size);
    }

    /// <summary>
    /// 输出文件名
    /// </summary>
    public string GetOutputFileName(string baseName)
    {
        return $"{baseName}_{Index}";
    }
}
