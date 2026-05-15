namespace SpineExporter.Models;

/// <summary>
/// 表示一个 Spine 动画的数据
/// </summary>
public sealed class SpineAnimationData
{
    /// <summary>
    /// 动画名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 动画时长（秒）
    /// </summary>
    public required float Duration { get; init; }

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public required bool Loop { get; init; }

    /// <summary>
    /// 计算指定帧率下的帧数
    /// </summary>
    public int GetFrameCount(int fps)
    {
        return Math.Max(1, (int)Math.Ceiling(Duration * fps));
    }
}

/// <summary>
/// 表示一个 Spine 骨骼文件的所有数据
/// </summary>
public sealed class SpineSkeletonData
{
    /// <summary>
    /// 骨骼文件路径
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// 骨骼名称（基于文件名）
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 所有动画数据
    /// </summary>
    public required IReadOnlyList<SpineAnimationData> Animations { get; init; }

    /// <summary>
    /// Atlas 文件路径
    /// </summary>
    public required string AtlasPath { get; init; }

    /// <summary>
    /// PNG 纹理路径
    /// </summary>
    public required string PngPath { get; init; }
}
