---
name: spine-exporter
description: 将 Spine 动画批量转换为精灵表序列帧格式
---

# Spine 动画导出工具

将项目中的 Spine 动画转换为精灵表格式。

## 使用方式

### 方式 1: 在 Godot 编辑器中运行（推荐 - 真实渲染）

这是推荐的方式，可以获得真正的 Spine 动画渲染结果：

1. 在 Godot 编辑器中打开 `.claude/skills/spine-exporter/godot/spine_exporter_editor.gd`
2. 修改脚本顶部的参数（可选）：
   - `input_dir` - 输入目录，默认 "animations"
   - `output_dir` - 输出目录，默认 "animations_spritesheet"
   - `filter` - 过滤器，只处理匹配的动画，留空处理所有
   - `fps` - 帧率，默认 8
   - `sheet_size` - 精灵表尺寸，默认 2048
3. 点击编辑器右上角的"运行脚本"按钮（或按 Ctrl+Shift+X）
4. 等待导出完成

### 方式 2: .NET 命令行工具（占位符渲染）

不依赖 Godot 编辑器，但只能生成从图集提取的静态帧：

```bash
# Windows
.claude\skills\spine-exporter\spine-exporter.bat --filter monsters --fps 8

# 或直接调用 DLL
dotnet .claude/skills/spine-exporter/src/bin/Release/net8.0/SpineExporter.dll --filter monsters --fps 8
```

### 方式 3: Godot Headless（需要标准版 Godot）

```bash
godot --headless --script res://.claude/skills/spine-exporter/godot/spine_exporter.gd -- --input animations --output animations_spritesheet --fps 8
```

> **注意**: Godot Mono 版本在 headless 模式下可能崩溃。推荐使用标准版 Godot。

## 参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| --input, -i | animations | 输入目录 |
| --output, -o | animations_spritesheet | 输出目录 |
| --filter, -f | 无 | 只处理匹配路径的动画 |
| --fps | 8 | 帧率 |
| --sheet-size, -s | 2048 | 精灵表尺寸 |
| --overwrite | false | 覆盖已存在的文件 |

## 输出格式

```
animations_spritesheet/
├── monsters/
│   └── twig_slime_s/
│       ├── twig_slime_s_0.png    # 精灵表图片
│       └── twig_slime_s_0.atlas  # 帧位置描述
└── export_report.json
```

## Atlas 文件格式

```ini
# twig_slime_s_0.atlas

[metadata]
source: twig_slime_s.skel
fps: 8
created: 2026-05-04

[texture]
file: twig_slime_s_0.png
size: 2048, 2048

[animation: idle_loop]
loop: true
frames: 8
frame_00:
  bounds: 0, 0, 155, 83
frame_01:
  bounds: 155, 0, 155, 83
...

[animation: attack]
loop: false
frames: 8
frame_00:
  bounds: 1240, 0, 155, 83
...
```

## 三种方式的区别

| 方式 | 渲染质量 | 依赖 | 速度 |
|------|----------|------|------|
| Godot 编辑器 | 真实 Spine 渲染 | Godot 编辑器 | 较慢 |
| .NET 工具 | 从图集提取区域 | 无 | 快 |
| Godot Headless | 真实 Spine 渲染 | 标准版 Godot | 较慢 |

## 构建 .NET 工具

```bash
cd .claude/skills/spine-exporter/src
dotnet build -c Release
```

## 依赖

- Godot 编辑器运行方式需要 Spine GDExtension 正确安装
- .NET 工具需要 .NET 8 SDK
