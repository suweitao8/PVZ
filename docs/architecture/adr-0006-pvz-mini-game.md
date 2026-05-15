# ADR-0006: PvZ Mini Game Architecture

## Status

Proposed

## Date

2026-05-15

## Last Verified

2026-05-15

## Decision Makers

User Request

## Summary

植物大战僵尸迷你游戏作为一个独立子系统存在，拥有自己的场景、实体系统和游戏循环。它通过主菜单按钮进入，与主线游戏完全解耦，共享引擎基础设施但不依赖核心游戏逻辑。

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Godot 4.5.1 |
| **Domain** | Gameplay / Mini Game |
| **Knowledge Risk** | LOW — 标准 Godot 节点和信号 |
| **References Consulted** | `docs/engine-reference/godot/VERSION.md` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | 触摸输入在移动端正常工作 |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (C# primary) |
| **Enables** | PvZ Mini Game |
| **Blocks** | None |
| **Ordering Note** | 独立子系统，可并行开发 |

## Context

### Problem Statement

需要在 Slay the Spire 2 主游戏中嵌入一个完整的塔防小游戏，要求：
1. 不影响主线游戏代码
2. 可独立开发和测试
3. 共享引擎基础设施（音频、输入、UI框架）
4. 支持键鼠和触摸输入

### Current State

- 主菜单已有 Singleplayer、Multiplayer 等按钮
- 游戏使用 Godot 4.5.1 + C#
- 输入系统支持键鼠、手柄、触摸

### Constraints

- 不能修改核心战斗系统
- 需要遵循现有的命名和代码规范
- 资源预算有限（迷你游戏）

### Requirements

- 5x9 网格布局
- 植物放置和攻击逻辑
- 僵尸生成和移动逻辑
- 阳光收集系统
- 波次管理
- 胜利/失败判定

## Decision

采用独立子系统架构：

### 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                     Main Menu                                │
│  [Singleplayer] [PvZ Mode] [Multiplayer] [Settings]         │
└─────────────────────┬───────────────────────────────────────┘
                      │ 点击 PvZ Mode 按钮
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   PvZ Game Scene                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ PvZGameManager (Autoload for scene)                  │   │
│  │  - GameState: Playing/Paused/Victory/Defeat          │   │
│  │  - SunCount: int                                      │   │
│  │  - CurrentWave: int                                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌───────────────┐  │
│  │ PvZGridManager │  │ PvZWaveManager │  │ PvZSunManager │  │
│  │  - 5x9 cells   │  │  - Wave data   │  │  - Sky drops  │  │
│  │  - Plant refs  │  │  - Spawn timer │  │  - Collection │  │
│  └────────────────┘  └────────────────┘  └───────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ PvZPlantPanel (UI)                                   │   │
│  │  - Plant selection buttons                           │   │
│  │  - Sun counter                                       │   │
│  │  - Wave counter                                      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                      │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
    ┌──────────┐ ┌──────────┐ ┌──────────┐
    │  Plants  │ │ Zombies  │ │ Projectiles│
    │  (Node2D)│ │ (Node2D) │ │  (Node2D) │
    └──────────┘ └──────────┘ └──────────┘
```

### 关键类设计

```csharp
// PvZGameManager.cs - 场景主控制器
public partial class PvZGameManager : Node
{
    public static PvZGameManager Instance { get; private set; }
    
    public PvZGameState State { get; private set; }
    public int SunCount { get; private set; }
    public int CurrentWave { get; private set; }
    public int TotalWaves { get; } = 10;
    
    public void AddSun(int amount);
    public bool TrySpendSun(int amount);
    public void StartNextWave();
    public void CheckVictoryCondition();
}

// PvZGridManager.cs - 网格管理
public partial class PvZGridManager : Node
{
    private readonly PvZCell[,] _cells = new PvZCell[5, 9];
    
    public bool TryPlacePlant(int row, int col, PlantType type);
    public void RemovePlant(int row, int col);
    public PvZCell GetCell(int row, int col);
}

// PlantBase.cs - 植物基类
public abstract partial class PlantBase : Node2D
{
    public abstract int SunCost { get; }
    public abstract float Cooldown { get; }
    protected int Row { get; }
    protected int Col { get; }
    
    public abstract void OnPlaced();
    public abstract void OnZombieInRow(ZombieBase zombie);
}

// ZombieBase.cs - 僵尸基类
public abstract partial class ZombieBase : Node2D
{
    public abstract int MaxHealth { get; }
    public abstract float MoveSpeed { get; }
    protected int Row { get; }
    protected float CurrentHealth { get; set; }
    
    public abstract void TakeDamage(int amount);
    public abstract void OnReachEnd();
}
```

### 实现指南

1. **场景结构**:
   - 创建 `scenes/pvz/pvz_game.tscn` 作为主场景
   - 网格使用 `GridContainer` 或自定义布局
   - 植物/僵尸使用 `Node2D` 子类

2. **信号通信**:
   ```csharp
   [Signal]
   public delegate void SunCollectedEventHandler(int amount);
   
   [Signal]
   public delegate void WaveStartedEventHandler(int waveNumber);
   
   [Signal]
   public delegate void PlantPlacedEventHandler(int row, int col, PlantType type);
   
   [Signal]
   public delegate void ZombieSpawnedEventHandler(int row, ZombieType type);
   ```

3. **输入处理**:
   - 使用 `_Input(InputEvent @event)` 处理点击
   - 支持触摸: `InputEventScreenTouch`
   - 支持鼠标: `InputEventMouseButton`

4. **更新循环**:
   ```csharp
   public override void _Process(double delta)
   {
       if (State != PvZGameState.Playing) return;
       
       UpdateSunDrops(delta);
       UpdatePlants(delta);
       UpdateZombies(delta);
       UpdateProjectiles(delta);
   }
   ```

5. **资源加载**:
   - 使用 `PreloadManager` 预加载 PvZ 资源
   - 植物/僵尸使用对象池减少 GC

## Alternatives Considered

### Alternative 1: 使用现有 Combat 系统

- **描述**: 复用 CombatManager 和 GameAction 队列
- **Pros**: 减少代码量，保持一致性
- **Cons**: 过度耦合，塔防逻辑与卡牌逻辑差异大
- **Estimated Effort**: 更高（重构现有系统）
- **Rejection Reason**: 战斗系统和塔防系统设计目标不同

### Alternative 2: GDScript 实现

- **描述**: 使用 GDScript 编写 PvZ 逻辑
- **Pros**: 快速原型，无需编译
- **Cons**: 与项目 C# 主要语言不一致，维护困难
- **Estimated Effort**: 类似
- **Rejection Reason**: 项目约定 C# 为主要语言

## Consequences

### Positive

- 完全独立，不影响主线游戏
- 可独立测试和迭代
- 清晰的代码边界

### Negative

- 需要额外的场景和资源
- 不能复用现有战斗系统的实体

### Neutral

- 遵循项目 C# 规范
- 使用 Godot 标准节点

## Risks

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| 触摸输入在移动端不准确 | Medium | Medium | 在移动设备上测试，调整点击检测区域 |
| 性能问题（大量实体） | Low | Medium | 使用对象池，限制最大实体数 |
| 资源占用过多内存 | Low | Low | 压缩贴图，复用材质 |

## Performance Implications

| Metric | Before | Expected After | Budget |
|--------|--------|---------------|--------|
| CPU (per frame) | N/A | ~2-5ms (100 entities) | <10ms |
| Memory | N/A | ~20-50MB (textures) | <100MB |
| Scene load time | N/A | <1s | <3s |

## Migration Plan

新功能，无需迁移。

## Validation Criteria

- [ ] PvZ 场景可独立运行
- [ ] 主菜单按钮正确切换到 PvZ 场景
- [ ] 所有植物正确实现
- [ ] 所有僵尸正确实现
- [ ] 波次系统正常工作
- [ ] 胜利/失败判定正确

## GDD Requirements Addressed

- `design/gdd/pvz-mini-game.md` — 全部需求

## Related

- ADR-0001: C# as Primary Language
- `design/gdd/pvz-mini-game.md` — 设计文档
