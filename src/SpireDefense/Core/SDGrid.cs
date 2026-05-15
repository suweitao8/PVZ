using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Entities.Units;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 网格管理系统
/// 管理单位的放置和位置
/// </summary>
public partial class SDGrid : Control
{
    private const int Rows = 5;
    private const int Columns = 9;

    // 网格数据
    private readonly SDGridCell[,] _cells = new SDGridCell[Rows, Columns];

    // 网格配置
    private const float CellWidth = 80f;
    private const float CellHeight = 100f;
    private const float GridOffsetX = 50f;
    private const float GridOffsetY = 150f;

    // 节点引用
    private Control _cellsContainer;

    // 当前拖拽预览
    private SDUnitBase _previewUnit;
    private SDGridCell _highlightedCell;

    public int RowCount => Rows;
    public int ColumnCount => Columns;

    public override void _Ready()
    {
        _cellsContainer = GetNode<Control>("%CellsContainer");
        CreateGridCells();
        Log.Info($"[SDGrid] Grid created: {Rows}x{Columns}");
    }

    private void CreateGridCells()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                var cell = CreateCell(row, col);
                _cells[row, col] = cell;
                _cellsContainer?.AddChild(cell);
            }
        }
    }

    private SDGridCell CreateCell(int row, int col)
    {
        var cell = new SDGridCell(row, col);
        cell.Position = new Vector2(
            GridOffsetX + col * CellWidth,
            GridOffsetY + row * CellHeight
        );
        cell.CustomMinimumSize = new Vector2(CellWidth, CellHeight);

        // 连接信号
        cell.MouseEntered += () => OnCellHover(cell);
        cell.MouseExited += () => OnCellExit(cell);

        return cell;
    }

    #region 单位放置

    public bool CanPlaceUnit(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns)
            return false;

        return _cells[row, col].IsEmpty;
    }

    public bool PlaceUnit(SDUnitBase unit, int row, int col)
    {
        if (!CanPlaceUnit(row, col))
        {
            Log.Warn($"[SDGrid] Cannot place unit at ({row}, {col})");
            return false;
        }

        var cell = _cells[row, col];
        cell.SetUnit(unit);
        unit.GridPosition = new Vector2I(col, row);
        unit.Position = cell.Position + new Vector2(CellWidth / 2, CellHeight / 2);

        Log.Info($"[SDGrid] Unit placed at ({row}, {col})");
        return true;
    }

    public void RemoveUnit(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns)
            return;

        _cells[row, col].ClearUnit();
    }

    public SDUnitBase GetUnitAt(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Columns)
            return null;

        return _cells[row, col].Unit;
    }

    #endregion

    #region 拖拽预览

    public void ShowPlacementPreview(SDUnitBase unit, Vector2 globalPosition)
    {
        // 隐藏之前的预览
        HidePlacementPreview();

        // 计算最近的格子
        var (row, col) = GlobalPositionToGrid(globalPosition);

        if (CanPlaceUnit(row, col))
        {
            // 创建预览
            _previewUnit = unit;
            _previewUnit.Modulate = new Color(1, 1, 1, 0.5f);
            _previewUnit.Position = new Vector2(
                GridOffsetX + col * CellWidth + CellWidth / 2,
                GridOffsetY + row * CellHeight + CellHeight / 2
            );
            AddChild(_previewUnit);

            // 高亮格子
            _highlightedCell = _cells[row, col];
            _highlightedCell.SetHighlight(true, true);
        }
        else
        {
            // 红色高亮不可放置
            if (row >= 0 && row < Rows && col >= 0 && col < Columns)
            {
                _highlightedCell = _cells[row, col];
                _highlightedCell.SetHighlight(true, false);
            }
        }
    }

    public void HidePlacementPreview()
    {
        if (_previewUnit != null)
        {
            _previewUnit.QueueFree();
            _previewUnit = null;
        }

        if (_highlightedCell != null)
        {
            _highlightedCell.SetHighlight(false, true);
            _highlightedCell = null;
        }
    }

    public (int row, int col) GlobalPositionToGrid(Vector2 globalPosition)
    {
        var localPos = globalPosition - GlobalPosition;
        int col = (int)((localPos.X - GridOffsetX) / CellWidth);
        int row = (int)((localPos.Y - GridOffsetY) / CellHeight);
        return (row, col);
    }

    #endregion

    #region 鼠标事件

    private void OnCellHover(SDGridCell cell)
    {
        // 鼠标悬停效果
    }

    private void OnCellExit(SDGridCell cell)
    {
        // 鼠标离开
    }

    #endregion

    #region 获取单位

    public SDUnitBase[] GetAllUnits()
    {
        var units = new System.Collections.Generic.List<SDUnitBase>();
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (!_cells[row, col].IsEmpty)
                {
                    units.Add(_cells[row, col].Unit);
                }
            }
        }
        return units.ToArray();
    }

    public SDUnitBase[] GetUnitsInRow(int row)
    {
        var units = new System.Collections.Generic.List<SDUnitBase>();
        for (int col = 0; col < Columns; col++)
        {
            if (!_cells[row, col].IsEmpty)
            {
                units.Add(_cells[row, col].Unit);
            }
        }
        return units.ToArray();
    }

    #endregion
}
