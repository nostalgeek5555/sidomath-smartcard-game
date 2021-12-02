using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using NaughtyAttributes;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private int cardId;
    [SerializeField] private string cardPairType;
    public bool matched = false;
    public bool flipped = false;
    
    public int _cardId { get => cardId; set => cardId = value; }
    public string _cardPairType { get => cardPairType; set => cardPairType = value; }

    [Header("Card Contents")]
    public Image cardImage;
    public CanvasGroup canvasGroup;
    public Animator animator;


    public void InitCard(int _id, string _pairType, Sprite _sprite)
    {
        _cardId = _id;
        _cardPairType = _pairType;
        cardImage.sprite = _sprite;
        matched = false;
        flipped = false;
    }
}
