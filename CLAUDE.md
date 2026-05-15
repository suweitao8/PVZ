# Slay the Spire 2 — Claude Code Game Studios 配置

独立游戏开发，由 49 个协作的 Claude Code 子代理管理。
每个代理拥有特定领域，强制关注点分离和质量。

## 项目信息

- **游戏**: Slay the Spire 2 (杀戮尖塔2)
- **类型**: Roguelike 卡牌构建游戏
- **引擎**: Godot 4.5
- **语言**: C# (主要), GDScript (工具)

## 技术栈

- **引擎**: Godot 4.5
- **语言**: C# (.NET 9.0)
- **渲染**: Forward+ (Windows 上使用 D3D12)
- **物理**: 禁用 (Dummy)
- **版本控制**: Git，主干开发模式
- **音频**: FMOD
- **动画**: Spine (通过 MegaSpine 绑定)
- **崩溃报告**: Sentry

## 项目结构

@.claude/docs/directory-structure.md

## 引擎版本参考

@docs/engine-reference/godot/VERSION.md

## 技术偏好

@.claude/docs/technical-preferences.md

## 协调规则

@.claude/docs/coordination-rules.md

## 协作协议

**启用自动执行模式。**
Claude 拥有完整权限，可以直接执行任务，无需请求批准。

- 直接写入/编辑文件，无需询问
- 直接执行 Bash 命令
- 直接派发代理和委托任务
- 用户可随时打断或重定向

**自动提交规则：**
完成每个需求后，必须按以下顺序执行：

1. **编译验证**：运行 `dotnet build` 确保编译通过
2. **自动提交**：编译通过后自动 git 提交
   - `git add` 暂存本次需求相关的文件（不要 `git add -A`，排除无关文件）
   - `git commit` 使用 Conventional Commits 格式提交（`feat:` / `fix:` / `refactor:` / `docs:` 等）
   - 提交信息用中文描述变更内容
3. **编译失败处理**：如果编译失败，必须先修复错误，再提交

**编译验证是强制性的，不可跳过。**

## 编码标准

@.claude/docs/coding-standards.md

## 上下文管理

@.claude/docs/context-management.md

## 现有插件

本项目使用以下插件，应予以保留：

- `addons/fmod/` — FMOD 音频中间件集成
- `addons/sentry/` — 崩溃报告
- `addons/mega_text/` — 文本渲染系统
- `addons/atlas_generator/` — 图集生成工具
- `addons/dev_tools/` — 开发工具
- `addons/megacontentcreator/` — 内容创作工具

## 自定义技能

本项目在 `.claude/skills/` 中包含自定义技能：

- `spine-exporter/` — Spine 动画精灵表导出器

## 受限文件

**永远不要读取或访问以下文件：**

- `D:\Github\PVZ\提示词.md` — 用户的提示词起草工作区，私密且禁止访问

## 语言要求

**必须使用中文进行所有交流和文档输出。**
