using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[Serializable]
public class Tile : MonoBehaviour
{
    [Header("Tile Properties")]
    public string tileIndex;
    [SerializeField] private bool droppable = true;
    [SerializeField] private bool placed = false;

    [Header("Tile Display Component")]
    public Sprite tileSprite;
    [ReorderableList] public List<Transform> tilePoints;

    [Header("Properties if card dropped on Tile")]
    public DroppedPoint droppedPoint;
    public TopBottomSpecific topBottomSpecificPoint;
    public Card droppedCard;
    public BoxCollider2D boxCollider;

    public bool _droppable { get => droppable; set => droppable = value; }
    public bool _placed { get => placed; set => placed = value; }

    [Header("Properties for Card Connector")]
    public bool haveCardConnector = false;

    public void InitTile(string id)
    {
        tileIndex = id;
        _droppable = true;
        _placed = false;
        haveCardConnector = false;
        droppedPoint = DroppedPoint.Empty;
        topBottomSpecificPoint = TopBottomSpecific.None;
    }

    public void SetTileDropped(Card card)
    {
        droppedCard = card;
        droppable = false;
        placed = true;
        droppedPoint = card.droppedPoint;

        switch (droppedPoint)
        {
            case DroppedPoint.Top:
                topBottomSpecificPoint = card.topBottomSpecific;
                break;
            case DroppedPoint.Bottom:
                topBottomSpecificPoint = card.topBottomSpecific;
                break;
            default:
                topBottomSpecificPoint = TopBottomSpecific.None;
                break;
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision != null)
    //    {
    //        if (collision.TryGetComponent(out CardConnector cardConnector))
    //        {
    //            cardConnector.tileBelow = this;
    //            boxCollider.enabled = false;
    //        }
    //    }
    //}

    public enum DroppedPoint
    {
        Empty = 0,
        Top = 1,
        Bottom = 2,
        Left = 3,
        Right = 4
    }

    public enum TopBottomSpecific
    {
        None = 0,
        Mid = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5
    }
}
