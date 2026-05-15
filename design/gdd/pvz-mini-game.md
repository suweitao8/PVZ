# GDD: 植物大战僵尸迷你游戏 (PvZ Mini Game)

> **Status**: In Development
> **System**: Mini Game / PvZ Mode
> **Last Updated**: 2026-05-16
> **Dependencies**: Main Menu System, Combat System (card drag), Spine Animation System

---

## 1. Overview

植物大战僵尸迷你游戏是一个塔防小游戏，玩家通过卡牌抽取系统获取植物卡牌，拖拽放置到网格上抵御僵尸进攻。作为 Slay the Spire 2 的彩蛋小游戏，它复用了 STS2 的角色 Spine 动画和卡牌拖拽系统，提供了熟悉的视觉风格和流畅的交互体验。

**核心特色**:
- 🎨 复用 STS2 角色 Spine 动画（Ironclad、Silent、怪物等）
- 🃏 卡牌抽取系统，从手牌区拖拽放置
- 🎮 角色预览反馈，拖拽时显示动画预览

---

## 2. Player Fantasy

玩家想要一个轻松有趣的塔防体验，同时感受到 STS2 熟悉的视觉风格和交互手感。看到熟悉的角色（如 Ironclad、Silent）化身植物和僵尸，产生惊喜和代入感。卡牌拖拽放置保持了 STS2 的核心交互习惯。

---

## 3. Detailed Rules

### 3.1 游戏场地

- **网格布局**: 5 行 x 9 列的经典草坪布局
- **阳光系统**: 玩家收集阳光作为资源
- **波次系统**: 僵尸分波次进攻，每波难度递增
- **视觉分区**:
  - 顶部: 阳光数量 + 波次进度 + 开始波次按钮
  - 主区域: 5x9 网格草坪
  - 底部: 手牌区 + 抽卡按钮

### 3.2 角色映射系统

#### 植物角色映射（使用 STS2 角色 Spine 动画）

| 植物类型 | 映射角色 | 阳光消耗 | 效果 | 动画复用 |
|---------|---------|----------|------|---------|
| 向日葵 | Silent (安静者) | 50 | 每 24 秒产出 50 阳光 | idle（站桩）、attack（产阳光动作） |
| 豌豆射手 | Ironclad (铁甲) | 100 | 射击豌豆 (伤害 20) | idle（待机）、attack（射击） |
| 坚果墙 | Battleworn Dummy (训练假人) | 50 | 阻挡僵尸 (生命 400) | idle（站立）、hit（被击摇晃） |
| 寒冰射手 | Defect (缺陷体) | 175 | 射击冰豌豆，减速 5 秒 | idle、attack（带冰霜 VFX） |
| 樱桃炸弹 | Gas Bomb (瓦斯弹) | 150 | 范围爆炸 (伤害 1800) | idle、attack（爆炸 VFX） |
| 双发射手 | Magi Knight (魔法骑士) | 200 | 双倍射速 (伤害 20x2) | idle、attack（双连击） |
| 大嘴花 | Chomper (吞噬者) | 150 | 一口吃掉僵尸，咀嚼 30 秒 | idle、attack（吞噬） |

#### 僵尸角色映射

| 僵尸类型 | 映射角色 | 生命值 | 速度 | 特殊能力 | 动画复用 |
|---------|---------|--------|------|----------|---------|
| 普通僵尸 | Sneaky Gremlin (潜行哥布林) | 100 | 慢 (20px/s) | 无 | walk、attack、hit、death |
| 路障僵尸 | Tough Egg (硬蛋) | 200 | 慢 (20px/s) | 头戴路障 | walk、attack、hit（护甲） |
| 铁桶僵尸 | Living Shield (活盾) | 400 | 慢 (20px/s) | 头戴铁桶 | walk、attack、hit（重甲） |
| 跑步僵尸 | Thieving Hopper (跳跃窃贼) | 100 | 快 (60px/s) | 冲刺，遇植物减速 | run、attack、hit |
| 巨人僵尸 | Bygone Effigy (远古雕像) | 1000 | 慢 (15px/s) | 投掷小僵尸 | walk、attack（投掷） |

### 3.3 卡牌抽取系统

#### 基础规则

- **牌库**: 初始包含所有 7 种植物卡牌，每种各 N 张（可配置）
- **手牌上限**: 5 张
- **初始手牌**: 开局抽取 5 张
- **抽卡消耗**: 25 阳光/次
- **刷新手牌**: 50 阳光，弃掉所有手牌重新抽取 5 张
- **冷却系统**: 每张卡牌放置后有独立冷却时间

#### 抽卡流程

