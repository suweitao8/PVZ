using Godot;

namespace MegaCrit.Sts2.PvZ.Core;

/// <summary>
/// 浮动文字效果
/// 显示向上漂浮并消失的文字
/// </summary>
public partial class PvZFloatingText : Node2D
{
    private string _text = "";
    private Color _color = new Color(1.0f, 1.0f, 1.0f);
    private float _lifetime = 1.5f;
    private float _speed = 50f;
    private float _age = 0f;

    public void Setup(string text, Color color, float lifetime = 1.5f, float speed = 50f)
    {
        _text = text;
        _color = color;
        _lifetime = lifetime;
        _speed = speed;
    }

    public override void _Process(double delta)
    {
        _age += (float)delta;

        // 向上漂浮
        Position = new Vector2(Position.X, Position.Y - _speed * (float)delta);

        // 淡出
        float alpha = 1.0f - (_age / _lifetime);
        Modulate = new Color(_color.R, _color.G, _color.B, alpha);

        // 消失
        if (_age >= _lifetime)
        {
            QueueFree();
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(-30, 0), _text,
            fontSize: 20,
            modulate: Modulate);
    }
}
