using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 尖塔防卫战卡牌持有者
/// 完全复用 STS2 的 NHandCardHolder 动画和交互逻辑
/// </summary>
public partial class SDHandCardHolder : NHandCardHolder
{
    // 卡牌数据
    private SDUnitType _unitType;
    private int _energyCost = 50;
    private UnitConfig? _unitConfig;

    // 事件（重新定义以适配尖塔防卫战）
    public event Action<SDHandCardHolder>? CardDragStarted;
    public event Action<SDHandCardHolder, Vector2>? CardDragEnded;

    public SDUnitType UnitType => _unitType;
    public int EnergyCost => _energyCost;

    /// <summary>
    /// 创建卡牌持有者
    /// </summary>
    public static SDHandCardHolder Create(SDUnitType unitType)
    {
        // 加载 STS2 的 hand_card_holder 场景
        var scene = ResourceLoader.Load<PackedScene>("res://scenes/cards/holders/hand_card_holder.tscn");
        if (scene == null)
        {
            Log.Error("[SDHandCardHolder] Failed to load hand_card_holder.tscn");
            return null!;
        }

        var holder = scene.Instantiate<SDHandCardHolder>();
        if (holder != null)
        {
            holder._unitType = unitType;
            holder.SetupCard();
        }
        return holder!;
    }

    public override void _Ready()
    {
        // 不调用 base._Ready()，因为 NHandCardHolder 会抛出异常
        // 直接调用 ConnectSignals
        ConnectSignals();
        CreateCardVisuals();
    }

    private void SetupCard()
    {
        _unitConfig = SDUnitFactory.GetUnitConfig(_unitType);
        _energyCost = _unitConfig?.EnergyCost ?? 50;
    }

    private void CreateCardVisuals()
    {
        // 创建简单的卡牌视觉效果
        // NHandCardHolder 已经有 Hitbox，我们只需要添加卡牌内容
        var content = new Control
        {
            Name = "CardContent",
            Position = new Vector2(-75, -100),
            Size = new Vector2(150, 200)
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

        AddChild(content);

        // 连接父类的信号到我们的事件
        Connect(SignalName.HolderFocused, Callable.From<NHandCardHolder>(OnHolderFocused));
        Connect(SignalName.HolderUnfocused, Callable.From<NHandCardHolder>(OnHolderUnfocused));
        Connect(SignalName.HolderMouseClicked, Callable.From<NCardHolder>(OnHolderClicked));
    }

    private void OnHolderFocused(NHandCardHolder holder)
    {
        // 通知 SDHandArea 重新排列
    }

    private void OnHolderUnfocused(NHandCardHolder holder)
    {
        // 通知 SDHandArea 恢复排列
    }

    private void OnHolderClicked(NCardHolder holder)
    {
        // 开始拖拽
        CardDragStarted?.Invoke(this);
    }

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

    /// <summary>
    /// 立即设置位置（无动画）
    /// </summary>
    public void SetPositionInstantly(Vector2 position)
    {
        Position = position;
    }

    /// <summary>
    /// 设置默认目标位置（用于放回手牌）
    /// </summary>
    public void SetDefaultTargets(int handSize, int index)
    {
        ZIndex = 0;
        SetTargetPosition(HandPosHelper.GetPosition(handSize, index) * 0.5f);
        SetTargetAngle(HandPosHelper.GetAngle(handSize, index));
        SetTargetScale(HandPosHelper.GetScale(handSize) * 0.8f);
    }
}
