# Spine 动画转序列帧工具设计文档

**日期**: 2026-05-04
**状态**: 待审核
**作者**: Claude

---

## 1. 概述

### 1.1 目标

创建一个 Claude Code Skill，将项目中的 Spine 动画（`.skel` + `.atlas` + `.png`）批量转换为精灵表（Sprite Sheet）格式，用于游戏内替换 Spine 运行时渲染。

### 1.2 背景

- 项目使用 Godot 4.5 + C# 开发
- 现有 158 个 Spine 动画文件，分布在 144 个目录
- 目标：用序列帧替代 Spine 动画，减少运行时依赖
- 作为 Claude Code Skill 集成，方便复用

---

## 2. 功能需求

### 2.1 核心功能

| 功能 | 描述 |
|------|------|
| 批量扫描 | 自动扫描指定目录下所有 `.skel` 文件 |
| 动画解析 | 读取骨骼数据，获取所有动画名称和时长 |
| 帧渲染 | 以指定帧率渲染每个动画的每一帧 |
| 精灵表打包 | 将所有帧打包到 2048×2048 的图片中 |
| Atlas 生成 | 生成配套的帧位置描述文件 |

### 2.2 输出规格

| 参数 | 值 |
|------|-----|
| 帧率 | 8 FPS（可配置） |
| 精灵表尺寸 | 2048×2048（超出自动拆分） |
| 输出格式 | PNG + 自定义 Atlas 格式 |
| 背景 | 透明（保留 Alpha 通道） |
| 组织方式 | 每个角色/怪物一张精灵表 |

---

## 3. 项目结构（Skill 格式）

```
.claude/
├── skills/
│   └── spine-exporter/
│       ├── skill.md              # Skill 定义文件
│       └── src/
│           ├── SpineExporter.csproj    # .NET 8 控制台项目
│           ├── Program.cs              # CLI 入口，参数解析
│           ├── Core/
│           │   ├── SpineLoader.cs      # 加载 .skel/.atlas/.png
│           │   ├── AnimationRenderer.cs # 渲染动画帧
│           │   └── SpriteSheetPacker.cs # 精灵表打包算法
│           ├── Models/
│           │   ├── SpineAnimationData.cs # 动画数据模型
│           │   ├── AnimationFrame.cs     # 单帧数据
│           │   └── SpriteSheet.cs        # 精灵表模型
│           └── Output/
│               └── AtlasWriter.cs        # 写入精灵表和atlas文件
└── settings.json                   # 项目设置（如已存在）
```

---

## 4. Skill 定义

### 4.1 skill.md 内容

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
```

---

## 5. 核心工作流程

```
┌─────────────────────────────────────────────────────────────────┐
│                        主流程 (Program.cs)                       │
├─────────────────────────────────────────────────────────────────┤
│  1. 扫描 animations/ 目录，找到所有 .skel 文件                   │
│  2. 对每个 .skel 文件：                                          │
│     a. 加载配套的 .atlas 和 .png                                 │
│     b. 解析骨骼数据，获取所有动画名称和时长                       │
│     c. 对每个动画：                                              │
│        - 根据时长和 8 FPS 计算帧数                               │
│        - 逐帧渲染到 Image 对象                                   │
│     d. 将所有帧打包成 2048×2048 精灵表（超出则拆分）             │
│     e. 生成对应的 .atlas 描述文件                                │
│     f. 输出到 animations_spritesheet/                           │
│  3. 输出转换报告（成功/失败/警告）                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. 输出目录结构

```
animations_spritesheet/
├── monsters/
│   ├── twig_slime_s/
│   │   ├── twig_slime_s_0.png    # 精灵表图片
│   │   └── twig_slime_s_0.atlas  # 帧位置描述
│   └── ironclad/
│       ├── ironclad_0.png
│       ├── ironclad_0.atlas
│       └── ironclad_1.png        # 如帧数超出，拆分多张
├── characters/
│   └── ...
├── backgrounds/
│   └── ...
├── vfx/
│   └── ...
└── export_report.json            # 转换报告
```

---

## 7. 命令行接口

### 7.1 使用方式

```bash
# 基本用法：转换所有 Spine 动画
/spine-exporter

# 指定输入输出目录
/spine-exporter --input animations --output animations_spritesheet

# 只转换指定目录
/spine-exporter --filter "monsters/twig_slime_s"

# 自定义参数
/spine-exporter --fps 12 --sheet-size 4096

# 查看帮助
/spine-exporter --help
```

### 7.2 参数说明

