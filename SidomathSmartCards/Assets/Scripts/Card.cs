using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Card Data")]
    public CardType cardType;
    public Facing facing;
    public Tile.DroppedPoint droppedPoint;
    public Tile.TopBottomSpecific topBottomSpecific;
    public MatchedSide matchedSide;
    [SerializeField] private int cardId;
    [SerializeField] private string cardPairType;
    [SerializeField] private int orderIndex;
    public bool onHand = true;
    public bool canBePick = true;
    public bool draggable = true;
    public bool picked = false;
    public bool dropped = false;
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

    [Header("Dragging Properties")]
    public bool insideCardConnector = false;
    public bool onTweeningBack = false;
    public GameObject mainConnector = null;
    public CardConnector currentCardConnector = null;
    public Transform topCardConnector = null;
    public Transform bottomCardConnector = null;
    public PointerEventData pointerEventData = null;
    public BoxCollider2D collider2D;
    private Vector2 dragOffset = Vector2.zero;
    private Vector2 limits = Vector2.zero;
    public LayoutElement layoutElement;
    public Transform originParent = null;
    public Transform placeholderParent = null;
    private GameObject placeholder = null;

    [Header("Dropped Card Properties")]
    public Card pairingCard = null;
    public GraphicRaycaster graphicRaycaster = null;
    public Tile currentTile = null;
    public string tileIndex;
    

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
        dropped = false;
        matched = false;
        flipped = false;
        cardType = CardType.OnHand;
        matchedSide = MatchedSide.None;
        droppedPoint = Tile.DroppedPoint.Empty;
        topBottomSpecific = Tile.TopBottomSpecific.None;

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
        if (cardType == CardType.OnHand)
        {
            if (player != null && player.playerType == Player.PlayerType.Player)
            {
                player.PickCard(this);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (player != null)
        {
            if (player.playerState == Player.PlayerState.GET_TURN && draggable)
            {
                pointerEventData = eventData;
                collider2D.enabled = true;
                player.handCardFitter.enabled = false;
                player.handCardGroup.enabled = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.6f;
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                player.PickCard(this);
                Debug.Log($"Begin to drag card with id :: {cardId} & pair type {cardPairType}");
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (player != null)
        {
            if (player.playerState == Player.PlayerState.GET_TURN && cardType == CardType.OnHand)
            {
                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -Camera.main.transform.position.z);
                Vector3 screenTouch = screenCenter + new Vector3(eventData.delta.x, eventData.delta.y, 0);

                Vector3 worldCenterPosition = Camera.main.ScreenToWorldPoint(screenCenter);
                Vector3 worldTouchPosition = Camera.main.ScreenToWorldPoint(screenTouch);

                Vector3 delta = worldTouchPosition - worldCenterPosition;

                transform.position += delta;
            }

            else
            {
                throw new Exception();
            }
        }
    }

    public void TweenBack(Action<int, Card> action = null)
    {
        if (GameplayManager.Instance != null)
        {
            if (player != null)
            {
                onTweeningBack = true;
                draggable = false;
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.1f);
                sequence.Join(transform.DOMove(player.handCardParent.position, 0.4f));
                sequence.AppendCallback(() =>
                {
                    pointerEventData = null;
                    onHand = true;
                    collider2D.enabled = false;
                    transform.SetParent(GameplayManager.Instance.player.handCardGroup.transform);
                    transform.SetSiblingIndex(cardId);
                    player.handCardFitter.enabled = true;
                    player.handCardGroup.enabled = true;
                    action?.Invoke(cardId, this);
                });
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CardConnector>() != null)
        {
            CardConnector cardConnector = collision.GetComponent<CardConnector>();
            if (!cardConnector.dropped)
            {
                cardConnector.draggedCard = this;
                insideCardConnector = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<CardConnector>() != null)
        {
            CardConnector cardConnector = collision.GetComponent<CardConnector>();
            if (!cardConnector.dropped)
            {
                cardConnector.draggedCard = null;
                currentCardConnector = null;
                insideCardConnector = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<CardConnector>() != null)
        {
            CardConnector cardConnector = collision.GetComponent<CardConnector>();
            if (!cardConnector.dropped)
            {
                cardConnector.draggedCard = this;
                insideCardConnector = true;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (player != null)
        {
            canvasGroup.blocksRaycasts = true;

            if (currentCardConnector == null)
            {
                TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
            }

            else
            {
                if (insideCardConnector)
                {
                    if (!currentCardConnector.CheckIfConnectedCardFlipped())
                    {
                        Card parentCard = currentCardConnector.transform.parent.parent.parent.GetComponent<Card>();
                        BoardManager.Instance.OnDropCard(this, parentCard, currentCardConnector);
                        Debug.Log("Dropped on valid card connector");
                    }

                    else
                    {
                        Debug.Log("on card flipped tween back card");
                        TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                    }
                }

                else
                {
                    TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                }
            }
        }
    }

    public enum Facing
    {
       Vertical = 0,
       Horizontal = 1
    }
    public enum CardType
    {
        OnHand = 0,
        OnBoard = 1
    }

    public enum MatchedSide
    {
        None = 0,
        Left = 1,
        Right = 2,
        Both = 3
    }
}
