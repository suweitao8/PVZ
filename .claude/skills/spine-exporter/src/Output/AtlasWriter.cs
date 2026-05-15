using SpineExporter.Models;
using SpineExporter.Core;
using SixLabors.ImageSharp;

namespace SpineExporter.Output;

/// <summary>
/// 写入精灵表和 atlas 文件
/// </summary>
public sealed class AtlasWriter
{
    /// <summary>
    /// 写入精灵表到目录
    /// </summary>
    public async Task WriteAsync(
        List<SpriteSheet> sheets,
        string outputDir,
        string baseName,
        string sourceName,
        int fps)
    {
        Directory.CreateDirectory(outputDir);

        foreach (var sheet in sheets)
        {
            var fileName = sheet.GetOutputFileName(baseName);
            var pngPath = Path.Combine(outputDir, $"{fileName}.png");
            var atlasPath = Path.Combine(outputDir, $"{fileName}.atlas");

            // 写入 PNG
            await sheet.Image.SaveAsPngAsync(pngPath);
            Logger.Info($"    写入: {pngPath}");

            // 写入 Atlas
            var atlasContent = GenerateAtlas(sheet, sourceName, fps);
            await File.WriteAllTextAsync(atlasPath, atlasContent);
            Logger.Info($"    写入: {atlasPath}");
        }
    }

    /// <summary>
    /// 生成 atlas 文件内容
    /// </summary>
    private string GenerateAtlas(SpriteSheet sheet, string sourceName, int fps)
    {
        var writer = new StringWriter();
        var fileName = sheet.GetOutputFileName(Path.GetFileNameWithoutExtension(sourceName));

        writer.WriteLine($"# {fileName}.atlas");
        writer.WriteLine();

        // Metadata
        writer.WriteLine("[metadata]");
        writer.WriteLine($"source: {sourceName}");
        writer.WriteLine($"fps: {fps}");
        writer.WriteLine($"created: {DateTime.UtcNow:yyyy-MM-dd}");
        writer.WriteLine();

        // Texture
        writer.WriteLine("[texture]");
        writer.WriteLine($"file: {fileName}.png");
        writer.WriteLine($"size: {sheet.Size}, {sheet.Size}");
        writer.WriteLine();

        // Animations
        var animationGroups = sheet.Frames
            .GroupBy(f => f.AnimationName)
            .OrderBy(g => g.Key);

        foreach (var group in animationGroups)
        {
            var frames = group.OrderBy(f => f.FrameIndex).ToList();

            writer.WriteLine($"[animation: {group.Key}]");
            writer.WriteLine($"loop: {IsLoopAnimation(group.Key).ToString().ToLower()}");
            writer.WriteLine($"frames: {frames.Count}");

            foreach (var frame in frames)
            {
                writer.WriteLine($"frame_{frame.FrameIndex:D2}:");
                writer.WriteLine($"  bounds: {frame.Bounds.X}, {frame.Bounds.Y}, {frame.Bounds.Width}, {frame.Bounds.Height}");
            }

            writer.WriteLine();
        }

        return writer.ToString();
    }

    /// <summary>
    /// 判断动画是否为循环动画
    /// </summary>
    private static bool IsLoopAnimation(string animationName)
    {
        var loopPatterns = new[] { "idle", "walk", "run", "fly", "swim", "float", "hover", "loop" };
        return loopPatterns.Any(p => animationName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