| 参数 | 短参数 | 默认值 | 说明 |
|------|--------|--------|------|
| `--input` | `-i` | `animations` | 输入目录 |
| `--output` | `-o` | `animations_spritesheet` | 输出目录 |
| `--filter` | `-f` | 无 | 只处理匹配路径的动画 |
| `--fps` | | `8` | 帧率 |
| `--sheet-size` | `-s` | `2048` | 精灵表尺寸 |
| `--overwrite` | | `false` | 覆盖已存在的文件 |
| `--help` | `-h` | | 显示帮助信息 |

---

## 8. Atlas 文件格式

精灵表配套的 `.atlas` 文件采用文本格式，描述每个动画的每帧位置和尺寸：

```atlas
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
frames: 12
frame_00:
  bounds: 0, 0, 128, 96
frame_01:
  bounds: 128, 0, 128, 96
frame_02:
  bounds: 256, 0, 128, 96

[animation: attack]
loop: false
frames: 8
frame_00:
  bounds: 0, 96, 150, 120
```

**格式说明：**
- `[metadata]`: 元信息，包含源文件名、帧率、创建时间
- `[texture]`: 精灵表图片信息
- `[animation: name]`: 每个动画的定义
  - `loop`: 是否循环
  - `frames`: 总帧数
  - `frame_XX`: 每帧在精灵表中的位置（x, y, width, height）

---

## 9. 依赖项

### 9.1 NuGet 包

| 包名 | 版本 | 用途 |
|------|------|------|
| `spine-csharp` | 最新 | 官方 Spine C# Runtime，解析骨骼和动画 |
| `SixLabors.ImageSharp` | 3.x | 图像处理，生成精灵表 PNG |
| `System.CommandLine` | 2.x | 命令行参数解析 |

### 9.2 spine-csharp 获取方式

1. **官方源**（推荐）：从 [Spine 官网](http://esotericsoftware.com/spine-runtimes) 下载
2. **NuGet**：`Spine-CSharp` 非官方包
3. **GitHub**：克隆 [spine-runtimes](https://github.com/EsotericSoftware/spine-runtimes) 仓库

**注意**：需要确认项目现有 Spine GDExtension 版本与 spine-csharp 版本兼容。

---

## 10. 错误处理

### 10.1 错误类型与处理

| 情况 | 处理方式 | 级别 |
|------|----------|------|
| 找不到 .skel 文件 | 跳过，记录警告 | WARN |
| .atlas 或 .png 缺失 | 跳过，记录错误 | ERROR |
| 动画渲染失败 | 跳过该动画，继续处理其他 | ERROR |
| 精灵表超出尺寸 | 自动拆分多张 | INFO |
| 写入文件失败 | 报错并停止 | FATAL |

### 10.2 日志输出示例

```
[INFO] 开始扫描 animations/ 目录...
[INFO] 找到 158 个 Spine 文件

[INFO] 处理: animations/monsters/twig_slime_s/twig_slime_s.skel
[INFO]   - 动画: idle_loop (12帧)
[INFO]   - 动画: attack (8帧)
[INFO]   - 输出: animations_spritesheet/monsters/twig_slime_s/

[WARN] 跳过: animations/xxx/xxx.skel (缺少 .atlas 文件)

[INFO] 完成! 成功: 155, 失败: 3
[INFO] 详细报告: animations_spritesheet/export_report.json
```

### 10.3 导出报告格式

```json
{
  "timestamp": "2026-05-04T12:00:00Z",
  "summary": {
    "total": 158,
    "success": 155,
    "failed": 3
  },
  "details": [
    {
      "source": "animations/monsters/twig_slime_s/twig_slime_s.skel",
      "status": "success",
      "animations": ["idle_loop", "attack"],
      "frames": 20,
      "output": "animations_spritesheet/monsters/twig_slime_s/"
    },
    {
      "source": "animations/xxx/xxx.skel",
      "status": "failed",
      "error": "缺少 .atlas 文件"
    }
  ]
}
```

---

## 11. 限制与已知问题

1. **混合动画**：不支持导出动画混合/过渡效果，只导出单个动画的帧序列
2. **皮肤系统**：当前只导出默认皮肤，多皮肤角色需要分别处理
3. **帧率限制**：8 FPS 对于快速动画可能不够流畅，可能需要后续调整
4. **精灵表尺寸**：2048×2048 对于复杂角色可能不够，会自动拆分

---

## 12. 后续优化方向

1. **皮肤支持**：支持导出多个皮肤的动画
2. **并行处理**：多线程加速批量转换
3. **预览功能**：生成动画预览 GIF
4. **Godot 集成**：生成 Godot 可直接使用的 `.tres` 资源文件
