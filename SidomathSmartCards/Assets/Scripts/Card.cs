using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    [Header("Card Data")]
    public CardType cardType;
    [SerializeField] private int cardId;
    [SerializeField] private string cardPairType;
    [SerializeField] private int orderIndex;
    public bool onHand = true;
    public bool picked = false;
    public bool matched = false;
    public bool flipped = false;
    
    public int _cardId { get => cardId; set => cardId = value; }
    public string _cardPairType { get => cardPairType; set => cardPairType = value; }
    public int _orderIndex { get => orderIndex; set => orderIndex = value; }

    [Header("Card Contents")]
    public Button cardButton;
    public GameObject cardHighlight;
    public Canvas overrideCanvas;
    public Image cardImage;
    public CanvasGroup canvasGroup;
    public Animator animator;
    public Player player;

    private void OnDisable()
    {
        if (onHand)
        {
            cardButton.onClick.RemoveAllListeners();
        }
    }

    public void InitCard(int _id, string _pairType, Sprite _sprite)
    {
        _cardId = _id;
        _cardPairType = _pairType;
        cardImage.sprite = _sprite;
        matched = false;
        flipped = false;

        if (onHand)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() =>
            {
                Picked();
            });
        }
    }

    public void Picked()
    {
        if (player.playerType == Player.PlayerType.Player)
        {
            player.PickCard(this);
        }
        
    }

    public enum CardType
    {
        OnHand = 0,
        OnBoard = 1
    }
}
