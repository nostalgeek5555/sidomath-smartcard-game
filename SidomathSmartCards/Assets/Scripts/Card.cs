using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IDragHandler, IDropHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Card Data")]
    public CardType cardType;
    public Tile.DroppedPoint droppedPoint;
    public Tile.TopBottomSpecific topBottomSpecific;
    public MatchedSide matchedSide;
    [SerializeField] private int cardId;
    [SerializeField] private string cardPairType;
    [SerializeField] private int orderIndex;
    public bool onHand = true;
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
    private Vector2 dragOffset = Vector2.zero;
    private Vector2 limits = Vector2.zero;
    public LayoutElement layoutElement;
    public Transform originParent = null;
    public Transform placeholderParent = null;
    private GameObject placeholder = null;

    private void OnEnable()
    {
        BoardManager.Instance.OnCardDropped += Dropped;
        BoardManager.Instance.OnCardMatch += Matched;
    }

    private void OnDisable()
    {
        if (onHand)
        {
            cardButton.onClick.RemoveAllListeners();
        }

        BoardManager.Instance.OnCardDropped -= Dropped;
        BoardManager.Instance.OnCardMatch -= Matched;
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
        if (player.playerType == Player.PlayerType.Player)
        {
            player.PickCard(this);
        }
        
    }

    public void Dropped()
    {

    }

    public void Matched()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (player.playerType == Player.PlayerType.Player)
        {
            Debug.Log($"Begin to drag card with id :: {cardId} & pair type {cardPairType}");

            placeholder = new GameObject();
            placeholder.transform.SetParent(transform.parent);
            LayoutElement _layoutElement = placeholder.AddComponent<LayoutElement>();
            _layoutElement.preferredWidth = layoutElement.preferredWidth;
            _layoutElement.preferredHeight = layoutElement.preferredHeight;
            _layoutElement.flexibleHeight = 0;
            _layoutElement.flexibleWidth = 0;

            placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

            originParent = transform.parent;
            placeholderParent = originParent;
            transform.SetParent(transform.parent.parent);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -Camera.main.transform.position.z);
        Vector3 screenTouch = screenCenter + new Vector3(eventData.delta.x, eventData.delta.y, 0);

        Vector3 worldCenterPosition = Camera.main.ScreenToWorldPoint(screenCenter);
        Vector3 worldTouchPosition = Camera.main.ScreenToWorldPoint(screenTouch);

        Vector3 delta = worldTouchPosition - worldCenterPosition;

        transform.position += delta;

        if (placeholder.transform.parent != placeholderParent)
        {
            placeholder.transform.SetParent(placeholderParent);
        }

        int newSiblingIndex = placeholderParent.childCount;

        for (int i = 0; i < placeholderParent.childCount; i++)
        {
            if (transform.position.x < placeholderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;
                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;

                break;
            }
        }

        placeholder.transform.SetSiblingIndex(newSiblingIndex);
        //transform.position = eventData.position - dragOffset;
        //var dragPos = transform.localPosition;
        //if (dragPos.x < -limits.x) { dragPos.x = -limits.x; }
        //if (dragPos.x > limits.x) { dragPos.x = limits.x; }
        //if (dragPos.y < -limits.y) { dragPos.y = -limits.y; }
        //if (dragPos.y > limits.y) { dragPos.y = limits.y; }
        //transform.localPosition = dragPos;
    }

    public void OnDrop(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        dragOffset = Vector2.zero;
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
        Right = 2
    }
}
