using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 防守卡牌
/// 可拖拽放置单位的卡牌
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
    private Control _unitPreview;

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
        CustomMinimumSize = new Vector2(80, 120);
        MouseFilter = MouseFilterEnum.Pass;

        CreateVisuals();
    }

    private void SetupCard()
    {
        // 根据单位类型设置费用
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

    private void CreateVisuals()
    {
        // 背景
        _background = new ColorRect
        {
            Color = GetCardColor(),
            Size = new Vector2(78, 118),
            Position = new Vector2(1, 1)
        };
        AddChild(_background);

        // 边框
        var border = new ReferenceRect
        {
            BorderColor = Colors.White,
            EditorOnly = false,
            Size = new Vector2(78, 118),
            Position = new Vector2(1, 1)
        };
        AddChild(border);

        // 名称标签
        _nameLabel = new Label
        {
            Position = new Vector2(5, 70),
            Size = new Vector2(70, 20),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = GetUnitName()
        };
        _nameLabel.AddThemeColorOverride("font_color", Colors.White);
        _nameLabel.AddThemeFontSizeOverride("font_size", 12);
        AddChild(_nameLabel);

        // 费用标签
        _costLabel = new Label
        {
            Position = new Vector2(5, 95),
            Size = new Vector2(70, 20),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = $"能量: {_energyCost}"
        };
        _costLabel.AddThemeColorOverride("font_color", Colors.Gold);
        _costLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_costLabel);

        // 单位预览（简化版）
        var preview = new ColorRect
        {
            Color = GetUnitColor(),
            Size = new Vector2(40, 40),
            Position = new Vector2(20, 20)
        };
        AddChild(preview);
    }

    private Color GetCardColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.4f, 0.2f, 0.2f),
            SDUnitType.Silent => new Color(0.2f, 0.3f, 0.2f),
            SDUnitType.Defect => new Color(0.2f, 0.2f, 0.4f),
            SDUnitType.Necrobinder => new Color(0.3f, 0.2f, 0.4f),
            SDUnitType.Regent => new Color(0.4f, 0.35f, 0.2f),
            _ => new Color(0.3f, 0.3f, 0.3f)
        };
    }

    private Color GetUnitColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.8f, 0.3f, 0.2f),
            SDUnitType.Silent => new Color(0.3f, 0.6f, 0.4f),
            SDUnitType.Defect => new Color(0.3f, 0.5f, 0.8f),
            SDUnitType.Necrobinder => new Color(0.5f, 0.3f, 0.6f),
            SDUnitType.Regent => new Color(0.9f, 0.7f, 0.2f),
            _ => Colors.Gray
        };
    }

    private string GetUnitName()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => "铁卫",
            SDUnitType.Silent => "影弓",
            SDUnitType.Defect => "构造体",
            SDUnitType.Necrobinder => "死灵师",
            SDUnitType.Regent => "摄政王",
            _ => "未知"
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
