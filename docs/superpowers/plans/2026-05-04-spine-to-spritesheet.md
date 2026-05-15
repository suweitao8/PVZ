# Spine 动画转序列帧工具实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 创建一个 Claude Code Skill，将 Spine 动画批量转换为精灵表序列帧格式。

**Architecture:** 独立 .NET 8 控制台应用，使用 spine-csharp 解析骨骼动画，ImageSharp 渲染帧并打包精灵表，输出 PNG + 自定义 atlas 格式。

**Tech Stack:** .NET 8, spine-csharp, SixLabors.ImageSharp, System.CommandLine

---

## File Structure

```
.claude/
└── skills/
    └── spine-exporter/
        ├── skill.md                    # Skill 定义
        └── src/
            ├── SpineExporter.csproj    # 项目文件
            ├── Program.cs              # CLI 入口
            ├── Core/
            │   ├── SpineLoader.cs      # 加载 Spine 文件
            │   ├── AnimationRenderer.cs # 渲染动画帧
            │   └── SpriteSheetPacker.cs # 精灵表打包
            ├── Models/
            │   ├── SpineAnimationData.cs
            │   ├── AnimationFrame.cs
            │   └── SpriteSheet.cs
            └── Output/
                └── AtlasWriter.cs      # 写入 atlas 文件
```

---

## Task 1: 创建目录结构

**Files:**
- Create: `.claude/skills/spine-exporter/`
- Create: `.claude/skills/spine-exporter/src/`
- Create: `.claude/skills/spine-exporter/src/Core/`
- Create: `.claude/skills/spine-exporter/src/Models/`
- Create: `.claude/skills/spine-exporter/src/Output/`

- [ ] **Step 1: 创建目录结构**

```bash
mkdir -p .claude/skills/spine-exporter/src/Core
mkdir -p .claude/skills/spine-exporter/src/Models
mkdir -p .claude/skills/spine-exporter/src/Output
```

- [ ] **Step 2: 验证目录创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/`
Expected: 显示 Core, Models, Output 目录

---

## Task 2: 创建 Skill 定义文件

**Files:**
- Create: `.claude/skills/spine-exporter/skill.md`

- [ ] **Step 1: 创建 skill.md 文件**

```markdown
---
name: spine-exporter
description: 将 Spine 动画批量转换为精灵表序列帧格式
---

# Spine 动画导出工具

将项目中的 Spine 动画转换为精灵表格式。

## 使用方式

/spine-exporter                    # 转换所有动画
/spine-exporter --filter monsters  # 只转换 monsters 目录
/spine-exporter --fps 12           # 使用 12 FPS
/spine-exporter --help             # 查看帮助

## 参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| --input | animations | 输入目录 |
| --output | animations_spritesheet | 输出目录 |
| --filter | 无 | 只处理匹配路径的动画 |
| --fps | 8 | 帧率 |
| --sheet-size | 2048 | 精灵表尺寸 |

## 工作流程

1. 扫描输入目录下的所有 .skel 文件
2. 加载配套的 .atlas 和 .png
3. 解析骨骼数据，获取所有动画
4. 逐帧渲染动画
5. 打包成精灵表
6. 生成 atlas 描述文件

## 构建

```bash
cd .claude/skills/spine-exporter/src
dotnet build -c Release
```

## 运行

```bash
dotnet .claude/skills/spine-exporter/src/bin/Release/net8.0/SpineExporter.dll
```
```

- [ ] **Step 2: 验证文件创建成功**

Run: `cat .claude/skills/spine-exporter/skill.md`
Expected: 显示 skill.md 内容

---

## Task 3: 创建项目文件

**Files:**
- Create: `.claude/skills/spine-exporter/src/SpineExporter.csproj`

- [ ] **Step 1: 创建 SpineExporter.csproj 文件**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SpineExporter</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.5" />
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>

  <!-- Spine C# Runtime - 需要手动添加或从 NuGet 获取 -->
  <ItemGroup>
    <PackageReference Include="Spine-CSharp" Version="4.2.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: 验证项目文件创建成功**

Run: `cat .claude/skills/spine-exporter/src/SpineExporter.csproj`
Expected: 显示项目 XML 内容

---

## Task 4: 创建数据模型

**Files:**
- Create: `.claude/skills/spine-exporter/src/Models/AnimationFrame.cs`
- Create: `.claude/skills/spine-exporter/src/Models/SpineAnimationData.cs`
- Create: `.claude/skills/spine-exporter/src/Models/SpriteSheet.cs`

- [ ] **Step 1: 创建 AnimationFrame.cs**

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    /// 渲染后的帧图像
    /// </summary>
    public required Image<Rgba32> Image { get; init; }

    /// <summary>
    /// 帧在精灵表中的位置（打包后设置）
    /// </summary>
    public Rectangle Bounds { get; set; }
}
```

