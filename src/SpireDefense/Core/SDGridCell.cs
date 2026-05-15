using Godot;
using MegaCrit.Sts2.SpireDefense.Entities.Units;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 网格单元格
/// 管理单个格子的状态和显示
/// </summary>
public partial class SDGridCell : Control
{
    public int Row { get; }
    public int Column { get; }
    public SDUnitBase Unit { get; private set; }
    public bool IsEmpty => Unit == null;

    private ColorRect _background;
    private ColorRect _highlight;

    public SDGridCell(int row, int column)
    {
        Row = row;
        Column = column;

        // 创建背景
        _background = new ColorRect
        {
            Color = new Color(0.2f, 0.4f, 0.2f, 0.5f),
            Size = new Vector2(78, 98)
        };
        AddChild(_background);

        // 创建高亮层
        _highlight = new ColorRect
        {
            Color = Colors.Transparent,
            Size = new Vector2(78, 98),
            Visible = false
        };
        AddChild(_highlight);

        // 边框
        var border = new ReferenceRect
        {
            BorderColor = new Color(0.3f, 0.5f, 0.3f, 0.8f),
            EditorOnly = false
        };
        AddChild(border);

        // 鼠标检测
        MouseFilter = MouseFilterEnum.Pass;
    }

    public void SetUnit(SDUnitBase unit)
    {
        Unit = unit;
    }

    public void ClearUnit()
    {
        Unit = null;
    }

    public void SetHighlight(bool active, bool canPlace)
    {
        _highlight.Visible = active;
        if (active)
        {
            _highlight.Color = canPlace
                ? new Color(0, 1, 0, 0.3f)  // 绿色 = 可放置
                : new Color(1, 0, 0, 0.3f);  // 红色 = 不可放置
        }
    }
}
