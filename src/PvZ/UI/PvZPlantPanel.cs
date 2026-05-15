using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.PvZ.Core;

namespace MegaCrit.Sts2.PvZ.UI;

/// <summary>
/// PvZ 植物选择面板
/// 显示可选植物和阳光数量
/// </summary>
public partial class PvZPlantPanel : Control
{
    #region References

    private Label? _sunLabel;
    private Label? _waveLabel;
    private GridContainer? _plantGrid;

    #endregion

    #region State

    private PlantType _selectedPlant;
    private bool _hasSelection;

    #endregion

    #region Plant Data

    private readonly (PlantType type, string name, int cost)[] _plants = {
        (PlantType.Sunflower, "Sunflower", 50),
        (PlantType.Peashooter, "Peashooter", 100),
        (PlantType.Wallnut, "Wallnut", 50),
        (PlantType.SnowPea, "SnowPea", 175),
        (PlantType.CherryBomb, "CherryBomb", 150),
        (PlantType.Repeater, "Repeater", 200),
        (PlantType.Chomper, "Chomper", 150)
    };

    #endregion

    public override void _Ready()
    {
        CreateUI();

        // 连接信号
        var gameManager = PvZGameManager.Instance;
        if (gameManager != null)
        {
            gameManager.SunChanged += OnSunChanged;
            gameManager.WaveStarted += OnWaveStarted;
        }
    }

    private void CreateUI()
    {
        // 背景面板
        var background = new ColorRect
        {
            Color = new Color(0.2f, 0.3f, 0.1f, 0.8f),
        };
        background.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        background.CustomMinimumSize = new Vector2(0, 120);
        AddChild(background);

        // 容器
        var container = new HBoxContainer();
        container.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        container.OffsetLeft = 20;
        container.OffsetRight = -20;
        container.OffsetTop = 10;
        AddChild(container);

        // 阳光显示
        var sunContainer = new VBoxContainer();
        _sunLabel = new Label();
        _sunLabel.Text = "Sun: 50";
        _sunLabel.AddThemeFontSizeOverride("font_size", 24);
        sunContainer.AddChild(_sunLabel);
        container.AddChild(sunContainer);

        // 波次显示
        var waveContainer = new VBoxContainer();
        _waveLabel = new Label();
        _waveLabel.Text = "Wave: 0/10";
        _waveLabel.AddThemeFontSizeOverride("font_size", 20);
        waveContainer.AddChild(_waveLabel);
        container.AddChild(waveContainer);

        // 植物选择网格
        _plantGrid = new GridContainer
        {
            Columns = 7
        };

        foreach (var (type, name, cost) in _plants)
        {
            var button = CreatePlantButton(type, name, cost);
            _plantGrid.AddChild(button);
        }

        container.AddChild(_plantGrid);
    }

    private Button CreatePlantButton(PlantType type, string name, int cost)
    {
        var button = new Button
        {
            CustomMinimumSize = new Vector2(60, 60),
            TooltipText = $"{name}\nCost: {cost}"
        };

        button.Pressed += () => SelectPlant(type);

        return button;
    }

    private void SelectPlant(PlantType type)
    {
        _selectedPlant = type;
        _hasSelection = true;
        Log.Info($"[PvZ] Selected plant: {type}");
    }

    #region Signal Handlers

    private void OnSunChanged(int sun)
    {
        if (_sunLabel != null)
        {
            _sunLabel.Text = $"Sun: {sun}";
        }
    }

    private void OnWaveStarted(int wave, int total)
    {
        if (_waveLabel != null)
        {
            _waveLabel.Text = $"Wave: {wave}/{total}";
        }
    }

    #endregion

    /// <summary>
    /// 获取当前选中的植物
    /// </summary>
    public PlantType? GetSelectedPlant()
    {
        if (!_hasSelection) return null;
        return _selectedPlant;
    }

    /// <summary>
    /// 清除选中
    /// </summary>
    public void ClearSelection()
    {
        _hasSelection = false;
    }
}
