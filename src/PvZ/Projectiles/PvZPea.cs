using Godot;
using MegaCrit.Sts2.PvZ.Entities.Zombies;

namespace MegaCrit.Sts2.PvZ.Projectiles;

/// <summary>
/// 豌豆子弹
/// </summary>
public partial class PvZPea : Area2D
{
    /// <summary>
    /// 伤害值
    /// </summary>
    public int Damage { get; init; } = 20;

    /// <summary>
    /// 移动速度
    /// </summary>
    public float Speed { get; init; } = 300f;

    /// <summary>
    /// 所在行
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// 是否为冰豌豆
    /// </summary>
    public bool IsIce { get; init; }

    private bool _hit;

    public PvZPea(bool isIce = false)
    {
        IsIce = isIce;

        // 设置碰撞
        var collision = new CollisionShape2D();
        var shape = new CircleShape2D { Radius = 10 };
        collision.Shape = shape;
        AddChild(collision);

        // 连接碰撞信号
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_hit) return;

        // 向右移动
        Position = new Vector2(Position.X + Speed * (float)delta, Position.Y);

        // 超出屏幕范围则销毁
        if (Position.X > 2000)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (_hit) return;

        if (body is ZombieBase zombie && zombie.Row == Row && !zombie.IsDead)
        {
            _hit = true;
            zombie.TakeDamage(Damage);

            if (IsIce)
            {
                zombie.ApplySlow();
            }

            QueueFree();
        }
    }

    public override void _Draw()
    {
        Color peaColor = IsIce ? new Color(0.5f, 0.8f, 1.0f) : new Color(0.4f, 0.8f, 0.2f);
        DrawCircle(Vector2.Zero, 8, peaColor);
        DrawCircle(new Vector2(2, -2), 3, new Color(1, 1, 1, 0.5f));
    }
}
