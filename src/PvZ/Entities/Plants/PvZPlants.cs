using Godot;
using MegaCrit.Sts2.PvZ.Core;
using MegaCrit.Sts2.PvZ.Projectiles;

namespace MegaCrit.Sts2.PvZ.Entities.Plants;

/// <summary>
/// 豌豆射手 - 射击豌豆
/// </summary>
public partial class PvZPeashooter : PlantBase
{
    public override PlantType Type => PlantType.Peashooter;
    public override int SunCost => 100;
    public override float Cooldown => 7.5f;

    private float _shootTimer;
    private const float SHOOT_INTERVAL = 1.5f;

    public override void OnPlaced()
    {
        _shootTimer = SHOOT_INTERVAL;
    }

    public override void OnZombieInRow()
    {
        // 自动攻击
    }

    public override void _Process(double delta)
    {
        if (!IsInitialized) return;

        // 只有有僵尸时才射击
        if (!HasZombieToRight()) return;

        _shootTimer -= (float)delta;

        if (_shootTimer <= 0)
        {
            Shoot();
            _shootTimer = SHOOT_INTERVAL;
        }
    }

    private void Shoot()
    {
        var pea = new PvZPea();
        pea.Position = Position + new Vector2(30, 0);
        pea.Row = Row;
        PvZGameManager.Instance.ProjectileContainer?.AddChild(pea);
    }

    public override void _Draw()
    {
        // 豌豆射手头部
        DrawCircle(new Vector2(0, -10), 20, new Color(0.2f, 0.8f, 0.2f));
        // 茎
        DrawRect(new Rect2(-5, 0, 10, 30), new Color(0.1f, 0.6f, 0.1f));
        // 眼睛
        DrawCircle(new Vector2(8, -15), 5, new Color(1, 1, 1));
        DrawCircle(new Vector2(10, -15), 3, new Color(0, 0, 0));
    }
}

/// <summary>
/// 坚果墙 - 阻挡僵尸
/// </summary>
public partial class PvZWallnut : PlantBase
{
    public override PlantType Type => PlantType.Wallnut;
    public override int SunCost => 50;
    public override float Cooldown => 30.0f;

    public int MaxHealth { get; } = 400;
    public int CurrentHealth { get; private set; }

    public override void OnPlaced()
    {
        CurrentHealth = MaxHealth;
    }

    public override void OnZombieInRow()
    {
        // 坚果墙不主动攻击
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            QueueFree();
        }
    }

    public override void _Draw()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;
        Color color = healthPercent > 0.66f ? new Color(0.8f, 0.6f, 0.2f) :
                      healthPercent > 0.33f ? new Color(0.7f, 0.5f, 0.2f) :
                      new Color(0.5f, 0.4f, 0.2f);

        DrawCircle(Vector2.Zero, 25, color);
        // 裂纹效果
        if (healthPercent < 0.66f)
        {
            DrawLine(new Vector2(-10, -10), new Vector2(10, 10), new Color(0.3f, 0.2f, 0.1f), 2);
        }
        if (healthPercent < 0.33f)
        {
            DrawLine(new Vector2(10, -10), new Vector2(-10, 10), new Color(0.3f, 0.2f, 0.1f), 2);
        }
    }
}

/// <summary>
/// 寒冰射手 - 射击冰豌豆，减速僵尸
/// </summary>
public partial class PvZSnowPea : PlantBase
{
    public override PlantType Type => PlantType.SnowPea;
    public override int SunCost => 175;
    public override float Cooldown => 7.5f;

    private float _shootTimer;
    private const float SHOOT_INTERVAL = 1.5f;

    public override void OnPlaced()
    {
        _shootTimer = SHOOT_INTERVAL;
    }

    public override void OnZombieInRow() { }

    public override void _Process(double delta)
    {
        if (!IsInitialized || !HasZombieToRight()) return;

        _shootTimer -= (float)delta;
        if (_shootTimer <= 0)
        {
            var pea = new PvZPea(isIce: true);
            pea.Position = Position + new Vector2(30, 0);
            pea.Row = Row;
            PvZGameManager.Instance.ProjectileContainer?.AddChild(pea);
            _shootTimer = SHOOT_INTERVAL;
        }
    }

    public override void _Draw()
    {
        DrawCircle(new Vector2(0, -10), 20, new Color(0.5f, 0.8f, 1.0f));
        DrawRect(new Rect2(-5, 0, 10, 30), new Color(0.3f, 0.6f, 0.8f));
    }
}

