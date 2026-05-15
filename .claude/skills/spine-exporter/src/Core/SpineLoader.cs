using SpineExporter.Models;

namespace SpineExporter.Core;

/// <summary>
/// 加载 Spine 骨骼文件
/// </summary>
public sealed class SpineLoader
{
    // 已知的动画名称列表（按优先级排序）
    private static readonly string[] KnownAnimationNames = new[]
    {
        "idle_loop", "idle", "attack", "hit", "death", "cast", "walk", "run", "jump",
        "fly", "swim", "float", "hover", "spawn", "summon", "shoot", "charge",
        "defend", "block", "dash", "teleport", "cast_loop", "attack_loop",
        "hurt", "damage", "die", "victory", "defeat", "intro", "outro"
    };

    /// <summary>
    /// 加载 Spine 文件信息
    /// </summary>
    public SpineSkeletonData Load(string skelPath)
    {
        // 查找配套文件
        var directory = Path.GetDirectoryName(skelPath)
            ?? throw new InvalidOperationException($"无法获取目录: {skelPath}");
        var baseName = Path.GetFileNameWithoutExtension(skelPath);

        var atlasPath = FindFile(directory, baseName, ".atlas")
            ?? throw new FileNotFoundException($"找不到 atlas 文件: {directory}/{baseName}.atlas");

        var pngPath = FindFile(directory, baseName, ".png")
            ?? throw new FileNotFoundException($"找不到 png 文件: {directory}/{baseName}.png");

        Logger.Info($"加载: {skelPath}");

        // 从 .skel 二进制文件中解析动画名称
        var animations = ParseAnimationsFromSkel(skelPath);

        if (animations.Count == 0)
        {
            Logger.Warn("  未找到动画，使用默认列表");
            animations = GetDefaultAnimations();
        }

        Logger.Info($"  找到 {animations.Count} 个动画: {string.Join(", ", animations.Select(a => a.Name))}");

        return new SpineSkeletonData
        {
            FilePath = skelPath,
            Name = baseName,
            Animations = animations,
            AtlasPath = atlasPath,
            PngPath = pngPath
        };
    }

    /// <summary>
    /// 从 .skel 二进制文件解析动画名称
    /// 直接搜索已知动画名称
    /// </summary>
    private List<SpineAnimationData> ParseAnimationsFromSkel(string skelPath)
    {
        var animations = new List<SpineAnimationData>();

        try
        {
            var data = File.ReadAllBytes(skelPath);

            // 直接搜索已知动画名称
            foreach (var animName in KnownAnimationNames)
            {
                var pattern = System.Text.Encoding.UTF8.GetBytes(animName);
                int idx = IndexOfPattern(data, pattern);

                if (idx >= 0)
                {
                    // 验证边界：前面应该是非打印字符或长度前缀
                    // 后面应该是非打印字符
                    bool validStart = idx == 0 || !IsLetterOrDigit(data[idx - 1]);
                    bool validEnd = idx + pattern.Length >= data.Length || !IsLetterOrDigit(data[idx + pattern.Length]);

                    if (validStart && validEnd && !animations.Any(a => a.Name == animName))
                    {
                        animations.Add(new SpineAnimationData
                        {
                            Name = animName,
                            Duration = 1.0f,
                            Loop = IsLoopAnimation(animName)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"解析 .skel 文件失败: {ex.Message}");
        }

        return animations;
    }

    /// <summary>
    /// 检查字节是否为字母或数字
    /// </summary>
    private static bool IsLetterOrDigit(byte b)
    {
        return (b >= 'a' && b <= 'z') || (b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9') || b == '_';
    }

    /// <summary>
    /// 在字节数组中查找模式
    /// </summary>
    private static int IndexOfPattern(byte[] data, byte[] pattern)
    {
        for (int i = 0; i <= data.Length - pattern.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (data[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            }
            if (found)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 获取默认动画列表
    /// </summary>
    private static List<SpineAnimationData> GetDefaultAnimations()
    {
        return new List<SpineAnimationData>
        {
            new() { Name = "idle_loop", Duration = 1.0f, Loop = true },
            new() { Name = "attack", Duration = 0.5f, Loop = false },
            new() { Name = "hit", Duration = 0.3f, Loop = false },
            new() { Name = "death", Duration = 0.8f, Loop = false }
        };
    }

    /// <summary>
    /// 判断动画是否为循环动画
    /// </summary>
    private static bool IsLoopAnimation(string animationName)
    {
        var loopPatterns = new[] { "idle", "walk", "run", "fly", "swim", "float", "hover", "loop" };
        return loopPatterns.Any(p => animationName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 查找文件（支持同名不同扩展名）
    /// </summary>
    private static string? FindFile(string directory, string baseName, string extension)
    {
        var exactPath = Path.Combine(directory, baseName + extension);
        if (File.Exists(exactPath))
            return exactPath;

        var files = Directory.GetFiles(directory, "*" + extension);
        var match = files.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals(baseName, StringComparison.OrdinalIgnoreCase));

        return match;
    }
}
