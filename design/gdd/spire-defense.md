# GDD: 尖塔防卫战 (Spire Defense)

> **Status**: In Development
> **System**: Mini Game / Tower Defense
> **Last Updated**: 2026-05-16
> **Dependencies**: Combat System, Card System, Spine Animation System

---

## 1. Overview

**尖塔防卫战**是一个借鉴《植物大战僵尸》玩法的塔防小游戏。玩家在网格上部署 STS2 的角色作为防守单位，抵御从右侧进攻的怪物波次。

**核心特色**：
- 🎮 **PvZ 玩法内核**：网格部署、资源管理、波次防守
- 🎨 **STS2 角色**：直接复用 Ironclad、Silent、Defect 等角色
- 🃏 **卡牌部署**：从手牌拖拽放置单位，复用 STS2 卡牌交互
- ⚔️ **怪物进攻**：复用 STS2 怪物作为进攻方

---

## 2. Player Fantasy

玩家在 STS2 的世界观中体验经典塔防的乐趣。操控熟悉的角色（Ironclad 挡在前线，Silent 远程输出，Defect 提供支援），抵御熟悉的怪物（Slaver、Gremlin、Cultist 等）的进攻。卡牌拖拽的交互方式让 STS2 玩家无缝上手。

---

## 3. Detailed Rules

### 3.1 游戏场地

```
┌──────────────────────────────────────────────────────────────┐
│  能量: 150        波次: 3/10        [开始波次]                │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                    5行 x 9列 网格                        │ │
│  │                                                         │ │
│  │  [Ironclad]  [Silent]          [Defect]                 │ │
│  │      🛡️         🏹                   ⚡                   │ │
│  │                    [攻击中的怪物 →]                      │ │
│  │                                    [Cultist] [Slaver]   │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  手牌区 (最大 5 张)                                           │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                    │
│  │Iron │ │Silent│ │Defect│ │Necro│ │Regent│   [抽卡: 25]   │
│  │ 50  │ │ 100 │ │ 75  │ │ 100 │ │ 150 │     [刷新: 50]   │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘                    │
│                                                              │
│  牌库: 35 张                                                 │
└──────────────────────────────────────────────────────────────┘
```

### 3.2 阵营划分

#### 防守方（玩家可部署单位）

**角色 → 防守单位映射**

| 单位名称 | 原型角色 | 能量消耗 | 定位 | 攻击方式 | 特殊能力 |
|---------|---------|----------|------|---------|---------|
| **铁卫** | Ironclad (铁甲) | 50 | 坦克 | 近战挥砍 | 高血量，可格挡 |
| **影弓** | Silent (安静者) | 100 | 远程 | 飞刀射击 | 攻击叠加中毒 |
| **构造体** | Defect (缺陷体) | 75 | 法师 | 电弧攻击 | 充能后爆发 |
| **死灵师** | Necrobinder (死灵绑定者) | 100 | 召唤 | 召唤骷髅 | 死亡单位复活 |
| **摄政王** | Regent (摄政) | 150 | 精英 | 剑气斩击 | 强化相邻单位 |
| **守护者** | Guardian（自定义） | 25 | 基础 | 普通攻击 | 低费基础单位 |

#### 进攻方（怪物波次）

**怪物 → 进攻单位映射**

| 单位名称 | 原型怪物 | 生命值 | 速度 | 攻击方式 | 特殊能力 |
|---------|---------|--------|------|---------|---------|
| **教徒** | Cultist | 80 | 慢 | 远程 | 召唤仪式强化 |
| **哥布林** | Gremlin | 40 | 快 | 近战 | 死亡分裂 |
| **奴役者** | Slaver | 150 | 中 | 近战 | 奴役低血量单位 |
| **真菌** | Fungi Beast | 100 | 慢 | 孢子 | 死亡释放毒素 |
| **拉加** | Lagavulin | 300 | 慢 | 震荡波 | 高护甲 |
| **六角** | Hexaghost | 500 | 中 | 多段攻击 | Boss 级别 |
| **史莱姆** | Slime | 60 | 中 | 酸液 | 分裂 |
| **蛇女** | Ssserpent | 200 | 中 | 毒牙 | 中毒攻击 |

