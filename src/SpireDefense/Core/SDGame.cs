using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 尖塔防卫战主控制器
/// 管理游戏状态、波次、能量系统
/// </summary>
public partial class SDGame : Control
{
    private const string ScenePath = "res://scenes/spire_defense/spire_defense_game.tscn";

    // 游戏状态
    private int _currentEnergy = 100;
    private int _currentWave = 0;
    private const int MaxWaves = 10;
    private bool _waveInProgress = false;

    // 子系统
    private SDGrid _grid;
    private SDHandArea _handArea;
    private SDWaveManager _waveManager;

    // UI 节点
    private Label _energyLabel;
    private Label _waveLabel;
    private Button _startWaveButton;

    // 事件
    public event Action<int> EnergyChanged;
    public event Action<int> WaveChanged;

    // 单例访问
    public static SDGame Instance { get; private set; }

    public int CurrentEnergy => _currentEnergy;
    public int CurrentWave => _currentWave;
    public bool WaveInProgress => _waveInProgress;
    public SDGrid Grid => _grid;

    public static SDGame Create()
    {
        var scene = GD.Load<PackedScene>(ScenePath);
        if (scene == null)
        {
            Log.Error($"[SDGame] Failed to load scene: {ScenePath}");
            return null;
        }
        return scene.Instantiate<SDGame>();
    }

    public override void _Ready()
    {
        Instance = this;
        Log.Info("[SDGame] Spire Defense game initialized");

        // 获取子节点
        _grid = GetNode<SDGrid>("%Grid");
        _handArea = GetNode<SDHandArea>("%HandArea");
        _waveManager = GetNode<SDWaveManager>("%WaveManager");

        _energyLabel = GetNode<Label>("%EnergyLabel");
        _waveLabel = GetNode<Label>("%WaveLabel");
        _startWaveButton = GetNode<Button>("%StartWaveButton");

        // 连接信号
        _startWaveButton.Connect(Button.SignalName.Pressed, Callable.From(OnStartWavePressed));

        // 初始化 UI
        UpdateEnergyDisplay();
        UpdateWaveDisplay();

        // 开始天空能量掉落
        StartEnergyDrop();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #region 能量系统

    public bool TrySpendEnergy(int amount)
    {
        if (_currentEnergy >= amount)
        {
            _currentEnergy -= amount;
            UpdateEnergyDisplay();
            EnergyChanged?.Invoke(_currentEnergy);
            return true;
        }
        return false;
    }

    public void AddEnergy(int amount)
    {
        _currentEnergy += amount;
        UpdateEnergyDisplay();
        EnergyChanged?.Invoke(_currentEnergy);
    }

    private void UpdateEnergyDisplay()
    {
        if (_energyLabel != null)
        {
            _energyLabel.Text = $"能量: {_currentEnergy}";
        }
    }

    private async void StartEnergyDrop()
    {
        var random = new Random();
        while (true)
        {
            // 等待 5-10 秒
            float delay = 5.0f + (float)random.NextDouble() * 5.0f;
            await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);

            // 生成能量球
            if (IsInstanceValid(this))
            {
                SpawnEnergyOrb(25);
            }
        }
    }

    private void SpawnEnergyOrb(int value)
    {
        // TODO: 创建能量球节点
        Log.Info($"[SDGame] Energy orb spawned: {value}");
    }

    #endregion

    #region 波次系统

    private void OnStartWavePressed()
    {
        if (_waveInProgress)
        {
            Log.Warn("[SDGame] Wave already in progress");
            return;
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        _currentWave++;
        _waveInProgress = true;
        UpdateWaveDisplay();
        WaveChanged?.Invoke(_currentWave);

        Log.Info($"[SDGame] Starting wave {_currentWave}");

        _waveManager?.StartWave(_currentWave);
    }

    public void OnWaveComplete()
    {
        _waveInProgress = false;
        Log.Info($"[SDGame] Wave {_currentWave} complete");

        if (_currentWave >= MaxWaves)
        {
            OnVictory();
        }
    }

    private void UpdateWaveDisplay()
    {
        if (_waveLabel != null)
        {
            _waveLabel.Text = $"波次: {_currentWave}/{MaxWaves}";
        }
    }

    #endregion

    #region 游戏结束

    private void OnVictory()
    {
        Log.Info("[SDGame] Victory!");
        // TODO: 显示胜利界面
    }

    public void OnDefeat()
    {
        Log.Info("[SDGame] Defeat!");
        // TODO: 显示失败界面
    }

    #endregion
}
