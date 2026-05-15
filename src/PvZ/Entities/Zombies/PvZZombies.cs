using System;
using Godot;
using MegaCrit.Sts2.PvZ.Core;

namespace MegaCrit.Sts2.PvZ.Entities.Zombies;

/// <summary>
/// 普通僵尸
/// </summary>
public partial class PvZBasicZombie : ZombieBase
{
    public override ZombieType Type => ZombieType.Basic;
    public override int MaxHealth => 100;
    public override float MoveSpeed => 20f;
}

/// <summary>
/// 路障僵尸 - 头戴路障
/// </summary>
public partial class PvZConeheadZombie : ZombieBase
{
    public override ZombieType Type => ZombieType.Conehead;
    public override int MaxHealth => 200;
    public override float MoveSpeed => 20f;

    public override void _Draw()
    {
        base._Draw();
        // 路障
        DrawPolygon(new Vector2[]
        {
            new Vector2(-15, -55),
            new Vector2(15, -55),
            new Vector2(0, -75)
        }, new Color[] { new Color(1.0f, 0.5f, 0.0f) });
    }
}

/// <summary>
/// 铁桶僵尸 - 头戴铁桶
/// </summary>
public partial class PvZBucketheadZombie : ZombieBase
{
    public override ZombieType Type => ZombieType.Buckethead;
    public override int MaxHealth => 400;
    public override float MoveSpeed => 20f;

    public override void _Draw()
    {
        base._Draw();
        // 铁桶
        DrawRect(new Rect2(-12, -70, 24, 25), new Color(0.5f, 0.5f, 0.5f));
        DrawRect(new Rect2(-12, -70, 24, 3), new Color(0.3f, 0.3f, 0.3f));
    }
}

/// <summary>
/// 跑步僵尸 - 快速移动
/// </summary>
public partial class PvZRunnerZombie : ZombieBase
{
    public override ZombieType Type => ZombieType.Runner;
    public override int MaxHealth => 100;
    public override float MoveSpeed => 60f;

    private bool _hasStopped;

    public override void _Process(double delta)
    {
        // 第一次遇到植物后停止跑步
        if (!_hasStopped && GetPlantInFront() != null)
        {
            _hasStopped = true;
        }

        base._Process(delta);
    }

    protected override void Move(float delta)
    {
        float speed = _hasStopped ? 20f : (IsSlowed ? MoveSpeed * 0.5f : MoveSpeed);
        Position = new Vector2(Position.X - speed * delta, Position.Y);
    }

    public override void _Draw()
    {
        Color bodyColor = IsSlowed ? new Color(0.5f, 0.7f, 1.0f) : new Color(0.5f, 0.5f, 0.4f);
        DrawRect(new Rect2(-15, -30, 30, 50), bodyColor);
        DrawCircle(new Vector2(0, -40), 15, new Color(0.7f, 0.7f, 0.6f));
        DrawCircle(new Vector2(-5, -42), 4, new Color(1, 0, 0));
        DrawCircle(new Vector2(5, -42), 4, new Color(1, 0, 0));
    }
}

/// <summary>
/// 巨人僵尸 - 高血量，投掷小僵尸
/// </summary>
public partial class PvZGargantuarZombie : ZombieBase
{
    public override ZombieType Type => ZombieType.Gargantuar;
    public override int MaxHealth => 1000;
    public override float MoveSpeed => 15f;

    protected float AttackRange { get; set; } = 60f;
    protected int AttackDamage { get; set; } = 300;
    protected float AttackInterval { get; set; } = 2.0f;

    private bool _hasThrownImp;

    public override void _Process(double delta)
    {
        // 生命值低于一半时投掷小僵尸
        if (!_hasThrownImp && CurrentHealth < MaxHealth / 2)
        {
            ThrowImp();
            _hasThrownImp = true;
        }

        base._Process(delta);
    }

    private void ThrowImp()
    {
        // 在随机行生成一个小僵尸
        var waveManager = PvZGameManager.Instance?.WaveManager;
        if (waveManager != null)
        {
            int row = new Random().Next(0, 5);
            waveManager.SpawnZombie(ZombieType.Basic, row);
        }
    }

    public override void _Draw()
    {
        // 巨大的身体
        Color bodyColor = IsSlowed ? new Color(0.5f, 0.7f, 1.0f) : new Color(0.5f, 0.3f, 0.3f);
        DrawRect(new Rect2(-25, -50, 50, 70), bodyColor);
        DrawCircle(new Vector2(0, -60), 25, new Color(0.7f, 0.5f, 0.5f));
        DrawCircle(new Vector2(-8, -65), 6, new Color(1, 0, 0));
        DrawCircle(new Vector2(8, -65), 6, new Color(1, 0, 0));

        // 生命条（更大）
        float healthPercent = (float)CurrentHealth / MaxHealth;
        DrawRect(new Rect2(-30, -85, 60, 8), new Color(0.3f, 0.3f, 0.3f));
        DrawRect(new Rect2(-30, -85, 60 * healthPercent, 8), new Color(1, 0, 0));
    }
}
