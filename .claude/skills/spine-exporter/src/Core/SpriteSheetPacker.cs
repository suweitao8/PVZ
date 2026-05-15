using SpineExporter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SpineExporter.Core;

/// <summary>
/// 精灵表打包器
/// </summary>
public sealed class SpriteSheetPacker
{
    private readonly int _sheetSize;

    public SpriteSheetPacker(int sheetSize = 2048)
    {
        _sheetSize = sheetSize;
    }

    /// <summary>
    /// 将帧打包成精灵表
    /// </summary>
    public List<SpriteSheet> Pack(List<AnimationFrame> frames, string baseName)
    {
        var sheets = new List<SpriteSheet>();
        var currentFrames = new List<AnimationFrame>();
        var currentX = 0;
        var currentY = 0;
        var rowHeight = 0;
        var sheetIndex = 0;

        // 按动画分组排序帧
        var sortedFrames = frames
            .GroupBy(f => f.AnimationName)
            .SelectMany(g => g.OrderBy(f => f.FrameIndex))
            .ToList();

        foreach (var frame in sortedFrames)
        {
            var frameWidth = frame.Width;
            var frameHeight = frame.Height;

            // 检查是否需要换行
            if (currentX + frameWidth > _sheetSize)
            {
                currentX = 0;
                currentY += rowHeight;
                rowHeight = 0;
            }

            // 检查是否需要新建精灵表
            if (currentY + frameHeight > _sheetSize)
            {
                // 保存当前精灵表
                sheets.Add(CreateSheet(sheetIndex, currentFrames, baseName));
                sheetIndex++;

                // 重置
                currentFrames = new List<AnimationFrame>();
                currentX = 0;
                currentY = 0;
                rowHeight = 0;
            }

            // 设置帧位置
            frame.Bounds = new Rectangle(currentX, currentY, frameWidth, frameHeight);
            currentFrames.Add(frame);

            currentX += frameWidth;
            rowHeight = Math.Max(rowHeight, frameHeight);
        }

        // 添加最后一个精灵表
        if (currentFrames.Count > 0)
        {
            sheets.Add(CreateSheet(sheetIndex, currentFrames, baseName));
        }

        Logger.Info($"    生成 {sheets.Count} 张精灵表");

        return sheets;
    }

    /// <summary>
    /// 创建精灵表
    /// </summary>
    private SpriteSheet CreateSheet(int index, List<AnimationFrame> frames, string baseName)
    {
        var sheet = new SpriteSheet
        {
            Index = index,
            Size = _sheetSize
        };
        sheet.InitializeImage();

        // 填充透明背景
        sheet.Image.Mutate(ctx => ctx.Fill(Color.Transparent));

        foreach (var frame in frames)
        {
            // 从帧数据创建图像
            using var frameImage = Image.LoadPixelData<Rgba32>(
                frame.ImageData,
                frame.Width,
                frame.Height);

            // 将帧图像绘制到精灵表
            sheet.Image.Mutate(ctx =>
            {
                ctx.DrawImage(frameImage, new Point(frame.Bounds.X, frame.Bounds.Y), 1f);
            });

            // 将帧添加到列表
            sheet.Frames.Add(frame);
        }

        return sheet;
    }
}