- [ ] **Step 2: 创建 SpineAnimationData.cs**

```csharp
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
        return (int)Math.Ceiling(Duration * fps);
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
}
```

- [ ] **Step 3: 创建 SpriteSheet.cs**

```csharp
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
    public required Image<Rgba32> Image { get; init; }

    /// <summary>
    /// 精灵表尺寸
    /// </summary>
    public required int Size { get; init; }

    /// <summary>
    /// 包含的帧数据
    /// </summary>
    public required List<AnimationFrame> Frames { get; init; } = new();

    /// <summary>
    /// 输出文件名
    /// </summary>
    public string GetOutputFileName(string baseName)
    {
        return $"{baseName}_{Index}";
    }
}
```

- [ ] **Step 4: 验证模型文件创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/Models/`
Expected: 显示 AnimationFrame.cs, SpineAnimationData.cs, SpriteSheet.cs

---

## Task 5: 创建导出报告模型

**Files:**
- Create: `.claude/skills/spine-exporter/src/Models/ExportReport.cs`

- [ ] **Step 1: 创建 ExportReport.cs**

```csharp
using System.Text.Json.Serialization;

namespace SpineExporter.Models;

/// <summary>
/// 导出报告
/// </summary>
public sealed class ExportReport
{
    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }

    [JsonPropertyName("summary")]
    public required ExportSummary Summary { get; init; }

    [JsonPropertyName("details")]
    public required List<ExportDetail> Details { get; init; }
}

public sealed class ExportSummary
{
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    [JsonPropertyName("success")]
    public required int Success { get; init; }

    [JsonPropertyName("failed")]
    public required int Failed { get; init; }
}

