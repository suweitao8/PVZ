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
        var loader = new SpineLoader();
        var skeletonData = loader.Load(skelFile);

        // 渲染所有动画
        var allFrames = new List<AnimationFrame>();
        using var renderer = new AnimationRenderer(skeletonData);

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
