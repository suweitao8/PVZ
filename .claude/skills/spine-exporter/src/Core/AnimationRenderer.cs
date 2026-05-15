using SpineExporter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SpineExporter.Core;

/// <summary>
/// 动画渲染器
/// 注意：实际的 Spine 动画渲染需要使用 Godot 的 Spine GDExtension
/// 此实现从图集中提取区域作为帧，用于测试工作流程
/// </summary>
public sealed class AnimationRenderer : IDisposable
{
    private readonly SpineSkeletonData _skeletonData;
    private Image<Rgba32>? _atlasImage;
    private Dictionary<string, AtlasRegionInfo> _regions = new();

    public AnimationRenderer(SpineSkeletonData skeletonData)
    {
        _skeletonData = skeletonData;
        LoadAtlas();
    }

    private void LoadAtlas()
    {
        try
        {
            _atlasImage = Image.Load<Rgba32>(_skeletonData.PngPath);
            _regions = ParseAtlas(_skeletonData.AtlasPath);
            Logger.Info($"    加载图集: {_skeletonData.PngPath} ({_regions.Count} 个区域)");
        }
        catch (Exception ex)
        {
            Logger.Warn($"    无法加载图集: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析 .atlas 文件
    /// </summary>
    private Dictionary<string, AtlasRegionInfo> ParseAtlas(string atlasPath)
    {
        var regions = new Dictionary<string, AtlasRegionInfo>();

        var lines = File.ReadAllLines(atlasPath);
        string? currentRegion = null;
        var info = new AtlasRegionInfo();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            // 检测新区域（不以冒号结尾的行）
            if (!trimmed.Contains(':') && !trimmed.StartsWith("size:") &&
                !trimmed.StartsWith("filter:") && !trimmed.StartsWith("format:") &&
                !trimmed.StartsWith("repeat:") && !trimmed.StartsWith("pma:"))
            {
                if (currentRegion != null)
                {
                    regions[currentRegion] = info;
                    info = new AtlasRegionInfo();
                }
                currentRegion = trimmed;
            }
            else if (trimmed.StartsWith("bounds:"))
            {
                var parts = trimmed.Substring(7).Trim().Split(',');
                if (parts.Length == 4)
                {
                    info.X = int.Parse(parts[0].Trim());
                    info.Y = int.Parse(parts[1].Trim());
                    info.Width = int.Parse(parts[2].Trim());
                    info.Height = int.Parse(parts[3].Trim());
                }
            }
            else if (trimmed.StartsWith("offsets:"))
            {
                var parts = trimmed.Substring(8).Trim().Split(',');
                if (parts.Length >= 2)
                {
                    info.OffsetX = int.Parse(parts[0].Trim());
                    info.OffsetY = int.Parse(parts[1].Trim());
                }
            }
            else if (trimmed.StartsWith("rotate:"))
            {
                info.Rotate = trimmed.Contains("true");
            }
        }

        if (currentRegion != null)
        {
            regions[currentRegion] = info;
        }

        return regions;
    }

    /// <summary>
    /// 渲染指定动画的所有帧
    /// </summary>
    public List<AnimationFrame> RenderAnimation(string animationName, int fps)
    {
        var animData = _skeletonData.Animations.FirstOrDefault(a => a.Name == animationName);
        if (animData == null)
        {
            throw new InvalidOperationException($"找不到动画: {animationName}");
        }

        var frameCount = animData.GetFrameCount(fps);
        Logger.Info($"    渲染动画: {animationName} ({frameCount} 帧, {animData.Duration:F2}s)");

        var frames = new List<AnimationFrame>();

        // 计算动画包围盒
        var bounds = CalculateAnimationBounds();

        for (int i = 0; i < frameCount; i++)
        {
            var time = i * (1f / fps);
            var frame = RenderFrame(animationName, i, time, animData.Duration, bounds);
            frames.Add(frame);
        }

        return frames;
    }

    /// <summary>
    /// 计算动画的包围盒（基于所有图集区域）
    /// </summary>
    private (int width, int height) CalculateAnimationBounds()
    {
        if (_atlasImage == null || _regions.Count == 0)
        {
            return (128, 128);
        }

        // 找到最大的区域
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var region in _regions.Values)
        {
            maxWidth = Math.Max(maxWidth, region.Width);
            maxHeight = Math.Max(maxHeight, region.Height);
        }

        // 确保最小尺寸
        return (Math.Max(64, maxWidth), Math.Max(64, maxHeight));
    }

    /// <summary>
    /// 渲染单帧
    /// </summary>
    private AnimationFrame RenderFrame(string animationName, int frameIndex, float time, float duration, (int width, int height) bounds)
    {
        var width = bounds.width;
        var height = bounds.height;

        // 创建帧图像
        using var frameImage = new Image<Rgba32>(width, height);

        // 如果有图集，尝试从中提取区域
        if (_atlasImage != null && _regions.Count > 0)
        {
            // 选择一个区域作为帧内容
            // 简化实现：根据帧索引选择不同区域
            var regionIndex = frameIndex % _regions.Count;
            var region = _regions.Values.ElementAt(regionIndex);

            // 从图集中复制区域
            try
            {
                // 创建临时图像存储裁剪的区域
                using var regionImage = _atlasImage.Clone(ctx => ctx
                    .Crop(new Rectangle(region.X, region.Y, region.Width, region.Height)));

                // 计算居中位置
                int destX = (width - region.Width) / 2;
                int destY = (height - region.Height) / 2;

                // 绘制到帧上
                frameImage.Mutate(ctx => ctx
                    .Fill(Color.Transparent)
                    .DrawImage(regionImage, new Point(destX, destY), 1f));
            }
            catch
            {
                // 如果裁剪失败，填充占位符
                FillPlaceholder(frameImage, time, duration);
            }
        }
        else
        {
            // 填充占位符
            FillPlaceholder(frameImage, time, duration);
        }

        // 转换为字节数组
        var imageData = new byte[width * height * 4];
        frameImage.CopyPixelDataTo(imageData);

        return new AnimationFrame
        {
            AnimationName = animationName,
            FrameIndex = frameIndex,
            Time = time,
            Width = width,
            Height = height,
            ImageData = imageData,
            Bounds = new Rectangle(0, 0, width, height)
        };
    }

    /// <summary>
    /// 填充占位符内容
    /// </summary>
    private static void FillPlaceholder(Image<Rgba32> image, float time, float duration)
    {
        var progress = duration > 0 ? time / duration : 0;
        var alpha = (byte)(128 + (byte)(progress * 64));

        image.Mutate(ctx => ctx.Fill(new Rgba32(200, 200, 200, alpha)));
    }

    public void Dispose()
    {
        _atlasImage?.Dispose();
    }

    private sealed class AtlasRegionInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public bool Rotate { get; set; }
    }
}
