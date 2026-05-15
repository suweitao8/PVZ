using System;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 尖塔防卫战卡牌持有者
/// 包装 STS2 的 NHandCardHolder，添加尖塔防卫战特有功能
/// </summary>
public partial class SDHandCardHolder : Control
{
    // 内部的 NHandCardHolder
    private NHandCardHolder _innerHolder;

    // 卡牌数据
    private SDUnitType _unitType;
    private int _energyCost = 50;
    private UnitConfig? _unitConfig;

    // 事件
    public event Action<SDHandCardHolder>? CardDragStarted;
    public event Action<SDHandCardHolder, Vector2>? CardDragEnded;

    public SDUnitType UnitType => _unitType;
    public int EnergyCost => _energyCost;

    /// <summary>
    /// 创建卡牌持有者
    /// </summary>
    public static SDHandCardHolder Create(SDUnitType unitType)
    {
        var holder = new SDHandCardHolder();
        holder._unitType = unitType;
        holder.SetupCard();
        return holder;
    }

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(150, 200);

        // 加载并实例化 NHandCardHolder 场景
        var scene = ResourceLoader.Load<PackedScene>("res://scenes/cards/holders/hand_card_holder.tscn");
        if (scene == null)
        {
            Log.Error("[SDHandCardHolder] Failed to load hand_card_holder.tscn");
            CreateFallbackVisuals();
            return;
        }

        _innerHolder = scene.Instantiate<NHandCardHolder>();
        if (_innerHolder == null)
        {
            Log.Error("[SDHandCardHolder] Failed to instantiate NHandCardHolder");
            CreateFallbackVisuals();
            return;
        }

        AddChild(_innerHolder);

        // 创建卡牌视觉内容
        CreateCardVisuals();

        // 连接信号
        _innerHolder.Connect(NHandCardHolder.SignalName.HolderFocused,
            Callable.From<NHandCardHolder>(OnHolderFocused));
        _innerHolder.Connect(NHandCardHolder.SignalName.HolderUnfocused,
            Callable.From<NHandCardHolder>(OnHolderUnfocused));
        _innerHolder.Connect(NHandCardHolder.SignalName.HolderMouseClicked,
            Callable.From<NCardHolder>(OnHolderClicked));
    }

    private void SetupCard()
    {
        _unitConfig = SDUnitFactory.GetUnitConfig(_unitType);
        _energyCost = _unitConfig?.EnergyCost ?? 50;
    }

    private void CreateFallbackVisuals()
    {
        var bg = new ColorRect
        {
            Color = GetCardBackgroundColor(),
            Size = new Vector2(150, 200),
            Position = new Vector2(-75, -100)
        };
        AddChild(bg);
    }

    private void CreateCardVisuals()
    {
        // 在 Hitbox 后面添加卡牌内容
        var content = new Control
        {
            Name = "CardContent",
            Position = new Vector2(-75, -100),
            Size = new Vector2(150, 200),
            ZIndex = -1  // 在 Hitbox 后面
        };

        // 卡牌背景
        var bg = new ColorRect
        {
            Color = GetCardBackgroundColor(),
            Size = new Vector2(150, 200)
        };
        content.AddChild(bg);

        // 卡牌边框
        var border = new ReferenceRect
        {
            BorderColor = GetCardBorderColor(),
            EditorOnly = false,
            Size = new Vector2(150, 200)
        };
        content.AddChild(border);

        // 能量消耗
        var costLabel = new Label
        {
            Position = new Vector2(5, 5),
            Size = new Vector2(40, 40),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = _energyCost.ToString()
        };
        costLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
        costLabel.AddThemeFontSizeOverride("font_size", 24);
        content.AddChild(costLabel);

        // 单位名称
        var nameLabel = new Label
        {
            Position = new Vector2(5, 145),
            Size = new Vector2(140, 28),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = GetUnitName()
        };
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        content.AddChild(nameLabel);

        // 单位颜色预览
        var preview = new ColorRect
        {
            Color = GetUnitColor(),
            Size = new Vector2(80, 60),
            Position = new Vector2(35, 60)
        };
        content.AddChild(preview);

        _innerHolder.AddChild(content);
    }

    private void OnHolderFocused(NHandCardHolder holder)
    {
        // 悬停时通知 SDHandArea
    }

    private void OnHolderUnfocused(NHandCardHolder holder)
    {
        // 取消悬停时通知 SDHandArea
    }

    private void OnHolderClicked(NCardHolder holder)
    {
        CardDragStarted?.Invoke(this);
    }

    #region 代理 NHandCardHolder 方法

    public void SetTargetPosition(Vector2 position)
    {
        _innerHolder?.SetTargetPosition(position);
    }

    public void SetTargetAngle(float angle)
    {
        _innerHolder?.SetTargetAngle(angle);
    }

    public void SetTargetScale(Vector2 scale)
    {
        _innerHolder?.SetTargetScale(scale);
    }

    public void SetAngleInstantly(float angle)
    {
        _innerHolder?.SetAngleInstantly(angle);
    }

    public void SetScaleInstantly(Vector2 scale)
    {
        _innerHolder?.SetScaleInstantly(scale);
    }

    public void SetPositionInstantly(Vector2 position)
    {
        if (_innerHolder != null)
        {
            _innerHolder.Position = position;
        }
    }

    public void BeginDrag()
    {
        _innerHolder?.BeginDrag();
    }

    #endregion

    #region 颜色和文本

    private Color GetCardBackgroundColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.38f, 0.20f, 0.18f),
            SDUnitType.Silent => new Color(0.18f, 0.30f, 0.22f),
            SDUnitType.Defect => new Color(0.18f, 0.20f, 0.38f),
            SDUnitType.Necrobinder => new Color(0.28f, 0.18f, 0.38f),
            SDUnitType.Regent => new Color(0.38f, 0.30f, 0.15f),
            _ => new Color(0.25f, 0.25f, 0.25f)
        };
    }

    private Color GetCardBorderColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.85f, 0.35f, 0.25f),
            SDUnitType.Silent => new Color(0.35f, 0.75f, 0.45f),
            SDUnitType.Defect => new Color(0.35f, 0.55f, 0.95f),
            SDUnitType.Necrobinder => new Color(0.65f, 0.35f, 0.75f),
            SDUnitType.Regent => new Color(0.95f, 0.75f, 0.25f),
            _ => Colors.Gray
        };
    }

    private Color GetUnitColor()
    {
        return _unitType switch
        {
            SDUnitType.Ironclad => new Color(0.95f, 0.35f, 0.25f),
            SDUnitType.Silent => new Color(0.35f, 0.75f, 0.45f),
            SDUnitType.Defect => new Color(0.35f, 0.60f, 0.95f),
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
            _ => "未知"
        };
    }

    #endregion
}