1. 玩家点击 [抽卡: 25阳光] 按钮
2. 消耗 25 阳光
3. 从牌库随机抽取 1 张卡牌加入手牌
4. 如果手牌已满(5张)，提示"手牌已满"
5. 牌库空时，提示"牌库已空"

#### 卡牌放置流程

1. 玩家从手牌区**拖拽**卡牌
2. 卡牌放大并跟随鼠标
3. 在网格上显示**角色预览**（半透明 Spine 动画）
4. 显示放置指示：
   - ✅ 绿色边框 = 可放置（阳光足够 + 格子为空 + 冷却完成）
   - ❌ 红色边框 = 不可放置
5. 释放鼠标放置植物
6. 消耗阳光，触发冷却

### 3.4 游戏流程

1. **初始状态**: 100 阳光，手牌 5 张，无波次进行
2. **准备阶段**: 玩家可抽卡、刷新手牌、放置植物
3. **开始波次**: 点击 "Start Wave" 按钮
4. **战斗阶段**: 僵尸从右侧进入，植物自动攻击
5. **波次递进**: 每波结束后短暂准备时间
6. **胜利条件**: 击败所有 10 波僵尸
7. **失败条件**: 任何僵尸到达最左侧

### 3.5 阳光机制

- **初始阳光**: 100（提高到配合抽卡系统）
- **天空掉落**: 每 5-10 秒掉落一个阳光 (25)
- **向日葵产出**: 每 24 秒产出 50 阳光
- **抽卡消耗**: 25 阳光/次
- **刷新手牌**: 50 阳光

### 3.6 特效复用

从 STS2 复用的 VFX：

| 效果类型 | VFX 来源 | 用途 |
|---------|---------|------|
| 射击特效 | 攻击弹道 VFX | 豌豆、冰豌豆飞行 |
| 命中特效 | 伤害数字 VFX | 伤害显示 |
| 爆炸特效 | AOE 攻击 VFX | 樱桃炸弹爆炸 |
| 冰霜特效 | 冰冻状态 VFX | 寒冰射手减速效果 |
| 阳光特效 | 能量获取 VFX | 阳光掉落、收集 |

---

## 4. Formulas

### 伤害计算

```
植物伤害 = 基础伤害 * 等级加成
僵尸实际伤害 = 植物伤害 - 护甲 (最小为1)
```

### 阳光产出

```
天空阳光间隔 = 基础间隔 + 随机偏移(0-5秒)
向日葵产出间隔 = 24秒
```

### 波次难度

```
每波僵尸数量 = 波次编号 * 2 + 基础数量
僵尸类型概率 = 波次编号 / 总波次数
```

### 抽卡概率

```
基础概率: 每种植物均等概率 (1/7)
可配置权重系统用于调整稀有度
```

---

## 5. Edge Cases

| 情况 | 处理方式 |
|------|----------|
| 格子已有植物 | 红色边框提示，不可放置 |
| 阳光不足 | 红色边框 + "阳光不足" 提示 |
| 手牌已满抽卡 | 提示 "手牌已满"，不消耗阳光 |
| 牌库已空 | 提示 "牌库已空" |
| 所有植物被摧毁 | 游戏继续，可重新抽卡放置 |
| 僵尸到达左侧 | 游戏失败，显示结算画面 |
| 卡牌冷却中 | 显示冷却倒计时，灰色遮罩 |

---

## 6. Dependencies

| 系统 | 依赖程度 | 复用内容 |
|------|----------|----------|
| Main Menu System | 高 | 入口按钮、场景切换 |
| Card System | 高 | NCardPlay、NMouseCardPlay 拖拽逻辑 |
| Spine Animation | 高 | MegaSpine 绑定、动画播放 |
| VFX System | 中 | 攻击、命中、爆炸特效 |
| Audio System | 中 | FMOD 音效框架 |
| Input System | 高 | 鼠标拖拽交互 |

---

## 7. Tuning Knobs

| 参数 | 默认值 | 调整范围 | 说明 |
|------|--------|----------|------|
| 初始阳光 | 100 | 50-200 | 影响开局策略 |
| 初始手牌 | 5 | 3-7 | 影响开局选择 |
| 手牌上限 | 5 | 3-7 | 影响策略深度 |
| 抽卡消耗 | 25 | 10-50 | 影响资源平衡 |
| 刷新消耗 | 50 | 25-100 | 影响风险收益 |
| 总波次数 | 10 | 5-20 | 影响游戏时长 |
| 天空阳光频率 | 7.5秒 | 5-15秒 | 影响资源获取 |

---

## 8. Acceptance Criteria

