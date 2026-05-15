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

    public PvZSunflower()
    {
        BodyColor = new Color(1.0f, 0.8f, 0.0f);
        AccentColor = new Color(0.9f, 0.6f, 0.0f);
        LabelText = "SF";
        Size = 28f;
    }

    public override void OnPlaced()
    {
        _productionTimer = PRODUCTION_INTERVAL;
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
        // 直接添加阳光，并显示浮动文字
        PvZGameManager.Instance?.AddSun(SUN_VALUE, Position + new Vector2(0, -30));

        // 创建浮动文字效果
        CreateFloatingText($"+{SUN_VALUE} ☀", new Color(1.0f, 0.85f, 0.0f));
    }

    private void CreateFloatingText(string text, Color color)
    {
        var container = PvZGameManager.Instance?.FloatingTextContainer;
        if (container == null) return;

        var floatingText = new PvZFloatingText();
        floatingText.Position = Position + new Vector2(0, -50);
        floatingText.Setup(text, color);
        container.AddChild(floatingText);
    }

    protected override void DrawPlantSpecific()
    {
        // 花瓣效果
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.Pi / 4;
            Vector2 petalPos = new Vector2(Mathf.Cos(angle) * 20, Mathf.Sin(angle) * 20 - 5);
            DrawCircle(petalPos, 8, AccentColor);
        }

        // 进度指示
        float progress = 1.0f - (_productionTimer / PRODUCTION_INTERVAL);
        if (progress > 0)
        {
            DrawArc(new Vector2(0, -Size - 10), 10, -Mathf.Pi / 2, -Mathf.Pi / 2 + Mathf.Pi * 2 * progress, 16, new Color(1, 1, 0.5f), 2f);
        }
    }
}
