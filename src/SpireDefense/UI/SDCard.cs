using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Core;
using MegaCrit.Sts2.SpireDefense.Entities.Units;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 防守卡牌
/// 可拖拽放置单位的卡牌，类似 STS2 的卡牌样式
/// </summary>
public partial class SDCard : Control
{
    private SDUnitType _unitType;
    private int _energyCost;
    private bool _isDragging = false;
    private Vector2 _dragOffset;

    // UI 节点
    private ColorRect _background;
    private Label _nameLabel;
    private Label _costLabel;
    private Label _descLabel;
    private Control _unitPreview;

    // 单位配置
    private UnitConfig _unitConfig;

    // 事件
    public event Action DragStarted;
    public event Action<Vector2> DragEnded;

    public SDUnitType UnitType => _unitType;
    public int EnergyCost => _energyCost;

    public static SDCard Create(SDUnitType unitType)
    {
        var card = new SDCard();
        card._unitType = unitType;
        card.SetupCard();
        return card;
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(120, 160);
        MouseFilter = MouseFilterEnum.Pass;

        CreateVisuals();
    }

    private void SetupCard()
    {
        // 从工厂获取配置
        _unitConfig = SDUnitFactory.GetUnitConfig(_unitType);
        if (_unitConfig != null)
        {
            _energyCost = _unitConfig.EnergyCost;
        }
        else
        {
            // 默认费用
            _energyCost = _unitType switch
            {
                SDUnitType.Ironclad => 50,
                SDUnitType.Silent => 100,
                SDUnitType.Defect => 75,
                SDUnitType.Necrobinder => 100,
                SDUnitType.Regent => 150,
                _ => 50
            };
        }
    }

    private void CreateVisuals()
    {
        // 背景
        _background = new ColorRect
        {
            Color = GetCardBackgroundColor(),
            Size = new Vector2(118, 158),
            Position = new Vector2(1, 1)
        };
        AddChild(_background);

        // 边框
        var border = new ReferenceRect
        {
            BorderColor = GetCardBorderColor(),
            EditorOnly = false,
            Size = new Vector2(118, 158),
            Position = new Vector2(1, 1)
        };
        AddChild(border);

        // 能量消耗（左上角）
        var costBg = new ColorRect
        {
            Color = new Color(0.1f, 0.1f, 0.1f, 0.8f),
            Size = new Vector2(35, 35),
            Position = new Vector2(5, 5)
        };
        AddChild(costBg);

        _costLabel = new Label
        {
            Position = new Vector2(5, 8),
            Size = new Vector2(35, 25),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = _energyCost.ToString()
        };
        _costLabel.AddThemeColorOverride("font_color", Colors.Gold);
        _costLabel.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_costLabel);

        // 单位预览区域
        var previewBg = new ColorRect
        {
            Color = new Color(0.15f, 0.15f, 0.15f),
            Size = new Vector2(100, 60),
            Position = new Vector2(10, 45)
        };
        AddChild(previewBg);

        // 单位颜色预览
        var unitPreview = new ColorRect
        {
            Color = GetUnitColor(),
            Size = new Vector2(60, 40),
            Position = new Vector2(30, 55)
        };
        AddChild(unitPreview);

        // 名称标签
        _nameLabel = new Label
        {
            Position = new Vector2(5, 110),
            Size = new Vector2(110, 22),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = GetUnitName()
        };
        _nameLabel.AddThemeColorOverride("font_color", Colors.White);
        _nameLabel.AddThemeFontSizeOverride("font_size", 14);
        AddChild(_nameLabel);

        // 描述标签
        _descLabel = new Label
        {
            Position = new Vector2(5, 132),
            Size = new Vector2(110, 24),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = GetUnitDescription(),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _descLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _descLabel.AddThemeFontSizeOverride("font_size", 10);
        AddChild(_descLabel);
    }

    private Color GetCardBackgroundColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.35f, 0.18f, 0.18f),    // 深红
            SDUnitType.Silent => new Color(0.18f, 0.28f, 0.2f),       // 深绿
            SDUnitType.Defect => new Color(0.18f, 0.18f, 0.35f),      // 深蓝
            SDUnitType.Necrobinder => new Color(0.25f, 0.18f, 0.35f), // 深紫
            SDUnitType.Regent => new Color(0.35f, 0.28f, 0.15f),      // 金棕
            _ => new Color(0.25f, 0.25f, 0.25f)
        };
    }

    private Color GetCardBorderColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.8f, 0.3f, 0.3f),
            SDUnitType.Silent => new Color(0.3f, 0.7f, 0.4f),
            SDUnitType.Defect => new Color(0.3f, 0.5f, 0.9f),
            SDUnitType.Necrobinder => new Color(0.6f, 0.3f, 0.7f),
            SDUnitType.Regent => new Color(0.9f, 0.7f, 0.2f),
            _ => Colors.Gray
        };
    }

    private Color GetUnitColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.9f, 0.35f, 0.25f),
            SDUnitType.Silent => new Color(0.35f, 0.7f, 0.45f),
            SDUnitType.Defect => new Color(0.35f, 0.6f, 0.9f),
            SDUnitType.Necrobinder => new Color(0.65f, 0.35f, 0.75f),
            SDUnitType.Regent => new Color(0.95f, 0.75f, 0.25f),
            _ => Colors.Gray
        };
    }

    private string GetUnitName()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => "铁甲卫士",
            SDUnitType.Silent => "暗影猎手",
            SDUnitType.Defect => "奥术构造体",
            SDUnitType.Necrobinder => "亡灵缚魂师",
            SDUnitType.Regent => "摄政王",
            _ => "未知单位"
        };
    }

    private string GetUnitDescription()
    {
        var config = _unitConfig ?? new UnitConfig { MaxHp = 100, AttackDamage = 10 };
        return _unitType switch
        {
            SDUnitType.Ironclad => $"近战坦克\nHP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Silent => $"远程弓手\nHP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Defect => $"法术单位\nHP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Necrobinder => $"召唤师\nHP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Regent => $"精英单位\nHP:{config.MaxHp} ATK:{config.AttackDamage}",
            _ => "基础单位"
        };
    }

    #region 拖拽

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    StartDrag(mouseButton.Position);
                }
                else
                {
                    EndDrag();
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isDragging && @event is InputEventMouseMotion motion)
        {
            // 跟随鼠标
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }

    private void StartDrag(Vector2 localPosition)
    {
        _isDragging = true;
        _dragOffset = localPosition;

        // 提升显示层级
        ZIndex = 100;

        // 放大效果
        Scale = new Vector2(1.2f, 1.2f);

        DragStarted?.Invoke();
        Log.Info($"[SDCard] Started dragging: {_unitType}");
    }

    private void EndDrag()
    {
        if (!_isDragging) return;

        _isDragging = false;
        ZIndex = 0;
        Scale = Vector2.One;

        DragEnded?.Invoke(GetGlobalMousePosition());
        Log.Info($"[SDCard] Ended dragging: {_unitType}");
    }

    #endregion
}