- [ ] 主菜单显示 "PvZ Mode" 按钮
- [ ] 点击按钮进入 PvZ 游戏场景
- [ ] 5x9 网格正确渲染
- [ ] 手牌区显示卡牌（带 Spine 角色预览）
- [ ] 从手牌拖拽卡牌时显示角色预览
- [ ] 拖拽时网格显示可/不可放置指示
- [ ] 释放鼠标正确放置植物
- [ ] 植物使用 Spine 动画（idle、attack）
- [ ] 僵尸使用 Spine 动画（walk、attack、death）
- [ ] 阳光正常掉落和收集
- [ ] 抽卡系统正常工作
- [ ] 波次系统正常运作
- [ ] 胜利/失败结算画面正常显示

---

## 9. UI Requirements

### 主菜单

- 在 Singleplayer 按钮上方添加 "PvZ Mode" 按钮
- 按钮样式与现有按钮一致

### 游戏界面

```
┌─────────────────────────────────────────────────────────┐
│  阳光: 150         波次: 3/10         [Start Wave]       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │              5x9 网格草坪                        │   │
│  │         [Ironclad]  [Silent]  [Defect]          │   │
│  │            🌱          🌻          ❄️            │   │
│  │         (拖拽时显示角色 Spine 预览)               │   │
│  │                                                  │   │
│  │                              [Sneaky Gremlin] 🧟│   │
│  │                              [Tough Egg]     🧟│   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
├─────────────────────────────────────────────────────────┤
│  手牌区 (最大 5 张)                                      │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐              │
│  │Iron │ │Silent│ │Dummy│ │Def │ │Bomb │  [抽卡:25]   │
│  │ 100 │ │ 50  │ │ 50  │ │175 │ │150 │   [刷新:50]   │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘              │
│                                                         │
│  牌库剩余: 42 张                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 10. Technical Notes

### 场景结构

```
scenes/
  pvz/
    pvz_game.tscn          # 主游戏场景
    pvz_grid.tscn          # 网格场景
    pvz_hand_area.tscn     # 手牌区
    pvz_card.tscn          # PvZ卡牌（含Spine预览）
    plants/
      pvz_plant_base.tscn  # 植物基类（Spine节点）
    zombies/
      pvz_zombie_base.tscn # 僵尸基类（Spine节点）
```

### 代码结构

```
src/
  PvZ/
    Core/
      PvZGame.cs           # 游戏主控制器
      PvZGrid.cs           # 网格管理
      PvZWaveManager.cs    # 波次管理
      PvZCardDeck.cs       # 牌库管理
      PvZHandArea.cs       # 手牌区管理
    Entities/
      Plants/
        PvZPlantBase.cs    # 植物基类（Spine动画控制）
        PvZSunflower.cs    # 映射 Silent
        PvZPeashooter.cs   # 映射 Ironclad
        ...
      Zombies/
        PvZZombieBase.cs   # 僵尸基类（Spine动画控制）
        PvZBasicZombie.cs  # 映射 Sneaky Gremlin
        ...
    UI/
      PvZCard.cs           # PvZ 卡牌（可拖拽）
      PvZCardPreview.cs    # 拖拽时的角色预览
      PvZDeckDisplay.cs    # 牌库显示
```

### Spine 动画复用

```csharp
// 植物动画映射示例
public class PvZPeashooter : PvZPlantBase
{
    // 复用 Ironclad 的 Spine 数据
    private const string SpinePath = "res://animations/characters/ironclad/ironclad_skel_data.tres";

    protected override void PlayIdleAnimation()
    {
        _spine.AnimationState.SetAnimation(0, "idle", true);
    }

    protected override void PlayAttackAnimation()
    {
        _spine.AnimationState.SetAnimation(0, "attack", false);
        _spine.AnimationState.AddAnimation(0, "idle", true, 0);
    }
}
```

---

## 11. Localization

| Key | English | 中文 |
|-----|---------|------|
| PVZ_MODE | PvZ Mode | 植物大战僵尸 |
| PVZ_SUN | Sun | 阳光 |
| PVZ_WAVE | Wave | 波次 |
| PVZ_DRAW | Draw Card | 抽卡 |
| PVZ_REFRESH | Refresh | 刷新 |
| PVZ_HAND_FULL | Hand is full! | 手牌已满！ |
| PVZ_DECK_EMPTY | Deck is empty! | 牌库已空！ |
| PVZ_VICTORY | Victory! | 胜利！ |
| PVZ_DEFEAT | Defeat | 失败 |
| PVZ_RETURN_MENU | Return to Menu | 返回主菜单 |

---

## 12. Related Documents

- `design/gdd/combat-system.md` — 复用伤害计算逻辑
- `docs/architecture/adr-0003-megaspine-binding-pattern.md` — Spine 动画绑定模式
- `src/Core/Nodes/Combat/NCardPlay.cs` — 卡牌拖拽参考
- `src/Core/Nodes/Combat/NMouseCardPlay.cs` — 鼠标拖拽逻辑参考
