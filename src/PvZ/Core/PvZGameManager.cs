using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.PvZ.Entities.Plants;
using MegaCrit.Sts2.PvZ.Entities.Zombies;

namespace MegaCrit.Sts2.PvZ.Core;

/// <summary>
/// PvZ 迷你游戏主控制器
/// 管理游戏状态、阳光、波次等核心逻辑
/// </summary>
public partial class PvZGameManager : Node
{
    #region Singleton

    public static PvZGameManager Instance { get; private set; }

    #endregion

    #region Signals

    /// <summary>
    /// 阳光数量变化时触发
    /// </summary>
    [Signal]
    public delegate void SunChangedEventHandler(int currentSun);

    /// <summary>
    /// 波次开始时触发
    /// </summary>
    [Signal]
    public delegate void WaveStartedEventHandler(int waveNumber, int totalWaves);

    /// <summary>
    /// 游戏状态变化时触发
    /// </summary>
    [Signal]
    public delegate void GameStateChangedEventHandler(PvZGameState newState);

    /// <summary>
    /// 阳光从天空掉落时触发
    /// </summary>
    [Signal]
    public delegate void SunDroppedEventHandler(Vector2 position);

    #endregion

    #region Configuration

    /// <summary>
    /// 初始阳光数量
    /// </summary>
    [Export]
    public int InitialSun { get; set; } = 50;

    /// <summary>
    /// 总波次数
    /// </summary>
    [Export]
    public int TotalWaves { get; set; } = 10;

    /// <summary>
    /// 天空掉落阳光的基础间隔（秒）
    /// </summary>
    [Export]
    public float SunDropInterval { get; set; } = 7.5f;

    /// <summary>
    /// 天空掉落阳光的随机偏移范围（秒）
    /// </summary>
    [Export]
    public float SunDropRandomOffset { get; set; } = 2.5f;

    /// <summary>
    /// 每个天空阳光的值
    /// </summary>
    [Export]
    public int SkySunValue { get; set; } = 25;

    #endregion

    #region State

    public PvZGameState State { get; private set; } = PvZGameState.Preparing;
    public int SunCount { get; private set; }
    public int CurrentWave { get; private set; }

    private float _sunDropTimer;
    private float _nextSunDropTime;
    private readonly Random _random = new();

    #endregion

    #region References

    public PvZGridManager GridManager { get; private set; }
    public PvZWaveManager WaveManager { get; private set; }
    public Node2D ProjectileContainer { get; private set; }
    public Node2D SunContainer { get; private set; }

    #endregion

