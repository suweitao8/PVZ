using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.SpireDefense.Entities.Units;
using MegaCrit.Sts2.SpireDefense.UI;

namespace MegaCrit.Sts2.SpireDefense.Core;

/// <summary>
/// 手牌区管理
/// 管理卡牌的显示、抽取、刷新
/// </summary>
public partial class SDHandArea : Control
{
    private const int MaxHandSize = 5;
    private const int DrawCost = 25;
    private const int RefreshCost = 50;

    // 卡牌列表
    private readonly List<SDCard> _hand = new List<SDCard>();
    private readonly List<SDUnitType> _deck = new List<SDUnitType>();

    // UI 节点
    private HBoxContainer _cardsContainer;
    private Button _drawButton;
    private Button _refreshButton;
    private Label _deckCountLabel;

    // 当前拖拽的卡牌
    private SDCard _draggingCard;
    private bool _isDragging = false;

    public int HandCount => _hand.Count;
    public int DeckCount => _deck.Count;

    public override void _Ready()
    {
        _cardsContainer = GetNode<HBoxContainer>("%CardsContainer");
        _drawButton = GetNode<Button>("%DrawButton");
        _refreshButton = GetNode<Button>("%RefreshButton");
        _deckCountLabel = GetNode<Label>("%DeckCountLabel");

        // 连接按钮信号
        _drawButton.Connect(Button.SignalName.Pressed, Callable.From(OnDrawPressed));
        _refreshButton.Connect(Button.SignalName.Pressed, Callable.From(OnRefreshPressed));

        // 初始化牌库
        InitializeDeck();

        // 开局抽 5 张
        for (int i = 0; i < 5; i++)
        {
            DrawCard(spendEnergy: false);
        }

        UpdateDeckCount();
        Log.Info($"[SDHandArea] Hand area initialized with {_hand.Count} cards");
    }

    private void InitializeDeck()
    {
        // 创建基础牌库
        // 每种单位若干张
        for (int i = 0; i < 5; i++)
            _deck.Add(SDUnitType.Ironclad);    // 铁卫
        for (int i = 0; i < 4; i++)
            _deck.Add(SDUnitType.Silent);      // 影弓
        for (int i = 0; i < 4; i++)
            _deck.Add(SDUnitType.Defect);      // 构造体
        for (int i = 0; i < 3; i++)
            _deck.Add(SDUnitType.Necrobinder); // 死灵师
        for (int i = 0; i < 2; i++)
            _deck.Add(SDUnitType.Regent);      // 摄政王

        // 洗牌
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

    private void OnDrawPressed()
    {
        if (_hand.Count >= MaxHandSize)
        {
            Log.Info("[SDHandArea] Hand is full");
            // TODO: 显示提示
            return;
        }

        if (SDGame.Instance?.TrySpendEnergy(DrawCost) == true)
        {
            DrawCard(spendEnergy: true);
        }
        else
        {
            Log.Info("[SDHandArea] Not enough energy");
            // TODO: 显示提示
        }
    }

    private void OnRefreshPressed()
    {
        if (SDGame.Instance?.TrySpendEnergy(RefreshCost) == true)
        {
            // 弃掉所有手牌
            foreach (var card in _hand)
            {
                card.QueueFree();
            }
            _hand.Clear();

            // 重新洗牌
            ShuffleDeck();

            // 抽 5 张
            for (int i = 0; i < MaxHandSize && _deck.Count > 0; i++)
            {
                DrawCard(spendEnergy: false);
            }

            UpdateDeckCount();
            Log.Info("[SDHandArea] Hand refreshed");
        }
        else
        {
            Log.Info("[SDHandArea] Not enough energy");
        }
    }

    private void DrawCard(bool spendEnergy)
    {
        if (_deck.Count == 0)
        {
            Log.Info("[SDHandArea] Deck is empty");
            return;
        }

        // 抽取第一张
        var unitType = _deck[0];
        _deck.RemoveAt(0);

        // 创建卡牌
        var card = SDCard.Create(unitType);
        if (card != null)
        {
            _cardsContainer.AddChild(card);
            _hand.Add(card);

            // 连接拖拽信号
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

    #region 拖拽

    private void OnCardDragStarted(SDCard card)
    {
        _draggingCard = card;
        _isDragging = true;
        Log.Info($"[SDHandArea] Started dragging: {card.UnitType}");
    }

    private void OnCardDragEnded(SDCard card, Vector2 globalPosition)
    {
        _isDragging = false;

        // 尝试放置
        var grid = SDGame.Instance?.Grid;
        if (grid != null)
        {
            var (row, col) = grid.GlobalPositionToGrid(globalPosition);

            if (grid.CanPlaceUnit(row, col))
            {
                // 检查能量
                if (SDGame.Instance?.TrySpendEnergy(card.EnergyCost) == true)
                {
                    // 创建单位
                    var unit = SDUnitFactory.Create(card.UnitType);
                    if (unit != null)
                    {
                        grid.PlaceUnit(unit, row, col);

                        // 从手牌移除
                        _hand.Remove(card);
                        card.QueueFree();

                        Log.Info($"[SDHandArea] Placed {card.UnitType} at ({row}, {col})");
                    }
                }
            }
        }

        grid?.HidePlacementPreview();
        _draggingCard = null;
    }

    public override void _Process(double delta)
    {
        if (_isDragging && _draggingCard != null)
        {
            // 更新预览
            var grid = SDGame.Instance?.Grid;
            if (grid != null)
            {
                var mousePos = GetGlobalMousePosition();

                // 创建预览单位
                var previewUnit = SDUnitFactory.Create(_draggingCard.UnitType);
                if (previewUnit != null)
                {
                    grid.ShowPlacementPreview(previewUnit, mousePos);
                }
            }
        }
    }

    #endregion
}

/// <summary>
/// 单位类型枚举
/// </summary>
public enum SDUnitType
{
    Ironclad,    // 铁卫 - 坦克
    Silent,      // 影弓 - 远程
    Defect,      // 构造体 - 法师
    Necrobinder, // 死灵师 - 召唤
    Regent       // 摄政王 - 精英
}