### 3.3 核心玩法（借鉴 PvZ）

#### 资源系统：能量

- **初始能量**: 100
- **天空掉落**: 每 5-10 秒掉落能量球 (25)
- **能量产出单位**: 特定单位可产出额外能量
- **用途**: 抽卡、放置单位

#### 卡牌部署系统

**卡牌来源**：
- 牌库包含所有防守单位卡牌
- 开局抽取 5 张手牌
- 可花费能量抽卡 (25) 或刷新手牌 (50)

**部署流程**：
1. 从手牌区拖拽卡牌
2. 显示单位预览（Spine 动画）
3. 网格高亮显示可放置位置
4. 释放鼠标放置单位
5. 消耗能量，触发冷却

#### 波次系统

**波次结构**：
- 共 10 波，难度递增
- 每波包含多个怪物组合
- 波次间隙可部署/调整

**怪物生成规则**：
```
怪物数量 = 波次编号 × 2 + 基础数量
精英概率 = 波次编号 / 总波次
Boss波次 = 第 5 波、第 10 波
```

### 3.4 单位行为

#### 防守单位行为

| 状态 | 行为 |
|------|------|
| **待机** | 播放 idle 动画，检测范围内敌人 |
| **攻击** | 进入攻击范围时，播放 attack 动画，造成伤害 |
| **受击** | 被攻击时播放 hit 动画，扣减生命 |
| **死亡** | 生命归零时播放 death 动画，从网格移除 |

#### 进攻单位行为

| 状态 | 行为 |
|------|------|
| **移动** | 向左移动，播放 walk 动画 |
| **攻击** | 遇到防守单位时停止，播放 attack 动画 |
| **特殊** | 根据怪物类型触发特殊能力 |

### 3.5 战斗机制

**攻击优先级**：
1. 同行最近的敌人
2. 同行任意敌人
3. 相邻行的敌人

**伤害计算**（复用 STS2 公式）：
```
实际伤害 = 基础伤害 - 护甲 (最小为 1)
暴击伤害 = 基础伤害 × 1.5 (特殊触发)
```

**状态效果**（复用 STS2 Powers）：
- 中毒 (Poison): 每回合扣血
- 虚弱 (Weak): 伤害降低 25%
- 易伤 (Vulnerable): 受到伤害增加 50%
- 力量 (Strength): 伤害增加

### 3.6 游戏流程

```
┌─────────────────────────────────────────────────────┐
│                    游戏开始                          │
│                    ↓                                │
│            ┌─────────────────┐                      │
│            │   准备阶段      │ ←──────────┐        │
│            │  - 抽卡/刷新    │             │        │
│            │  - 部署单位     │             │        │
│            └────────┬────────┘             │        │
│                     ↓                      │        │
│            ┌─────────────────┐             │        │
│            │   波次进行      │             │        │
│            │  - 怪物移动     │             │        │
│            │  - 单位攻击     │             │        │
│            │  - 能量获取     │             │        │
│            └────────┬────────┘             │        │
│                     ↓                      │        │
│              ┌─────────────┐               │        │
│              │ 波次结束?   │─── 否 ───────┘        │
│              └──────┬──────┘                       │
│               是 ↓                                │
│              ┌─────────────┐                       │
│              │ 全部波次?   │─── 否 ──→ 下一波      │
│              └──────┬──────┘                       │
│               是 ↓                                │
│              ┌─────────────┐                       │
│              │   胜利！    │                       │
│              └─────────────┘                       │
│                                                     │
│              失败条件：怪物到达最左侧               │
└─────────────────────────────────────────────────────┘
```

---

## 4. Formulas

### 能量产出

```
天空能量间隔 = 基础间隔 (7.5s) + 随机偏移 (0-5s)
能量球价值 = 25
```

### 波次难度

