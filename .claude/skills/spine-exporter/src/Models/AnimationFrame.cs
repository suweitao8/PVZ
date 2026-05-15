using SixLabors.ImageSharp;

namespace SpineExporter.Models;

/// <summary>
/// 表示动画的单帧数据
/// </summary>
public sealed class AnimationFrame
{
    /// <summary>
    /// 帧所属的动画名称
    /// </summary>
    public required string AnimationName { get; init; }

    /// <summary>
    /// 帧在动画中的索引
    /// </summary>
    public required int FrameIndex { get; init; }

    /// <summary>
    /// 帧的时间点（秒）
    /// </summary>
    public required float Time { get; init; }

    /// <summary>
    /// 帧在精灵表中的位置（打包后设置）
    /// </summary>
    public required Rectangle Bounds { get; set; }

    /// <summary>
    /// 帧图像数据（RGBA 字节数组）
    /// </summary>
    public required byte[] ImageData { get; init; }

    /// <summary>
    /// 图像宽度
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// 图像高度
    /// </summary>
    public required int Height { get; init; }
}