    #region Lifecycle

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            Log.Error("PvZGameManager already exists.");
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Ready()
    {
        SunCount = InitialSun;
        CurrentWave = 0;
        _sunDropTimer = 0f;
        _nextSunDropTime = GetNextSunDropTime();

        // 获取子节点引用
        GridManager = GetNode<PvZGridManager>("GridManager");
        WaveManager = GetNode<PvZWaveManager>("WaveManager");
        ProjectileContainer = GetNode<Node2D>("ProjectileContainer");
        SunContainer = GetNode<Node2D>("SunContainer");

        Log.Info($"[PvZ] Game initialized. Sun: {SunCount}, Waves: {TotalWaves}");
        EmitSignal(SignalName.SunChanged, SunCount);
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public override void _Process(double delta)
    {
        if (State != PvZGameState.Playing) return;

        UpdateSunDrops((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (State == PvZGameState.Paused) return;

        // 点击收集阳光
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            HandleClick(mouseButton.Position);
        }
        else if (@event is InputEventScreenTouch touch && touch.Pressed)
        {
            HandleClick(touch.Position);
        }
    }

    #endregion

    #region Sun Management

    /// <summary>
    /// 添加阳光
    /// </summary>
    public void AddSun(int amount)
    {
        if (amount <= 0) return;

        SunCount += amount;
        EmitSignal(SignalName.SunChanged, SunCount);
        Log.Info($"[PvZ] Sun +{amount}. Total: {SunCount}");
    }

    /// <summary>
    /// 尝试花费阳光
    /// </summary>
    public bool TrySpendSun(int amount)
    {
        if (SunCount < amount)
        {
            Log.Info($"[PvZ] Not enough sun. Need {amount}, have {SunCount}");
            return false;
        }

        SunCount -= amount;
        EmitSignal(SignalName.SunChanged, SunCount);
        return true;
    }

    private void UpdateSunDrops(float delta)
    {
        _sunDropTimer += delta;

        if (_sunDropTimer >= _nextSunDropTime)
        {
            DropSunFromSky();
            _sunDropTimer = 0f;
            _nextSunDropTime = GetNextSunDropTime();
        }
    }

    private void DropSunFromSky()
    {
        // 随机位置（在网格范围内）
        float x = _random.Next(100, 1700);
        float y = _random.Next(100, 400);
        Vector2 position = new Vector2(x, y);

        EmitSignal(SignalName.SunDropped, position);
        Log.Info($"[PvZ] Sun dropped at {position}");
    }

    private float GetNextSunDropTime()
    {
        return SunDropInterval + (float)(_random.NextDouble() * 2 - 1) * SunDropRandomOffset;
    }

    #endregion

    #region Wave Management

    /// <summary>
    /// 开始下一波
    /// </summary>
    public void StartNextWave()
    {
        if (CurrentWave >= TotalWaves)
        {
            TriggerVictory();
            return;
        }

        CurrentWave++;
        State = PvZGameState.Playing;

        EmitSignal(SignalName.WaveStarted, CurrentWave, TotalWaves);
        EmitSignal(SignalName.GameStateChanged, (int)State);

        WaveManager?.SpawnWave(CurrentWave);
        Log.Info($"[PvZ] Wave {CurrentWave}/{TotalWaves} started");
    }

    /// <summary>
    /// 波次完成回调
    /// </summary>
    public void OnWaveComplete()
    {
        if (CurrentWave >= TotalWaves)
        {
            TriggerVictory();
        }
        else
        {
            State = PvZGameState.Preparing;
            EmitSignal(SignalName.GameStateChanged, (int)State);
            Log.Info($"[PvZ] Wave {CurrentWave} complete. Prepare for next wave.");
        }
    }

    #endregion

    #region Game End

    /// <summary>
    /// 触发胜利
    /// </summary>
    public void TriggerVictory()
    {
        State = PvZGameState.Victory;
        EmitSignal(SignalName.GameStateChanged, (int)State);
        Log.Info("[PvZ] Victory!");
    }

    /// <summary>
    /// 触发失败
    /// </summary>
    public void TriggerDefeat()
    {
        State = PvZGameState.Defeat;
        EmitSignal(SignalName.GameStateChanged, (int)State);
        Log.Info("[PvZ] Defeat!");
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void Pause()
    {
        if (State == PvZGameState.Playing)
        {
            State = PvZGameState.Paused;
            EmitSignal(SignalName.GameStateChanged, (int)State);
            GetTree().Paused = true;
        }
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void Resume()
    {
        if (State == PvZGameState.Paused)
        {
            State = PvZGameState.Playing;
            EmitSignal(SignalName.GameStateChanged, (int)State);
            GetTree().Paused = false;
        }
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        GetTree().Paused = false;
        // 切换回主菜单场景
        // NGame.Instance.ReturnToMainMenu();
        GetTree().ChangeSceneToFile("res://scenes/screens/main_menu.tscn");
    }

    #endregion

    #region Input Handling

    private void HandleClick(Vector2 position)
    {
        // 检查是否点击了阳光
        // 阳光收集逻辑在 PvZSun 节点中处理
    }

    #endregion

    #region Plant Placement

    /// <summary>
    /// 尝试在指定位置放置植物
    /// </summary>
    public bool TryPlacePlant(int row, int col, PlantType type)
    {
        if (State != PvZGameState.Preparing && State != PvZGameState.Playing)
            return false;

        return GridManager?.TryPlacePlant(row, col, type) ?? false;
    }

    #endregion
}
