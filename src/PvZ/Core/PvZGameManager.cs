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
    /// 显示浮动文字
    /// </summary>
    [Signal]
    public delegate void ShowFloatingTextEventHandler(Vector2 position, string text, Color color);

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
    /// 自动增加阳光的间隔（秒）
    /// </summary>
    [Export]
    public float AutoSunInterval { get; set; } = 10.0f;

    /// <summary>
    /// 每次自动增加的阳光数量
    /// </summary>
    [Export]
    public int AutoSunAmount { get; set; } = 25;

    #endregion

    #region State

    public PvZGameState State { get; private set; } = PvZGameState.Preparing;
    public int SunCount { get; private set; }
    public int CurrentWave { get; private set; }

    private float _autoSunTimer;

    #endregion

    #region References

    public PvZGridManager GridManager { get; private set; }
    public PvZWaveManager WaveManager { get; private set; }
    public Node2D ProjectileContainer { get; private set; }
    public Node2D SunContainer { get; private set; }
    public Node2D FloatingTextContainer { get; private set; }

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
        _autoSunTimer = 0f;

        // 获取子节点引用
        GridManager = GetNode<PvZGridManager>("GridManager");
        WaveManager = GetNode<PvZWaveManager>("WaveManager");
        ProjectileContainer = GetNode<Node2D>("ProjectileContainer");
        SunContainer = GetNode<Node2D>("SunContainer");
        FloatingTextContainer = GetNode<Node2D>("FloatingTextContainer");

        Log.Info($"[PvZ] Game initialized. Sun: {SunCount}, Waves: {TotalWaves}");
        EmitSignal(SignalName.SunChanged, SunCount);
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public override void _Process(double delta)
    {
        if (State != PvZGameState.Playing && State != PvZGameState.Preparing) return;

        // 自动增加阳光
        _autoSunTimer += (float)delta;
        if (_autoSunTimer >= AutoSunInterval)
        {
            AddSun(AutoSunAmount);
            _autoSunTimer = 0f;
            Log.Info($"[PvZ] Auto sun +{AutoSunAmount}. Total: {SunCount}");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (State == PvZGameState.Paused) return;
        // 点击处理由各个子节点自行处理
    }

    #endregion

    #region Sun Management

    /// <summary>
    /// 添加阳光，并在指定位置显示浮动文字
    /// </summary>
    public void AddSun(int amount, Vector2? floatTextPos = null)
    {
        if (amount <= 0) return;

        SunCount += amount;
        EmitSignal(SignalName.SunChanged, SunCount);

        // 显示浮动文字
        if (floatTextPos != null)
        {
            EmitSignal(SignalName.ShowFloatingText, floatTextPos.Value, $"+{amount} ☀", new Color(1.0f, 0.85f, 0.0f));
        }

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
        GetTree().ChangeSceneToFile("res://scenes/screens/main_menu.tscn");
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
