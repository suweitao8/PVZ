using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.SpireDefense.Core;

namespace MegaCrit.Sts2.SpireDefense.UI;

/// <summary>
/// 尖塔防卫战卡牌持有者
/// 使用 NClickableControl 实现悬停检测，复用 STS2 动画逻辑
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

    // 悬停状态
    private bool _isFocused;
    private Tween? _hoverTween;

    // 缩放常量
    private static readonly Vector2 SmallScale = Vector2.One * 0.8f;
    private static readonly Vector2 HoverScale = Vector2.One;

    // Hitbox（使用 STS2 的 NClickableControl）
    private NClickableControl _hitbox;

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
        CreateHitbox();
        CreateCardVisuals();
    }

    private void SetupCard()
    {
        _unitConfig = SDUnitFactory.GetUnitConfig(_unitType);
        _energyCost = _unitConfig?.EnergyCost ?? 50;
    }

    private void CreateHitbox()
    {
        // 使用 STS2 的 NClickableControl 作为 Hitbox
        _hitbox = new NClickableControl
        {
            Name = "Hitbox",
            Size = new Vector2(150, 200),
            Position = new Vector2(-75, -100),
            MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(_hitbox);

        // 连接信号
        _hitbox.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(OnHitboxFocused));
        _hitbox.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(OnHitboxUnfocused));
        _hitbox.Connect(NClickableControl.SignalName.MousePressed, Callable.From<InputEvent>(OnHitboxPressed));
        _hitbox.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(OnHitboxReleased));
    }

    private void OnHitboxFocused(NClickableControl control)
    {
        if (_isFocused) return;
        _isFocused = true;
        ZIndex = 50;

        // 悬停效果：立即变水平并放大
        _hoverTween?.Kill();
        SetAngleInstantly(0f);
        Scale = HoverScale;
    }

    private void OnHitboxUnfocused(NClickableControl control)
    {
        if (!_isFocused) return;
        _isFocused = false;
        ZIndex = 0;

        // 取消悬停：动画缩小
        _hoverTween?.Kill();
        _hoverTween = CreateTween();
        _hoverTween.TweenProperty(this, "scale", SmallScale, 0.3).SetEase(Tween.EaseType.Out);
    }

    private void OnHitboxPressed(InputEvent evt)
    {
        if (evt is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            CardDragStarted?.Invoke(this);
        }
    }

    private void OnHitboxReleased(InputEvent evt)
    {
        if (evt is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            CardDragEnded?.Invoke(this, GetGlobalMousePosition());
        }
    }

    private void CreateCardVisuals()
    {
        // 卡牌背景
        var bg = new ColorRect
        {
            Color = GetCardBackgroundColor(),
            Size = new Vector2(150, 200),
            Position = new Vector2(-75, -100),
            ZIndex = -1
        };
        AddChild(bg);

        // 卡牌边框
        var border = new ReferenceRect
        {
            BorderColor = GetCardBorderColor(),
            EditorOnly = false,
            Size = new Vector2(150, 200),
            Position = new Vector2(-75, -100),
            ZIndex = -1
        };
        AddChild(border);

        // 能量消耗
        var costLabel = new Label
        {
            Position = new Vector2(-70, -95),
            Size = new Vector2(40, 40),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = _energyCost.ToString()
        };
        costLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
        costLabel.AddThemeFontSizeOverride("font_size", 24);
        AddChild(costLabel);

        // 单位名称
        var nameLabel = new Label
        {
            Position = new Vector2(-70, 50),
            Size = new Vector2(140, 28),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = GetUnitName()
        };
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        AddChild(nameLabel);

        // 单位颜色预览
        var preview = new ColorRect
        {
            Color = GetUnitColor(),
            Size = new Vector2(80, 60),
            Position = new Vector2(-40, -35),
            ZIndex = -1
        };
        AddChild(preview);
    }

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