/// <summary>
/// 樱桃炸弹 - 范围爆炸
/// </summary>
public partial class PvZCherryBomb : PlantBase
{
    public override PlantType Type => PlantType.CherryBomb;
    public override int SunCost => 150;
    public override float Cooldown => 50.0f;

    private float _timer;
    private bool _exploded;

    public override void OnPlaced()
    {
        _timer = 1.0f; // 1秒后爆炸
    }

    public override void OnZombieInRow() { }

    public override void _Process(double delta)
    {
        if (_exploded) return;

        _timer -= (float)delta;
        if (_timer <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        _exploded = true;

        // 对范围内的僵尸造成伤害
        var waveManager = PvZGameManager.Instance?.WaveManager;
        if (waveManager != null)
        {
            for (int row = Row - 1; row <= Row + 1; row++)
            {
                foreach (var zombie in waveManager.GetZombiesInRow(row))
                {
                    float dist = System.Math.Abs(zombie.Position.X - Position.X);
                    if (dist < 100)
                    {
                        zombie.TakeDamage(1800); // 一击必杀
                    }
                }
            }
        }

        QueueFree();
    }

    public override void _Draw()
    {
        DrawCircle(new Vector2(-10, 0), 18, new Color(1.0f, 0.2f, 0.2f));
        DrawCircle(new Vector2(10, 0), 18, new Color(1.0f, 0.2f, 0.2f));
        DrawCircle(new Vector2(-10, -5), 5, new Color(0, 0, 0));
        DrawCircle(new Vector2(10, -5), 5, new Color(0, 0, 0));
    }
}

/// <summary>
/// 双发射手 - 双倍射速
/// </summary>
public partial class PvZRepeater : PlantBase
{
    public override PlantType Type => PlantType.Repeater;
    public override int SunCost => 200;
    public override float Cooldown => 7.5f;

    private float _shootTimer;
    private const float SHOOT_INTERVAL = 1.0f;

    public override void OnPlaced() => _shootTimer = SHOOT_INTERVAL;
    public override void OnZombieInRow() { }

    public override void _Process(double delta)
    {
        if (!IsInitialized || !HasZombieToRight()) return;

        _shootTimer -= (float)delta;
        if (_shootTimer <= 0)
        {
            // 发射两颗豌豆
            for (int i = 0; i < 2; i++)
            {
                var pea = new PvZPea();
                pea.Position = Position + new Vector2(30 + i * 20, 0);
                pea.Row = Row;
                PvZGameManager.Instance.ProjectileContainer?.AddChild(pea);
            }
            _shootTimer = SHOOT_INTERVAL;
        }
    }

    public override void _Draw()
    {
        DrawCircle(new Vector2(0, -10), 20, new Color(0.2f, 0.8f, 0.2f));
        DrawCircle(new Vector2(15, -10), 15, new Color(0.2f, 0.8f, 0.2f));
        DrawRect(new Rect2(-5, 0, 10, 30), new Color(0.1f, 0.6f, 0.1f));
    }
}

/// <summary>
/// 大嘴花 - 一口吃掉僵尸
/// </summary>
public partial class PvZChomper : PlantBase
{
    public override PlantType Type => PlantType.Chomper;
    public override int SunCost => 150;
    public override float Cooldown => 30.0f;

    private float _chewTimer;
    private bool _isChewing;

    public override void OnPlaced()
    {
        _chewTimer = 0;
        _isChewing = false;
    }

    public override void OnZombieInRow() { }

    public override void _Process(double delta)
    {
        if (_isChewing)
        {
            _chewTimer -= (float)delta;
            if (_chewTimer <= 0)
            {
                _isChewing = false;
            }
            return;
        }

        // 检查是否有僵尸在攻击范围内
        var zombie = GetFirstZombieToRight();
        if (zombie != null && zombie.Position.X - Position.X < 50)
        {
            EatZombie(zombie);
        }
    }

    private void EatZombie(PvZ.Entities.Zombies.ZombieBase zombie)
    {
        zombie.TakeDamage(zombie.MaxHealth); // 一击必杀
        _isChewing = true;
        _chewTimer = 30.0f; // 咀嚼时间
    }

    public override void _Draw()
    {
        Color headColor = _isChewing ? new Color(0.6f, 0.3f, 0.3f) : new Color(0.8f, 0.4f, 0.4f);
        DrawCircle(new Vector2(0, -15), 25, headColor);
        DrawRect(new Rect2(-8, 0, 16, 25), new Color(0.3f, 0.6f, 0.2f));
        // 嘴巴
        DrawArc(Vector2.Zero, 15, 0, 3.14159f, 10, new Color(0.2f, 0.1f, 0.1f), 3);
    }
}
