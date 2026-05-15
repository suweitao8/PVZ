using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Entities.Units;
using MegaCrit.Sts2.SpireDefense.UI;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 手牌区管理
/// 参考 STS2 的手牌系统：固定 5 张手牌，弧形排列，打出后自动补牌
/// 使用 SDHandCardHolder 复用 STS2 的动画逻辑
/// </summary>
public partial class SDHandArea : Control
{
    private const int MaxHandSize = 5;  // 固定 5 张手牌

    // 卡牌列表
    private readonly List<SDHandCardHolder> _hand = new List<SDHandCardHolder>();
    private readonly List<SDUnitType> _deck = new List<SDUnitType>();

    // UI 节点
    private Control _cardsContainer;
    private Label _deckCountLabel;

    // 拖拽相关
    private SDHandCardHolder _draggingCard;
    private bool _isDragging = false;
    private Line2D _dragArrow;  // 拖拽箭头

    public int HandCount => _hand.Count;
    public int DeckCount => _deck.Count;

    public override void _Ready()
    {
        _cardsContainer = GetNode<Control>("%CardsContainer");
        _deckCountLabel = GetNode<Label>("%DeckCountLabel");

        // 创建拖拽箭头
        CreateDragArrow();

        // 初始化牌库
        InitializeDeck();

        // 开局抽满手牌（5 张）
        for (int i = 0; i < MaxHandSize; i++)
        {
            DrawCard();
        }

        UpdateDeckCount();
        ArrangeCards();
        Log.Info($"[SDHandArea] Hand area initialized with {_hand.Count} cards");
    }

    private void CreateDragArrow()
    {
        _dragArrow = new Line2D
        {
            Width = 3f,
            DefaultColor = new Color(1f, 0.8f, 0.2f, 0.8f),
            ZIndex = 1000
        };
        AddChild(_dragArrow);
        _dragArrow.Visible = false;
    }

    private void InitializeDeck()
    {
        // 创建基础牌库
        for (int i = 0; i < 8; i++)
            _deck.Add(SDUnitType.Ironclad);
        for (int i = 0; i < 6; i++)
            _deck.Add(SDUnitType.Silent);
        for (int i = 0; i < 6; i++)
            _deck.Add(SDUnitType.Defect);
        for (int i = 0; i < 4; i++)
            _deck.Add(SDUnitType.Necrobinder);
        for (int i = 0; i < 3; i++)
            _deck.Add(SDUnitType.Regent);

        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        var random = new Random();
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
    }

    #region 抽卡

    private void DrawCard()
    {
        if (_deck.Count == 0 || _hand.Count >= MaxHandSize) return;

        var unitType = _deck[0];
        _deck.RemoveAt(0);

        var card = SDHandCardHolder.Create(unitType);
        if (card != null)
        {
            _cardsContainer.AddChild(card);
            _hand.Add(card);

            // 连接信号
            card.DragStarted += OnCardDragStarted;
            card.DragEnded += OnCardDragEnded;
            card.Hovered += OnCardHovered;
            card.Unhovered += OnCardUnhovered;

            // 设置初始位置（屏幕外）
            card.SetPositionInstantly(new Vector2(Size.X / 2f, Size.Y + 100f));
            card.SetScaleInstantly(Vector2.One * 0.5f);

            UpdateDeckCount();
            ArrangeCards();
            Log.Info($"[SDHandArea] Drew card: {unitType}");
        }
    }

    private void UpdateDeckCount()
    {
        if (_deckCountLabel != null)
        {
            _deckCountLabel.Text = $"牌库: {_deck.Count}";
        }
    }

    #endregion

    #region 卡牌排列