```
怪物数量 = floor(波次编号 × 1.5) + 3
怪物生命加成 = 1 + (波次编号 - 1) × 0.1
精英出现波次 = 3, 5, 7, 10
```

### 抽卡概率

```
普通单位: 60% (Ironclad, Silent, Defect)
精英单位: 25% (Necrobinder, Regent)
特殊单位: 15% (事件限定单位)
```

### 伤害公式

```
基础伤害 = 单位攻击力 × 力量加成
实际伤害 = max(基础伤害 - 护甲, 1)
中毒伤害 = 中毒层数 (每回合)
```

---

## 5. Edge Cases

| 情况 | 处理方式 |
|------|----------|
| 格子已有单位 | 红色边框，不可放置 |
| 能量不足 | 红色边框 + "能量不足" 提示 |
| 手牌已满 | 提示 "手牌已满" |
| 牌库已空 | 提示 "牌库已空"，可重洗 |
| 单位被击杀 | 播放死亡动画，格子清空 |
| 怪物突破防线 | 游戏失败 |
| Boss 波次 | 特殊提示，增加奖励 |
| 网络断开 | 单机模式不涉及 |

---

## 6. Dependencies

| 系统 | 依赖程度 | 复用内容 |
|------|----------|---------|
| **Combat System** | 高 | NCreature、伤害计算、状态效果 |
| **Card System** | 高 | NCardPlay、NMouseCardPlay、拖拽逻辑 |
| **Spine Animation** | 高 | MegaSpine 绑定、角色动画 |
| **VFX System** | 中 | 攻击、命中、死亡特效 |
| **Audio System** | 中 | FMOD 音效 |
| **Input System** | 高 | 鼠标拖拽交互 |
| **Save System** | 低 | 可选：保存最高分 |

---

## 7. Tuning Knobs

| 参数 | 默认值 | 调整范围 | 说明 |
|------|--------|----------|------|
| 初始能量 | 100 | 50-200 | 影响开局节奏 |
| 手牌上限 | 5 | 3-7 | 影响策略选择 |
| 抽卡消耗 | 25 | 10-50 | 影响资源管理 |
| 刷新消耗 | 50 | 25-100 | 影响风险收益 |
| 总波次数 | 10 | 5-20 | 影响游戏时长 |
| 天空能量频率 | 7.5秒 | 5-15秒 | 影响资源获取 |
| 单位攻击间隔 | 1.0秒 | 0.5-2.0秒 | 影响战斗节奏 |

---

## 8. Acceptance Criteria

### 基础功能

- [ ] 主菜单显示 "Spire Defense" 按钮
- [ ] 点击按钮进入游戏场景
- [ ] 5x9 网格正确渲染
- [ ] 手牌区显示卡牌
- [ ] 拖拽卡牌显示单位预览
- [ ] 释放鼠标正确放置单位
- [ ] 单位使用 Spine 动画

### 战斗功能

- [ ] 怪物从右侧按波次生成
- [ ] 怪物向左移动
- [ ] 单位自动攻击范围内怪物
- [ ] 怪物攻击相邻单位
- [ ] 伤害计算正确
- [ ] 状态效果生效
- [ ] 死亡动画播放

### 系统功能

- [ ] 能量正常获取
- [ ] 抽卡系统正常工作
- [ ] 波次系统正常运作
- [ ] 胜利/失败判定正确
- [ ] 可返回主菜单

---

## 9. UI Requirements

### 主菜单

- 在 Singleplayer 按钮上方添加 "Spire Defense" 按钮

### 游戏界面

见 3.1 游戏场地布局

### 拖拽反馈

- 卡牌拾起：放大 1.2x
- 网格高亮：绿色 = 可放置，红色 = 不可放置
- 单位预览：半透明 Spine 动画
- 费用显示：单位能量消耗

---

## 10. Technical Notes

### 场景结构

```
scenes/
  spire_defense/
    spire_defense_game.tscn    # 主游戏场景
    sd_grid.tscn               # 网格场景
    sd_hand_area.tscn          # 手牌区
    sd_card.tscn               # 防守卡牌
    units/
      sd_unit_base.tscn        # 单位基类（Spine节点）
    monsters/
      sd_monster_base.tscn     # 怪物基类（Spine节点）
```

