using Godot;
using MegaCrit.Sts2.PvZ.Core;

namespace MegaCrit.Sts2.PvZ.Entities.Plants;

/// <summary>
/// 向日葵 - 产出阳光
/// </summary>
public partial class PvZSunflower : PlantBase
{
    public override PlantType Type => PlantType.Sunflower;
    public override int SunCost => 50;
    public override float Cooldown => 7.5f;

    private float _productionTimer;
    private const float PRODUCTION_INTERVAL = 24.0f;
    private const int SUN_VALUE = 50;

    public override void OnPlaced()
    {
        _productionTimer = PRODUCTION_INTERVAL;
    }

    public override void OnZombieInRow()
    {
        // 向日葵不攻击僵尸
    }

    public override void _Process(double delta)
    {
        if (!IsInitialized) return;

        _productionTimer -= (float)delta;

        if (_productionTimer <= 0)
        {
            ProduceSun();
            _productionTimer = PRODUCTION_INTERVAL;
        }
    }

    private void ProduceSun()
    {
        // 创建阳光并添加到场景
        var sun = new PvZSun(SUN_VALUE);
        sun.Position = Position + new Vector2(0, -20);
        PvZGameManager.Instance.SunContainer?.AddChild(sun);
    }

    public override void _Draw()
    {
        // 绘制向日葵（临时用圆形代替）
        DrawCircle(Vector2.Zero, 25, new Color(1.0f, 0.8f, 0.0f));
        DrawCircle(new Vector2(-8, -5), 10, new Color(0.8f, 0.6f, 0.0f));
        DrawCircle(new Vector2(8, -5), 10, new Color(0.8f, 0.6f, 0.0f));
    }
}

/// <summary>
/// 阳光实体
/// </summary>
public partial class PvZSun : Area2D
{
    public int Value { get; }

    private bool _collected;
    private float _lifetime;
    private const float MAX_LIFETIME = 10.0f;

    public PvZSun(int value = 25)
    {
        Value = value;

        // 设置碰撞
        var collision = new CollisionShape2D();
        var shape = new CircleShape2D { Radius = 25 };
        collision.Shape = shape;
        AddChild(collision);

        // 连接点击信号
        InputEvent += OnInputEvent;
    }

    public override void _Ready()
    {
        _lifetime = MAX_LIFETIME;
    }

    public override void _Process(double delta)
    {
        if (_collected) return;

        _lifetime -= (float)delta;

        if (_lifetime <= 0)
        {
            QueueFree();
        }

        // 闪烁效果（快消失时）
        if (_lifetime < 2.0f)
        {
            Modulate = new Color(1, 1, 1, _lifetime / 2.0f);
        }
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (_collected) return;

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            Collect();
        }
        else if (@event is InputEventScreenTouch touch && touch.Pressed)
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (_collected) return;

        _collected = true;
        PvZGameManager.Instance?.AddSun(Value);
        QueueFree();
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 20, new Color(1.0f, 1.0f, 0.0f));
        DrawCircle(Vector2.Zero, 15, new Color(1.0f, 0.9f, 0.3f));
    }
}