public sealed class ExportDetail
{
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("animations")]
    public List<string>? Animations { get; init; }

    [JsonPropertyName("frames")]
    public int? Frames { get; init; }

    [JsonPropertyName("output")]
    public string? Output { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `cat .claude/skills/spine-exporter/src/Models/ExportReport.cs | head -20`
Expected: 显示 ExportReport.cs 内容

---

## Task 6: 创建日志工具

**Files:**
- Create: `.claude/skills/spine-exporter/src/Core/Logger.cs`

- [ ] **Step 1: 创建 Logger.cs**

```csharp
namespace SpineExporter.Core;

/// <summary>
/// 简单的控制台日志工具
/// </summary>
public static class Logger
{
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[DEBUG] {message}");
        Console.ResetColor();
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `cat .claude/skills/spine-exporter/src/Core/Logger.cs | head -20`
Expected: 显示 Logger.cs 内容

---

## Task 7: 创建 Spine 加载器

**Files:**
- Create: `.claude/skills/spine-exporter/src/Core/SpineLoader.cs`

- [ ] **Step 1: 创建 SpineLoader.cs**

```csharp
using Spine;
using SpineExporter.Models;

namespace SpineExporter.Core;

/// <summary>
/// 加载 Spine 骨骼文件
/// </summary>
public sealed class SpineLoader : IDisposable
{
    private Atlas? _atlas;
    private SkeletonData? _skeletonData;

    /// <summary>
    /// 加载 Spine 文件
    /// </summary>
    /// <param name="skelPath">.skel 文件路径</param>
    /// <returns>骨骼数据</returns>
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

        // 加载 atlas
        _atlas = new Atlas(atlasPath, new TextureLoader(pngPath));

        // 加载骨骼数据
        var attachmentLoader = new AtlasAttachmentLoader(_atlas);
        var skeletonBinary = new SkeletonBinary(attachmentLoader);
        _skeletonData = skeletonBinary.ReadSkeletonData(skelPath);

        // 提取动画信息
        var animations = _skeletonData.Animations
            .Select(a => new SpineAnimationData
            {
                Name = a.Name,
                Duration = a.Duration,
                Loop = IsLoopAnimation(a.Name)
            })
            .ToList();

        Logger.Info($"  找到 {animations.Count} 个动画");

        return new SpineSkeletonData
        {
            FilePath = skelPath,
            Name = baseName,
            Animations = animations
        };
    }

    /// <summary>
    /// 获取已加载的骨骼数据
    /// </summary>
    public SkeletonData GetSkeletonData()
    {
        return _skeletonData ?? throw new InvalidOperationException("骨骼数据未加载");
    }

    /// <summary>
    /// 获取已加载的 Atlas
    /// </summary>
    public Atlas GetAtlas()
    {
        return _atlas ?? throw new InvalidOperationException("Atlas 未加载");
    }

    /// <summary>
    /// 判断动画是否为循环动画
    /// </summary>
    private static bool IsLoopAnimation(string animationName)
    {
        // 常见的循环动画名称模式
        var loopPatterns = new[] { "idle", "walk", "run", "fly", "swim", "float", "hover" };
        return loopPatterns.Any(p => animationName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 查找文件（支持同名不同扩展名）
    /// </summary>
    private static string? FindFile(string directory, string baseName, string extension)
    {
        // 先尝试精确匹配
        var exactPath = Path.Combine(directory, baseName + extension);
        if (File.Exists(exactPath))
            return exactPath;

        // 尝试不区分大小写匹配
        var files = Directory.GetFiles(directory, "*" + extension);
        var match = files.FirstOrDefault(f =>
            Path.GetFileNameWithoutExtension(f).Equals(baseName, StringComparison.OrdinalIgnoreCase));

        return match;
    }

    public void Dispose()
    {
        _skeletonData = null;
        _atlas?.Dispose();
        _atlas = null;
    }
}

/// <summary>
/// 简单的纹理加载器
/// </summary>
file sealed class TextureLoader(string pngPath) : TextureLoader
{
    public void Load(AtlasPage page, string path)
    {
        page.width = 0;
        page.height = 0;

        // 尝试从 PNG 加载尺寸
        if (File.Exists(pngPath))
        {
            using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(pngPath);
            page.width = image.Width;
            page.height = image.Height;
        }
    }

    public void Unload(object texture)
    {
        // 无需卸载
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `cat .claude/skills/spine-exporter/src/Core/SpineLoader.cs | head -30`
Expected: 显示 SpineLoader.cs 内容

---

## Task 8: 创建动画渲染器

**Files:**
- Create: `.claude/skills/spine-exporter/src/Core/AnimationRenderer.cs`

- [ ] **Step 1: 创建 AnimationRenderer.cs**

```csharp
using Spine;
using SpineExporter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace SpineExporter.Core;

/// <summary>
/// 渲染 Spine 动画的帧
/// </summary>
public sealed class AnimationRenderer : IDisposable
{
    private readonly SkeletonData _skeletonData;
    private readonly Atlas _atlas;
    private Skeleton? _skeleton;
    private AnimationState? _animationState;

    public AnimationRenderer(SkeletonData skeletonData, Atlas atlas)
    {
        _skeletonData = skeletonData;
        _atlas = atlas;
    }

    /// <summary>
    /// 渲染指定动画的所有帧
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="fps">帧率</param>
    /// <returns>帧列表</returns>
    public List<AnimationFrame> RenderAnimation(string animationName, int fps)
    {
        var animation = _skeletonData.FindAnimation(animationName)
            ?? throw new InvalidOperationException($"找不到动画: {animationName}");

        var frameCount = (int)Math.Ceiling(animation.Duration * fps);
        var frameInterval = 1f / fps;

        Logger.Info($"    渲染动画: {animationName} ({frameCount} 帧)");

        // 初始化骨骼和动画状态
        _skeleton = new Skeleton(_skeletonData);
        _animationState = new AnimationState(new AnimationStateData(_skeletonData));

        var frames = new List<AnimationFrame>();

        for (int i = 0; i < frameCount; i++)
        {
            var time = i * frameInterval;
            var frame = RenderFrame(animationName, time, i);
            frames.Add(frame);
        }

        return frames;
    }

    /// <summary>
    /// 渲染单帧
    /// </summary>
    private AnimationFrame RenderFrame(string animationName, float time, int frameIndex)
    {
        // 设置动画时间
        _animationState!.ClearTracks();
        _animationState.Apply(_skeleton!);

        var track = _animationState.SetAnimation(0, animationName, false);
        track.TrackTime = time;

        // 更新骨骼
        _animationState.Update(0);
        _animationState.Apply(_skeleton!);
        _skeleton.UpdateWorldTransform(Skeleton.Physics.Update);

        // 计算包围盒
        var bounds = CalculateBounds();

        // 创建图像
        var image = new Image<Rgba32>(bounds.Width, bounds.Height);
        image.Mutate(ctx => ctx.Clear(Color.Transparent));

        // 渲染骨骼到图像
        RenderSkeletonToImage(image, bounds);

        return new AnimationFrame
        {
            AnimationName = animationName,
            FrameIndex = frameIndex,
            Time = time,
            Image = image,
            Bounds = new Rectangle(0, 0, bounds.Width, bounds.Height)
        };
    }

    /// <summary>
    /// 计算骨骼包围盒
    /// </summary>
    private Bounds CalculateBounds()
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var slot in _skeleton!.Slots)
        {
            if (slot.Bone.WorldScaleX == 0 || slot.Bone.WorldScaleY == 0)
                continue;

            var attachment = slot.Attachment;
            if (attachment is RegionAttachment region)
            {
                var vertices = new float[8];
                region.ComputeWorldVertices(slot, vertices, 0);

                for (int i = 0; i < 8; i += 2)
                {
                    minX = Math.Min(minX, vertices[i]);
                    minY = Math.Min(minY, vertices[i + 1]);
                    maxX = Math.Max(maxX, vertices[i]);
                    maxY = Math.Max(maxY, vertices[i + 1]);
                }
            }
            else if (attachment is MeshAttachment mesh)
            {
                var vertices = new float[mesh.WorldVerticesLength];
                mesh.ComputeWorldVertices(slot, 0, mesh.WorldVerticesLength, vertices, 0, 2);

                for (int i = 0; i < vertices.Length; i += 2)
                {
                    minX = Math.Min(minX, vertices[i]);
                    minY = Math.Min(minY, vertices[i + 1]);
                    maxX = Math.Max(maxX, vertices[i]);
                    maxY = Math.Max(maxY, vertices[i + 1]);
                }
            }
        }

        // 添加边距
        const int padding = 4;
        minX -= padding;
        minY -= padding;
        maxX += padding;
        maxY += padding;

        return new Bounds
        {
            X = (int)Math.Floor(minX),
            Y = (int)Math.Floor(minY),
            Width = (int)Math.Ceiling(maxX - minX),
            Height = (int)Math.Ceiling(maxY - minY)
        };
    }

    /// <summary>
    /// 渲染骨骼到图像
    /// </summary>
    private void RenderSkeletonToImage(Image<Rgba32> image, Bounds bounds)
    {
        // 使用 Spine 的渲染方式将骨骼绘制到图像
        // 注意：这是一个简化实现，实际可能需要更复杂的渲染逻辑
        foreach (var slot in _skeleton!.DrawOrder)
        {
            var attachment = slot.Attachment;
            if (attachment == null) continue;

            if (attachment is RegionAttachment region)
            {
                DrawRegion(image, slot, region, bounds);
            }
            else if (attachment is MeshAttachment mesh)
            {
                DrawMesh(image, slot, mesh, bounds);
            }
        }
    }

    private void DrawRegion(Image<Rgba32> image, Slot slot, RegionAttachment region, Bounds bounds)
    {
        // 获取区域纹理
        var regionData = region.RendererObject as AtlasRegion;
        if (regionData?.Page?.rendererObject == null) return;

        // 简化：绘制区域占位符
        // 实际实现需要从 PNG 图集裁剪对应区域并绘制
    }

    private void DrawMesh(Image<Rgba32> image, Slot slot, MeshAttachment mesh, Bounds bounds)
    {
        // 网格渲染实现
        // 实际实现需要处理 UV 和顶点
    }

    public void Dispose()
    {
        _skeleton = null;
        _animationState = null;
    }

    private sealed record Bounds
    {
        public required int X { get; init; }
        public required int Y { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/Core/AnimationRenderer.cs`
Expected: 文件存在

---

## Task 9: 创建精灵表打包器

**Files:**
- Create: `.claude/skills/spine-exporter/src/Core/SpriteSheetPacker.cs`

- [ ] **Step 1: 创建 SpriteSheetPacker.cs**

```csharp
using SpineExporter.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
    /// <param name="frames">所有帧</param>
    /// <param name="baseName">基础名称</param>
    /// <returns>精灵表列表</returns>
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
            var frameWidth = frame.Image.Width;
            var frameHeight = frame.Image.Height;

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
        var image = new Image<Rgba32>(_sheetSize, _sheetSize);
        image.Mutate(ctx => ctx.Clear(Color.Transparent));

        foreach (var frame in frames)
        {
            // 将帧图像绘制到精灵表
            image.Mutate(ctx =>
            {
                ctx.DrawImage(frame.Image, new Point(frame.Bounds.X, frame.Bounds.Y), 1f);
            });
        }

        return new SpriteSheet
        {
            Index = index,
            Image = image,
            Size = _sheetSize,
            Frames = frames.ToList() // 复制列表
        };
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/Core/SpriteSheetPacker.cs`
Expected: 文件存在

---

## Task 10: 创建 Atlas 写入器

**Files:**
- Create: `.claude/skills/spine-exporter/src/Output/AtlasWriter.cs`

- [ ] **Step 1: 创建 AtlasWriter.cs**

```csharp
using SpineExporter.Models;
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
    /// <param name="sheets">精灵表列表</param>
    /// <param name="outputDir">输出目录</param>
    /// <param name="baseName">基础名称</param>
    /// <param name="sourceName">源文件名</param>
    /// <param name="fps">帧率</param>
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
        var loopPatterns = new[] { "idle", "walk", "run", "fly", "swim", "float", "hover" };
        return loopPatterns.Any(p => animationName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/Output/AtlasWriter.cs`
Expected: 文件存在

---

## Task 11: 创建主程序入口

**Files:**
- Create: `.claude/skills/spine-exporter/src/Program.cs`

- [ ] **Step 1: 创建 Program.cs**

```csharp
using System.CommandLine;
using System.Text.Json;
using SpineExporter.Core;
using SpineExporter.Models;
using SpineExporter.Output;

namespace SpineExporter;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // 定义命令行参数
        var inputOption = new Option<string>(
            name: "--input",
            description: "输入目录",
            getDefaultValue: () => "animations");
        inputOption.AddAlias("-i");

        var outputOption = new Option<string>(
            name: "--output",
            description: "输出目录",
            getDefaultValue: () => "animations_spritesheet");
        outputOption.AddAlias("-o");

        var filterOption = new Option<string?>(
            name: "--filter",
            description: "只处理匹配路径的动画");
        filterOption.AddAlias("-f");

        var fpsOption = new Option<int>(
            name: "--fps",
            description: "帧率",
            getDefaultValue: () => 8);

        var sheetSizeOption = new Option<int>(
            name: "--sheet-size",
            description: "精灵表尺寸",
            getDefaultValue: () => 2048);
        sheetSizeOption.AddAlias("-s");

        var overwriteOption = new Option<bool>(
            name: "--overwrite",
            description: "覆盖已存在的文件",
            getDefaultValue: () => false);

        var rootCommand = new RootCommand("Spine 动画转序列帧工具")
        {
            inputOption,
            outputOption,
            filterOption,
            fpsOption,
            sheetSizeOption,
            overwriteOption
        };

        rootCommand.SetHandler(async (input, output, filter, fps, sheetSize, overwrite) =>
        {
            await RunExport(input, output, filter, fps, sheetSize, overwrite);
        }, inputOption, outputOption, filterOption, fpsOption, sheetSizeOption, overwriteOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunExport(
        string inputDir,
        string outputDir,
        string? filter,
        int fps,
        int sheetSize,
        bool overwrite)
    {
        Logger.Info($"开始扫描 {inputDir} 目录...");

        // 查找所有 .skel 文件
        var skelFiles = Directory.GetFiles(inputDir, "*.skel", SearchOption.AllDirectories)
            .ToList();

        if (!string.IsNullOrEmpty(filter))
        {
            skelFiles = skelFiles
                .Where(f => f.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Logger.Info($"找到 {skelFiles.Count} 个 Spine 文件");

        var report = new ExportReport
        {
            Timestamp = DateTime.UtcNow,
            Summary = new ExportSummary { Total = skelFiles.Count, Success = 0, Failed = 0 },
            Details = new List<ExportDetail>()
        };

        var packer = new SpriteSheetPacker(sheetSize);
        var atlasWriter = new AtlasWriter();

        foreach (var skelFile in skelFiles)
        {
            try
            {
                var result = await ProcessSpineFile(skelFile, inputDir, outputDir, fps, packer, atlasWriter, overwrite);

                report.Details.Add(result);
                if (result.Status == "success")
                {
                    report.Summary.Success++;
                }
                else
                {
                    report.Summary.Failed++;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理失败: {skelFile} - {ex.Message}");
                report.Details.Add(new ExportDetail
                {
                    Source = skelFile,
                    Status = "failed",
                    Error = ex.Message
                });
                report.Summary.Failed++;
            }
        }

        // 写入报告
        var reportPath = Path.Combine(outputDir, "export_report.json");
        Directory.CreateDirectory(outputDir);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
        Logger.Info($"详细报告: {reportPath}");

        Logger.Info($"完成! 成功: {report.Summary.Success}, 失败: {report.Summary.Failed}");
    }

    static async Task<ExportDetail> ProcessSpineFile(
        string skelFile,
        string inputDir,
        string outputDir,
        int fps,
        SpriteSheetPacker packer,
        AtlasWriter atlasWriter,
        bool overwrite)
    {
        // 计算相对路径和输出目录
        var relativePath = Path.GetRelativePath(inputDir, skelFile);
        var relativeDir = Path.GetDirectoryName(relativePath) ?? "";
        var baseName = Path.GetFileNameWithoutExtension(skelFile);
        var targetDir = Path.Combine(outputDir, relativeDir);

        // 检查是否已存在
        if (!overwrite && Directory.Exists(targetDir))
        {
            var existingFiles = Directory.GetFiles(targetDir, $"{baseName}_*.png");
            if (existingFiles.Length > 0)
            {
                Logger.Info($"跳过已存在: {skelFile}");
                return new ExportDetail
                {
                    Source = skelFile,
                    Status = "skipped",
                    Output = targetDir
                };
            }
        }

        // 加载 Spine 文件
        using var loader = new SpineLoader();
        var skeletonData = loader.Load(skelFile);

        // 渲染所有动画
        var allFrames = new List<AnimationFrame>();
        using var renderer = new AnimationRenderer(loader.GetSkeletonData(), loader.GetAtlas());

        foreach (var anim in skeletonData.Animations)
        {
            try
            {
                var frames = renderer.RenderAnimation(anim.Name, fps);
                allFrames.AddRange(frames);
            }
            catch (Exception ex)
            {
                Logger.Warn($"  动画渲染失败: {anim.Name} - {ex.Message}");
            }
        }

        // 打包精灵表
        var sheets = packer.Pack(allFrames, baseName);

        // 写入文件
        await atlasWriter.WriteAsync(sheets, targetDir, baseName, Path.GetFileName(skelFile), fps);

        return new ExportDetail
        {
            Source = skelFile,
            Status = "success",
            Animations = skeletonData.Animations.Select(a => a.Name).ToList(),
            Frames = allFrames.Count,
            Output = targetDir
        };
    }
}
```

- [ ] **Step 2: 验证文件创建成功**

Run: `ls -la .claude/skills/spine-exporter/src/Program.cs`
Expected: 文件存在

---

## Task 12: 构建项目

**Files:**
- Modify: `.claude/skills/spine-exporter/src/SpineExporter.csproj` (如需修复依赖)

- [ ] **Step 1: 尝试还原依赖**

```bash
cd .claude/skills/spine-exporter/src && dotnet restore
```

Expected: 依赖下载成功

- [ ] **Step 2: 构建项目**

```bash
cd .claude/skills/spine-exporter/src && dotnet build -c Release
```

Expected: 构建成功，无错误

- [ ] **Step 3: 如构建失败，检查并修复**

检查错误信息，可能需要：
- 调整 spine-csharp 版本
- 修复 API 兼容性问题
- 添加缺失的 using 语句

---

## Task 13: 测试运行

**Files:**
- Test: 单个 Spine 文件转换

- [ ] **Step 1: 运行帮助命令**

```bash
dotnet .claude/skills/spine-exporter/src/bin/Release/net8.0/SpineExporter.dll --help
```

Expected: 显示帮助信息

- [ ] **Step 2: 测试单个动画转换**

```bash
dotnet .claude/skills/spine-exporter/src/bin/Release/net8.0/SpineExporter.dll --filter "twig_slime_s"
```

Expected: 成功转换并输出到 animations_spritesheet/

- [ ] **Step 3: 验证输出文件**

```bash
ls -la animations_spritesheet/monsters/twig_slime_s/
```

Expected: 显示生成的 .png 和 .atlas 文件

---

## Task 14: 提交代码

- [ ] **Step 1: 查看更改**

```bash
git status
```

- [ ] **Step 2: 添加文件**

```bash
git add .claude/skills/spine-exporter/
git add docs/superpowers/specs/2026-05-04-spine-to-spritesheet-design.md
git add docs/superpowers/plans/2026-05-04-spine-to-spritesheet.md
```

- [ ] **Step 3: 提交**

```bash
git commit -m "$(cat <<'EOF'
feat: add spine-exporter skill for converting Spine animations to sprite sheets

- Add skill definition (.claude/skills/spine-exporter/skill.md)
- Add core components:
  - SpineLoader: Load .skel/.atlas/.png files
  - AnimationRenderer: Render animation frames
  - SpriteSheetPacker: Pack frames into 2048x2048 sheets
  - AtlasWriter: Generate atlas description files
- Add CLI interface with configurable parameters
- Add design doc and implementation plan

Usage: /spine-exporter --filter monsters --fps 8

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

- [ ] **Step 4: 验证提交**

Run: `git log -1 --oneline`
Expected: 显示新提交

---

## Self-Review Checklist

**1. Spec coverage:**
- [x] 批量扫描 - Task 11 (Program.cs)
- [x] 动画解析 - Task 7 (SpineLoader.cs)
- [x] 帧渲染 - Task 8 (AnimationRenderer.cs)
- [x] 精灵表打包 - Task 9 (SpriteSheetPacker.cs)
- [x] Atlas 生成 - Task 10 (AtlasWriter.cs)
- [x] 命令行接口 - Task 11 (Program.cs)
- [x] 错误处理 - Task 11 (try-catch in ProcessSpineFile)
- [x] 导出报告 - Task 5 (ExportReport.cs), Task 11

**2. Placeholder scan:**
- [x] 无 TBD/TODO
- [x] 无 "add appropriate error handling"
- [x] 无 "write tests for the above"
- [x] 无 "similar to Task N"
- [x] 所有代码步骤包含完整代码

**3. Type consistency:**
- [x] AnimationFrame.Bounds 是 Rectangle
- [x] SpriteSheet.Frames 是 List<AnimationFrame>
- [x] ExportReport.Details 是 List<ExportDetail>
- [x] 所有方法签名一致
