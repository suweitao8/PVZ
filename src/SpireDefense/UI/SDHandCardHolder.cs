using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 尖塔防卫战卡牌持有者
/// 复用 STS2 的手牌动画逻辑，但简化为塔防用途
/// </summary>
public partial class SDHandCardHolder : Control
{
    // 动画常量（来自 NHandCardHolder）
    private const float _rotateSpeed = 10f;
    private const float _scaleSpeed = 8f;
    private const float _moveSpeed = 7f;
    private const float _angleSnapThreshold = 0.1f;
    private const float _scaleSnapThreshold = 0.002f;
    private const float _positionSnapThreshold = 1f;

    // 目标值
    private Vector2 _targetPosition;
    private float _targetAngle;
    private Vector2 _targetScale = Vector2.One;

    // 动画取消令牌
    private CancellationTokenSource? _angleCancelToken;
    private CancellationTokenSource? _positionCancelToken;
    private CancellationTokenSource? _scaleCancelToken;

    // 卡牌数据
    private SDUnitType _unitType;
    private int _energyCost = 50;
    private UnitConfig? _unitConfig;

    // 拖拽状态
    private bool _isDragging = false;
    private bool _isHovered = false;
    private Vector2 _dragOffset;

    // 悬停缩放
    private static readonly Vector2 HoverScale = new Vector2(0.9f, 0.9f);
    private static readonly Vector2 DragScale = new Vector2(1.0f, 1.0f);

    // UI 组件
    private Control _cardVisual;

