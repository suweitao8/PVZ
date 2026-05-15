using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.PvZ.Entities.Plants;

namespace MegaCrit.Sts2.PvZ.Core;

/// <summary>
/// PvZ 网格单元格
/// </summary>
public class PvZCell
{
    public int Row { get; }
    public int Col { get; }
    public PlantBase? Plant { get; set; }
    public bool HasPlant => Plant != null;

    public PvZCell(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

/// <summary>
/// PvZ 网格管理器
/// 管理 5x9 的游戏网格
/// </summary>
public partial class PvZGridManager : Node2D
{
    #region Configuration

    /// <summary>
    /// 网格行数
    /// </summary>
    [Export]
    public int Rows { get; set; } = 5;

    /// <summary>
    /// 网格列数
    /// </summary>
    [Export]
    public int Columns { get; set; } = 9;

    /// <summary>
    /// 单元格宽度（像素）
    /// </summary>
    [Export]
    public float CellWidth { get; set; } = 80f;

    /// <summary>
    /// 单元格高度（像素）
    /// </summary>
    [Export]
    public float CellHeight { get; set; } = 100f;

    /// <summary>
    /// 网格起始位置
    /// </summary>
    [Export]
    public Vector2 GridOffset { get; set; } = new Vector2(80, 300);

    #endregion

    #region State

    private readonly PvZCell[,] _cells;

    #endregion

    #region Signals

    /// <summary>
    /// 植物被放置时触发
    /// </summary>
    [Signal]
    public delegate void PlantPlacedEventHandler(int row, int col, PlantType type);

    /// <summary>
    /// 植物被移除时触发
    /// </summary>
    [Signal]
    public delegate void PlantRemovedEventHandler(int row, int col);

    #endregion

    public PvZGridManager()
    {
        _cells = new PvZCell[5, 9];
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cells[row, col] = new PvZCell(row, col);
            }
        }
    }

    public override void _Ready()
    {
        Log.Info($"[PvZ] Grid initialized: {Rows}x{Columns}");
    }

    #region Cell Access

    /// <summary>
    /// 获取指定位置的单元格
    /// </summary>
    public PvZCell? GetCell(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns)
            return null;
        return _cells[row, col];
    }

    /// <summary>
    /// 检查位置是否有效
    /// </summary>
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Columns;
    }

    /// <summary>
    /// 检查位置是否有植物
    /// </summary>
    public bool HasPlantAt(int row, int col)
    {
        return GetCell(row, col)?.HasPlant ?? false;
    }

    #endregion

    #region Coordinate Conversion

    /// <summary>
    /// 将网格坐标转换为世界坐标
    /// </summary>
    public Vector2 GridToWorld(int row, int col)
    {
        return GridOffset + new Vector2(col * CellWidth + CellWidth / 2, row * CellHeight + CellHeight / 2);
    }

    /// <summary>
    /// 将世界坐标转换为网格坐标
    /// </summary>
    public (int row, int col)? WorldToGrid(Vector2 worldPos)
    {
        Vector2 localPos = worldPos - GridOffset;

        int col = (int)(localPos.X / CellWidth);
        int row = (int)(localPos.Y / CellHeight);

        if (IsValidPosition(row, col))
            return (row, col);

        return null;
    }

    #endregion

    #region Plant Management

    /// <summary>
    /// 尝试在指定位置放置植物
    /// </summary>
    public bool TryPlacePlant(int row, int col, PlantType type)
    {
        var cell = GetCell(row, col);
        if (cell == null || cell.HasPlant)
        {
            Log.Info($"[PvZ] Cannot place plant at ({row}, {col})");
            return false;
        }

        // 获取植物花费
        int cost = GetPlantCost(type);
        if (!PvZGameManager.Instance.TrySpendSun(cost))
            return false;

        // 创建植物实例
        var plant = CreatePlant(type, row, col);
        if (plant == null)
            return false;

        cell.Plant = plant;
        AddChild(plant);

        EmitSignal(SignalName.PlantPlaced, row, col, (int)type);
        Log.Info($"[PvZ] Plant {type} placed at ({row}, {col})");

        return true;
    }

    /// <summary>
    /// 移除指定位置的植物
    /// </summary>
    public void RemovePlant(int row, int col)
    {
        var cell = GetCell(row, col);
        if (cell == null || !cell.HasPlant)
            return;

        cell.Plant?.QueueFree();
        cell.Plant = null;

        EmitSignal(SignalName.PlantRemoved, row, col);
        Log.Info($"[PvZ] Plant removed from ({row}, {col})");
    }

    /// <summary>
    /// 获取指定行的所有植物
    /// </summary>
    public PlantBase[] GetPlantsInRow(int row)
    {
        var plants = new System.Collections.Generic.List<PlantBase>();

        for (int col = 0; col < Columns; col++)
        {
            var cell = GetCell(row, col);
            if (cell?.HasPlant == true)
            {
                plants.Add(cell.Plant!);
            }
        }

        return plants.ToArray();
    }

    #endregion

    #region Plant Creation

    private int GetPlantCost(PlantType type)
    {
        return type switch
        {
            PlantType.Sunflower => 50,
            PlantType.Peashooter => 100,
            PlantType.Wallnut => 50,
            PlantType.SnowPea => 175,
            PlantType.CherryBomb => 150,
            PlantType.Repeater => 200,
            PlantType.Chomper => 150,
            _ => 100
        };
    }

    private PlantBase? CreatePlant(PlantType type, int row, int col)
    {
        Vector2 position = GridToWorld(row, col);

        PlantBase? plant = type switch
        {
            PlantType.Sunflower => new PvZSunflower(),
            PlantType.Peashooter => new PvZPeashooter(),
            PlantType.Wallnut => new PvZWallnut(),
            PlantType.SnowPea => new PvZSnowPea(),
            PlantType.CherryBomb => new PvZCherryBomb(),
            PlantType.Repeater => new PvZRepeater(),
            PlantType.Chomper => new PvZChomper(),
            _ => null
        };

        if (plant != null)
        {
            plant.Position = position;
            plant.Setup(row, col);
        }

        return plant;
    }

    #endregion

    #region Debug

    /// <summary>
    /// 绘制网格线（调试用）
    /// </summary>
    public override void _Draw()
    {
        // 绘制网格边框
        for (int row = 0; row <= Rows; row++)
        {
            float y = GridOffset.Y + row * CellHeight;
            DrawLine(
                new Vector2(GridOffset.X, y),
                new Vector2(GridOffset.X + Columns * CellWidth, y),
                new Color(0.5f, 0.5f, 0.5f, 0.5f)
            );
        }

        for (int col = 0; col <= Columns; col++)
        {
            float x = GridOffset.X + col * CellWidth;
            DrawLine(
                new Vector2(x, GridOffset.Y),
                new Vector2(x, GridOffset.Y + Rows * CellHeight),
                new Color(0.5f, 0.5f, 0.5f, 0.5f)
            );
        }
    }

    #endregion
}
