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
/// </summary>
public partial class SDHandArea : Control
{
    private const int MaxHandSize = 5;  // 固定 5 张手牌

    // 卡牌列表
    private readonly List<SDCard> _hand = new List<SDCard>();
    private readonly List<SDUnitType> _deck = new List<SDUnitType>();

    // UI 节点
    private Control _cardsContainer;
    private Label _deckCountLabel;

    // 拖拽相关
    private SDCard _draggingCard;
    private bool _isDragging = false;
    private Line2D _dragArrow;  // 拖拽箭头

    // 动画
    private Tween _rearrangeTween;

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

        var card = SDCard.Create(unitType);
        if (card != null)
        {
            _cardsContainer.AddChild(card);
            _hand.Add(card);

            // 连接信号
            card.DragStarted += () => OnCardDragStarted(card);
            card.DragEnded += (pos) => OnCardDragEnded(card, pos);

            UpdateDeckCount();
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
    /// 按弧形排列卡牌
    /// </summary>
    private void ArrangeCards(bool animate = true)
    {
        if (_hand.Count == 0) return;

        _rearrangeTween?.Kill();
        if (animate)
        {
            _rearrangeTween = CreateTween();
        }

        for (int i = 0; i < _hand.Count; i++)
        {
            var card = _hand[i];
            if (card == _draggingCard) continue;

            // 使用 STS2 的位置和角度计算
            var pos = GetCardPosition(_hand.Count, i);
            var angle = GetCardAngle(_hand.Count, i);
            var scale = GetCardScale(_hand.Count);

            if (animate)
            {
                _rearrangeTween.Parallel().TweenProperty(card, "position", pos, 0.2f).SetEase(Tween.EaseType.Out);
                _rearrangeTween.Parallel().TweenProperty(card, "rotation", Mathf.DegToRad(angle), 0.2f).SetEase(Tween.EaseType.Out);
                _rearrangeTween.Parallel().TweenProperty(card, "scale", scale, 0.2f).SetEase(Tween.EaseType.Out);
            }
            else
            {
                card.Position = pos;
                card.Rotation = Mathf.DegToRad(angle);
                card.Scale = scale;
            }
        }
    }

    /// <summary>
    /// 获取卡牌在弧形中的位置
    /// </summary>
    private Vector2 GetCardPosition(int handSize, int index)
    {
        // 基于 STS2 的位置数据，调整为我们的屏幕尺寸
        float centerX = Size.X / 2f;
        float baseY = Size.Y / 2f;

        // 5 张牌的位置
        var positions = new Vector2[]
        {
            new Vector2(-340f, 10f),
            new Vector2(-170f, -30f),
            new Vector2(0f, -50f),
            new Vector2(170f, -30f),
            new Vector2(340f, 10f)
        };

        if (handSize <= 5 && index < positions.Length)
        {
            return new Vector2(centerX + positions[index].X, baseY + positions[index].Y);
        }

        // 动态计算
        float spacing = 170f;
        float startX = centerX - (handSize - 1) * spacing / 2f;
        return new Vector2(startX + index * spacing, baseY);
    }

    /// <summary>
    /// 获取卡牌角度
    /// </summary>
    private float GetCardAngle(int handSize, int index)
    {
        var angles = new float[] { -8f, -4f, 0f, 4f, 8f };
        if (handSize <= 5 && index < angles.Length)
        {
            return angles[index];
        }
        return 0f;
    }

    /// <summary>
    /// 获取卡牌缩放
    /// </summary>
    private Vector2 GetCardScale(int handSize)
    {
        return Vector2.One * 0.8f;
    }

    #endregion

    #region 拖拽

    private void OnCardDragStarted(SDCard card)
    {
        _draggingCard = card;
        _isDragging = true;

        // 显示拖拽箭头
        _dragArrow.Visible = true;

        // 提升卡牌层级
        card.ZIndex = 100;
        card.Scale = Vector2.One * 1.1f;
        card.Rotation = 0f;

        Log.Info($"[SDHandArea] Started dragging: {card.UnitType}");
    }

    private void OnCardDragEnded(SDCard card, Vector2 globalPosition)
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
    public void OnCardHovered(SDCard card)
    {
        if (_isDragging) return;

        // 提升悬停的卡牌
        card.ZIndex = 50;
        var tween = CreateTween();
        tween.Parallel().TweenProperty(card, "scale", Vector2.One * 0.9f, 0.1f);
        tween.Parallel().TweenProperty(card, "position:y", card.Position.Y - 20f, 0.1f);
    }

    public void OnCardUnhovered(SDCard card)
    {
        if (_isDragging) return;
        ArrangeCards();
    }
}