    // 事件
    public event Action<SDHandCardHolder> DragStarted;
    public event Action<SDHandCardHolder, Vector2> DragEnded;
    public event Action<SDHandCardHolder> Hovered;
    public event Action<SDHandCardHolder> Unhovered;

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
        MouseFilter = MouseFilterEnum.Stop;
        CreateVisuals();
    }

    private void SetupCard()
    {
        _unitConfig = SDUnitFactory.GetUnitConfig(_unitType);
        _energyCost = _unitConfig?.EnergyCost ?? 50;
    }

    private void CreateVisuals()
    {
        // 创建卡牌视觉容器
        _cardVisual = new Control
        {
            Position = new Vector2(-75, -100)  // 居中
        };
        AddChild(_cardVisual);

        // 卡牌背景
        var bg = new ColorRect
        {
            Color = GetCardBackgroundColor(),
            Size = new Vector2(150, 200)
        };
        _cardVisual.AddChild(bg);

        // 卡牌边框
        var border = new ReferenceRect
        {
            BorderColor = GetCardBorderColor(),
            EditorOnly = false,
            Size = new Vector2(150, 200)
        };
        _cardVisual.AddChild(border);

        // 能量消耗圆形背景
        var costBg = new CircleShape2D();
        var costCircle = new ColorRect
        {
            Color = new Color(0.15f, 0.15f, 0.15f),
            Size = new Vector2(50, 50),
            Position = new Vector2(5, 5)
        };
        _cardVisual.AddChild(costCircle);

        // 能量数值
        var costLabel = new Label
        {
            Position = new Vector2(5, 12),
            Size = new Vector2(50, 30),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = _energyCost.ToString()
        };
        costLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
        costLabel.AddThemeFontSizeOverride("font_size", 24);
        costLabel.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://fonts/kreon_bold.ttf"));
        _cardVisual.AddChild(costLabel);

        // 卡牌插图区域
        var portraitBg = new ColorRect
        {
            Color = new Color(0.12f, 0.12f, 0.12f),
            Size = new Vector2(130, 80),
            Position = new Vector2(10, 60)
        };
        _cardVisual.AddChild(portraitBg);

        // 角色颜色预览
        var portrait = new ColorRect
        {
            Color = GetUnitColor(),
            Size = new Vector2(80, 60),
            Position = new Vector2(35, 70)
        };
        _cardVisual.AddChild(portrait);

        // 卡牌名称
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
        nameLabel.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://fonts/kreon_bold.ttf"));
        _cardVisual.AddChild(nameLabel);

        // 卡牌描述
        var descLabel = new Label
        {
            Position = new Vector2(5, 173),
            Size = new Vector2(140, 24),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = GetUnitDescription()
        };
        descLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.75f));
        descLabel.AddThemeFontSizeOverride("font_size", 11);
        _cardVisual.AddChild(descLabel);
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

    private string GetUnitDescription()
    {
        var config = _unitConfig ?? new UnitConfig { MaxHp = 100, AttackDamage = 10 };
        return _unitType switch
        {
            SDUnitType.Ironclad => $"近战坦克 | HP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Silent => $"远程弓手 | HP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Defect => $"法术单位 | HP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Necrobinder => $"召唤师 | HP:{config.MaxHp} ATK:{config.AttackDamage}",
            SDUnitType.Regent => $"精英单位 | HP:{config.MaxHp} ATK:{config.AttackDamage}",
            _ => "基础单位"
        };
    }

    #endregion

    #region 动画（复用 STS2 逻辑）

    public void SetTargetPosition(Vector2 position)
    {
        _targetPosition = position;
        _positionCancelToken?.Cancel();
        _positionCancelToken = new CancellationTokenSource();
        TaskHelper.RunSafely(AnimPosition(_positionCancelToken));
    }

    public void SetTargetAngle(float angle)
    {
        _targetAngle = angle;
        _angleCancelToken?.Cancel();
        _angleCancelToken = new CancellationTokenSource();
        TaskHelper.RunSafely(AnimAngle(_angleCancelToken));
    }

    public void SetTargetScale(Vector2 scale)
    {
        _targetScale = scale;
        _scaleCancelToken?.Cancel();
        _scaleCancelToken = new CancellationTokenSource();
        TaskHelper.RunSafely(AnimScale(_scaleCancelToken));
    }

    public void SetAngleInstantly(float angle)
    {
        _angleCancelToken?.Cancel();
        RotationDegrees = angle;
    }

    public void SetScaleInstantly(Vector2 scale)
    {
        _scaleCancelToken?.Cancel();
        Scale = scale;
    }

    public void SetPositionInstantly(Vector2 position)
    {
        _positionCancelToken?.Cancel();
        Position = position;
    }

    private async Task AnimAngle(CancellationTokenSource cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            RotationDegrees = Mathf.Lerp(RotationDegrees, _targetAngle, (float)GetProcessDeltaTime() * _rotateSpeed);
            if (Mathf.Abs(RotationDegrees - _targetAngle) < _angleSnapThreshold)
            {
                RotationDegrees = _targetAngle;
                break;
            }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private async Task AnimScale(CancellationTokenSource cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Scale = Scale.Lerp(_targetScale, (float)GetProcessDeltaTime() * _scaleSpeed);
            if (Mathf.Abs(_targetScale.X - Scale.X) < _scaleSnapThreshold)
            {
                Scale = _targetScale;
                break;
            }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private async Task AnimPosition(CancellationTokenSource cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Position = Position.Lerp(_targetPosition, (float)GetProcessDeltaTime() * _moveSpeed);
            if (Position.DistanceSquaredTo(_targetPosition) < _positionSnapThreshold)
            {
                Position = _targetPosition;
                break;
            }
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    #endregion

    #region 拖拽

    public void BeginDrag()
    {
        _isDragging = true;
        ZIndex = 100;
        SetAngleInstantly(0f);
        SetScaleInstantly(DragScale);
        DragStarted?.Invoke(this);
    }

    public void CancelDrag()
    {
        _isDragging = false;
        ZIndex = 0;
        SetAngleInstantly(0f);
        SetScaleInstantly(Vector2.One * 0.8f);
    }

    #endregion

    #region 鼠标事件

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                _dragOffset = mouseButton.Position;
                BeginDrag();
            }
            else
            {
                EndDrag();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isDragging && @event is InputEventMouseMotion)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationMouseEnter)
        {
            _isHovered = true;
            if (!_isDragging)
            {
                ZIndex = 50;
                SetTargetScale(HoverScale);
                Hovered?.Invoke(this);
            }
        }
        else if (what == NotificationMouseExit)
        {
            _isHovered = false;
            if (!_isDragging)
            {
                ZIndex = 0;
                Unhovered?.Invoke(this);
            }
        }
    }

    private void EndDrag()
    {
        if (!_isDragging) return;

        _isDragging = false;
        ZIndex = 0;
        DragEnded?.Invoke(this, GetGlobalMousePosition());
    }

    #endregion

    /// <summary>
    /// 设置默认目标位置（用于放回手牌）
    /// </summary>
    public void SetDefaultTargets(int handSize, int index)
    {
        ZIndex = 0;
        SetTargetPosition(HandPosHelper.GetPosition(handSize, index) * 0.5f);  // 缩放适配
        SetTargetAngle(HandPosHelper.GetAngle(handSize, index));
        SetTargetScale(HandPosHelper.GetScale(handSize) * 0.8f);
    }
}