    /// <summary>
    /// 按弧形排列卡牌（使用 STS2 的 HandPosHelper）
    /// </summary>
    private void ArrangeCards(bool animate = true)
    {
        if (_hand.Count == 0) return;

        for (int i = 0; i < _hand.Count; i++)
        {
            var card = _hand[i];
            if (card == _draggingCard) continue;

            // 使用 STS2 的 HandPosHelper 获取位置和角度
            var pos = HandPosHelper.GetPosition(_hand.Count, i) * 0.5f;  // 缩放适配
            var angle = HandPosHelper.GetAngle(_hand.Count, i);
            var scale = HandPosHelper.GetScale(_hand.Count) * 0.8f;

            if (animate)
            {
                card.SetTargetPosition(pos);
                card.SetTargetAngle(angle);
                card.SetTargetScale(scale);
            }
            else
            {
                card.SetPositionInstantly(pos);
                card.SetAngleInstantly(angle);
                card.SetScaleInstantly(scale);
            }
        }
    }

    #endregion

    #region 拖拽

    private void OnCardDragStarted(SDHandCardHolder card)
    {
        _draggingCard = card;
        _isDragging = true;

        // 显示拖拽箭头
        _dragArrow.Visible = true;

        Log.Info($"[SDHandArea] Started dragging: {card.UnitType}");
    }

    private void OnCardDragEnded(SDHandCardHolder card, Vector2 globalPosition)
    {
        _isDragging = false;
        _draggingCard = null;
        _dragArrow.Visible = false;

        // 尝试放置
        var grid = SDGame.Instance?.Grid;
        bool placed = false;

        if (grid != null)
        {
            var (row, col) = grid.GlobalPositionToGrid(globalPosition);

            if (grid.CanPlaceUnit(row, col))
            {
                if (SDGame.Instance?.TrySpendEnergy(card.EnergyCost) == true)
                {
                    var unit = SDUnitFactory.Create(card.UnitType);
                    if (unit != null)
                    {
                        grid.PlaceUnit(unit, row, col);
                        _hand.Remove(card);
                        card.QueueFree();

                        // 自动补牌
                        DrawCard();

                        // 重新排列
                        ArrangeCards();

                        placed = true;
                        Log.Info($"[SDHandArea] Placed {card.UnitType} at ({row}, {col})");
                    }
                }
            }
        }

        grid?.HidePlacementPreview();

        // 如果没有放置成功，恢复卡牌位置
        if (!placed)
        {
            ArrangeCards();
        }
    }

    public override void _Process(double delta)
    {
        if (_isDragging && _draggingCard != null)
        {
            var mousePos = GetGlobalMousePosition();

            // 更新拖拽箭头
            UpdateDragArrow(_draggingCard.GlobalPosition, mousePos);

            // 更新网格预览
            var grid = SDGame.Instance?.Grid;
            if (grid != null)
            {
                var previewUnit = SDUnitFactory.Create(_draggingCard.UnitType);
                if (previewUnit != null)
                {
                    grid.ShowPlacementPreview(previewUnit, mousePos);
                }
            }
        }
    }

    /// <summary>
    /// 更新拖拽箭头（从卡牌到鼠标位置的曲线）
    /// </summary>
    private void UpdateDragArrow(Vector2 startPos, Vector2 endPos)
    {
        _dragArrow.ClearPoints();

        // 转换为本地坐标
        var localStart = startPos - GlobalPosition;
        var localEnd = endPos - GlobalPosition;

        // 创建贝塞尔曲线
        int segments = 20;
        var controlPoint = new Vector2(
            (localStart.X + localEnd.X) / 2f,
            localStart.Y - 100f
        );

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            var point = QuadraticBezier(localStart, controlPoint, localEnd, t);
            _dragArrow.AddPoint(point);
        }

        // 设置渐变颜色
        var gradient = new Gradient();
        gradient.AddPoint(0f, new Color(1f, 0.8f, 0.2f, 0.9f));
        gradient.AddPoint(1f, new Color(1f, 0.5f, 0.1f, 0.6f));
        _dragArrow.Gradient = gradient;
    }

    private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }

    #endregion

    /// <summary>
    /// 鼠标悬停在卡牌上时的高亮
    /// </summary>
    public void OnCardHovered(SDHandCardHolder card)
    {
        if (_isDragging) return;
        // SDHandCardHolder 自己处理悬停效果
    }

    public void OnCardUnhovered(SDHandCardHolder card)
    {
        if (_isDragging) return;
        // SDHandCardHolder 自己处理悬停效果
    }
}