### 代码结构

```
src/
  SpireDefense/
    Core/
      SDGame.cs                # 游戏主控制器
      SDGrid.cs                # 网格管理
      SDWaveManager.cs         # 波次管理
      SDCardDeck.cs            # 牌库管理
      SDHandArea.cs            # 手牌区管理
      SDEnergySystem.cs        # 能量系统
    Entities/
      Units/
        SDUnitBase.cs          # 防守单位基类
        SDIroncladUnit.cs      # Ironclad 单位
        SDSilentUnit.cs        # Silent 单位
        SDDefectUnit.cs        # Defect 单位
        ...
      Monsters/
        SDMonsterBase.cs       # 进攻怪物基类
        SDCultistMonster.cs    # Cultist 怪物
        SDGremlinMonster.cs    # Gremlin 怪物
        ...
    UI/
      SDCard.cs                # 防守卡牌（可拖拽）
      SDCardPreview.cs         # 拖拽预览
      SDWaveIndicator.cs       # 波次指示器
```

### 复用现有系统

**NCreature 复用**：
```csharp
// 防守单位继承 NCreature 的渲染逻辑
public class SDUnitBase : Control
{
    protected Creature Entity { get; set; }
    protected NCreatureVisuals Visuals { get; set; }

    // 复用 Spine 动画系统
    public void PlayAnimation(string animName)
    {
        Visuals.SpineBody?.PlayAnimation(animName, true);
    }
}
```

**卡牌拖拽复用**：
```csharp
// 复用 NCardPlay 的拖拽逻辑
public class SDCard : NCardPlay
{
    protected override void OnDragStart()
    {
        // 显示单位预览
        ShowUnitPreview();
    }

    protected override void OnDragEnd(Vector2 position)
    {
        // 放置单位到网格
        TryPlaceUnit(position);
    }
}
```

**伤害系统复用**：
```csharp
// 复用 Creature 和 DamageResult
public class SDCombat
{
    public void DealDamage(Creature attacker, Creature target, int damage)
    {
        var result = DamageResult.Calculate(attacker, target, damage);
        target.TakeDamage(result.Damage);
        // 播放 VFX
    }
}
```

---

## 11. Localization

| Key | English | 中文 |
|-----|---------|------|
| SPIRE_DEFENSE | Spire Defense | 尖塔防卫战 |
| SD_ENERGY | Energy | 能量 |
| SD_WAVE | Wave | 波次 |
| SD_DRAW | Draw | 抽卡 |
| SD_REFRESH | Refresh | 刷新 |
| SD_HAND_FULL | Hand is full! | 手牌已满！ |
| SD_DECK_EMPTY | Deck is empty! | 牌库已空！ |
| SD_VICTORY | Victory! | 胜利！ |
| SD_DEFEAT | Defeat | 失败 |
| SD_START_WAVE | Start Wave | 开始波次 |

---

## 12. 开发计划

### Phase 1: 基础框架 (MVP)

1. 创建游戏场景和网格系统
2. 实现卡牌拖拽放置
3. 基础单位放置（Ironclad）
4. 基础怪物生成（Cultist）
5. 简单攻击逻辑

### Phase 2: 核心玩法

1. 完整卡牌抽取系统
2. 能量系统
3. 波次系统
4. 多种单位/怪物
5. 状态效果

### Phase 3: 打磨

1. 完整动画支持
2. VFX 特效
3. 音效
4. UI 美化
5. 平衡调整

---

## 13. Related Documents

- `design/gdd/combat-system.md` — 伤害计算、状态效果
- `docs/architecture/adr-0003-megaspine-binding-pattern.md` — Spine 动画绑定
- `src/Core/Nodes/Combat/NCardPlay.cs` — 卡牌拖拽参考
- `src/Core/Nodes/Combat/NCreature.cs` — 生物渲染参考
- `src/Core/Entities/Creatures/Creature.cs` — 生物实体参考
