using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Lean.Pool;
using System.Linq;

[Serializable]
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public static event Action<Card, Card> OnAfterCardMatch;


    public Transform boardParent;
    public Transform topLeftPoint;
    public Transform bottomLeftPoint;
    public Transform topRightPoint;
    public Tile tilePrefab;
    public List<Tile> tilesOnBoard;
    public Tile[,] tilesOnBoards;
    public TileData[] tileDatas;
    [SerializeField] private int width, height;

    //card properties on board 
   
    [SerializeField] private string firstDroppedTileIndex;
    [SerializeField] private string mostLeftTileIndex;
    [SerializeField] private string mostRightTileIndex;
    public List<Tile> droppedTile = new List<Tile>();
    public List<Card> droppableCardList = new List<Card>();
    public List<Card> allDroppedCards = new List<Card>();
    public Card mostLeftCard = null;
    public Card mostRightCard = null;

    private void OnEnable()
    {
        OnAfterCardMatch += HandleAfterCardMatch;
    }

    private void OnDisable()
    {
        OnAfterCardMatch -= HandleAfterCardMatch;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }

        GenerateBoard(topLeftPoint.position, topRightPoint.position, bottomLeftPoint.position);
    }

    private void GenerateBoard(Vector3 _pointA, Vector3 _pointB, Vector3 _pointC)
    {
        tilesOnBoard = new List<Tile>();
        tilesOnBoards = new Tile[width, height];
        tileDatas = new TileData[width];
        allDroppedCards = new List<Card>();
        droppableCardList = new List<Card>();
        //GameObject container = new GameObject();
        //GameObject boardContainer = LeanPool.Spawn(new GameObject(), boardParent);
        //boardContainer.name = "Board Container";
        //boardContainer.transform.position = _pointC;
        //float spriteWidth = Vector3.Distance(_pointA, _pointB) / width;
        //float spriteHeight = Vector3.Distance(_pointA, _pointC) / height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile tile = LeanPool.Spawn(tilePrefab, boardParent);
                string tileId = x + "|" + y;
                tile.InitTile(tileId);
                //tile.transform.localPosition = new Vector3(x * spriteWidth, y * spriteHeight, -1);
                //tile.transform.localScale = new Vector3(spriteWidth, spriteHeight, 1);

                tilesOnBoard.Add(tile);
                tilesOnBoards[x, y] = tile;
                //tileDatas[x].tiles[y] = tile;
                //Debug.Log($"Tiles on board {x} {y} :: {tilesOnBoards[x, y]._tileIndex}");
            }
        }
    }

    public void RegisterAllTileOnCardConnectors(Card card)
    {
        string[] tileIndexTokens = card.tileIndex.Split('|');
        int tileIndexX = Convert.ToInt32(tileIndexTokens[0]);
        int tileIndexY = Convert.ToInt32(tileIndexTokens[1]);

        int boardWidth = tilesOnBoards.GetLength(0);
        int boardHeight = tilesOnBoards.GetLength(1);
        Debug.Log($"board width == {boardWidth}");
        Debug.Log($"board height == {boardHeight}");

        switch (card.facing)
        {
            case Card.Facing.Vertical:
                for (int i = 0; i < card.topCardConnector.childCount; i++)
                {
                    CardConnector cardConnector = card.topCardConnector.GetChild(i).GetComponent<CardConnector>();
                    Tile connectedTile; 

                    switch (cardConnector.cardConnectorType)
                    {
                        case CardConnector.CardConnectorType.Top:
                            if (tileIndexY - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY - 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;

                        case CardConnector.CardConnectorType.TopRight:
                            if (tileIndexX + 1 < boardWidth)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX + 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        case CardConnector.CardConnectorType.TopLeft:
                            if (tileIndexX - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX - 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < card.bottomCardConnector.childCount; i++)
                {
                    CardConnector cardConnector = card.bottomCardConnector.GetChild(i).GetComponent<CardConnector>();
                    Tile connectedTile;

                    switch (cardConnector.cardConnectorType)
                    {
                        case CardConnector.CardConnectorType.Bottom:
                            if (tileIndexY + 1 < boardHeight)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;
                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY + 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;

                        case CardConnector.CardConnectorType.BottomRight:
                            if (tileIndexX + 1 < boardWidth)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;
                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX + 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        case CardConnector.CardConnectorType.BottomLeft:
                            if (tileIndexX - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;
                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX - 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;

            case Card.Facing.Horizontal:
                for (int i = 0; i < card.topCardConnector.childCount; i++)
                {
                    CardConnector cardConnector = card.topCardConnector.GetChild(i).GetComponent<CardConnector>();
                    Tile connectedTile;

                    switch (cardConnector.cardConnectorType)
                    {
                        case CardConnector.CardConnectorType.Top:
                            if (tileIndexX - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX - 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;

                        case CardConnector.CardConnectorType.TopRight:
                            if (tileIndexY - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY - 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        case CardConnector.CardConnectorType.TopLeft:
                            if (tileIndexY + 1 < boardHeight)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Left;
                                cardConnector.parentCardPairingIndex = 0;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY + 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < card.bottomCardConnector.childCount; i++)
                {
                    CardConnector cardConnector = card.bottomCardConnector.GetChild(i).GetComponent<CardConnector>();
                    Tile connectedTile;

                    switch (cardConnector.cardConnectorType)
                    {
                        case CardConnector.CardConnectorType.Bottom:
                            if (tileIndexX + 1 < boardWidth)
                            {
                                cardConnector.facing = CardConnector.Facing.HORIZONTAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;
                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX + 1, tileIndexY];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;

                        case CardConnector.CardConnectorType.BottomRight:
                            if (tileIndexY - 1 >= 0)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Right;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;
                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 1;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY - 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        case CardConnector.CardConnectorType.BottomLeft:
                            if (tileIndexY + 1 < boardHeight)
                            {
                                cardConnector.facing = CardConnector.Facing.VERTICAL;
                                cardConnector.matchedSide = Card.MatchedSide.Left;
                                cardConnector.parentMatchedSide = Card.MatchedSide.Right;

                                cardConnector.parentCardPairingIndex = 1;
                                cardConnector.droppedCardPairingIndex = 0;
                                connectedTile = tilesOnBoards[tileIndexX, tileIndexY + 1];
                                cardConnector.tileBelow = connectedTile;
                                cardConnector.tileBelow.haveCardConnector = true;
                                cardConnector.gameObject.SetActive(true);
                                cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
                                Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
                            }

                            else
                            {
                                cardConnector.tileBelow = null;
                                cardConnector.gameObject.SetActive(false);
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void CheckIfTileHasConnector(CardConnector cardConnector, Tile tile)
    {
        if (!tile.haveCardConnector)
        {
            cardConnector.tileBelow = tile;
            cardConnector.tileBelow.haveCardConnector = true;
            cardConnector.gameObject.SetActive(true);
            cardConnector.cardConnectorTileIndex = cardConnector.tileBelow.tileIndex;
            Debug.Log($"get tile below index {cardConnector.tileBelow.tileIndex}");
        }

        else
        {
            cardConnector.tileBelow = null;
            cardConnector.gameObject.SetActive(false);
        }
    }

    public void OnDropCard(Card card, Card parentCard, CardConnector cardConnector)
    {
        Debug.Log($"card pair type parent {parentCard._cardPairType}");
        string[] cardTokens = card._cardPairType.Split('|');
        string[] parentTokens = parentCard._cardPairType.Split('|');

       if (parentCard.matchedSide == Card.MatchedSide.None)
       {
            switch (parentCard.facing)
            {
                case Card.Facing.Vertical:
                    if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Top
                        || cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopLeft)
                    {
                        if (cardTokens[1] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Left;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
                    {
                        if (cardTokens[1] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Right;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Bottom
                            || cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                    {
                        if (cardTokens[0] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Right;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else
                    {
                        if (cardTokens[0] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Left;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    break;
                case Card.Facing.Horizontal:
                    if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Top
                        || cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopRight)
                    {
                        if (cardTokens[1] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Left;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                    {
                        if (cardTokens[1] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Right;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Bottom
                            || cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
                    {
                        if (cardTokens[0] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Right;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }

                    else
                    {
                        if (cardTokens[0] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Left;
                            parentCard.matched = true;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                    }
                    break;
                default:
                    break;
            }
       }

       else if (parentCard.matchedSide == Card.MatchedSide.Right)
       {
            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Top)
            {
                if (cardTokens[1] == parentTokens[0])
                {
                    card.matchedSide = Card.MatchedSide.Right;
                    parentCard.matchedSide = Card.MatchedSide.Both;
                    OnDroppedCardMatch(card, parentCard, cardConnector);
                }

                else
                {
                    Debug.Log($"not match {cardTokens[1]} with {parentTokens[0]}");
                    card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                }
            }
            
            else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopRight)
            {
                switch (parentCard.facing)
                {
                    case Card.Facing.Vertical:
                        if (cardTokens[0] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[1]} with {parentTokens[0]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;

                    case Card.Facing.Horizontal:
                        if (cardTokens[1] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[1]} with {parentTokens[0]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    default:
                        break;
                }
            }

            else
            {
                switch (parentCard.facing)
                {
                    case Card.Facing.Vertical:
                        if (cardTokens[1] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[0]} with {parentTokens[0]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;

                    case Card.Facing.Horizontal:
                        if (cardTokens[0] == parentTokens[0])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[0]} with {parentTokens[0]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    default:
                        break;
                }
                
            }
       }

        else if (parentCard.matchedSide == Card.MatchedSide.Left)
        {
            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.Bottom)
            {
                if (cardTokens[0] == parentTokens[1])
                {
                    card.matchedSide = Card.MatchedSide.Left;
                    parentCard.matchedSide = Card.MatchedSide.Both;
                    OnDroppedCardMatch(card, parentCard, cardConnector);
                }

                else
                {
                    Debug.Log($"not match {cardTokens[0]} with {parentTokens[1]}");
                    card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                }
            }

            else if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
            {
                switch (parentCard.facing)
                {
                    case Card.Facing.Vertical:
                        if (cardTokens[1] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[1]} with {parentTokens[1]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    case Card.Facing.Horizontal:
                        if (cardTokens[0] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[1]} with {parentTokens[1]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    default:
                        break;
                }
            }

            else
            {
                switch (parentCard.facing)
                {
                    case Card.Facing.Vertical:
                        if (cardTokens[0] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Left;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[0]} with {parentTokens[1]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    case Card.Facing.Horizontal:
                        if (cardTokens[1] == parentTokens[1])
                        {
                            card.matchedSide = Card.MatchedSide.Right;
                            parentCard.matchedSide = Card.MatchedSide.Both;
                            OnDroppedCardMatch(card, parentCard, cardConnector);
                        }

                        else
                        {
                            Debug.Log($"not match {cardTokens[1]} with {parentTokens[1]}");
                            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        else
        {
            card.TweenBack(GameplayManager.Instance.player.OnCardFinishTweenBack);
        }
    }

    public void AddCardToDroppableList(Card parentCard, Card card = null)
    {
        Debug.Log("add card to droppable list");
        switch (parentCard.matchedSide)
        {
            case Card.MatchedSide.None:
                if (card != null)
                {
                    droppableCardList.Add(card);
                    droppableCardList.Add(parentCard);
                }

                else
                {
                    droppableCardList.Add(parentCard);
                }
                
                break;
            case Card.MatchedSide.Left:
                droppableCardList.Add(card);
                droppableCardList.Add(parentCard);

                droppableCardList = droppableCardList.Distinct().ToList();
                break;
            case Card.MatchedSide.Right:
                droppableCardList.Add(card);
                droppableCardList.Add(parentCard);

                droppableCardList = droppableCardList.Distinct().ToList();
                break;
            case Card.MatchedSide.Both:
                droppableCardList.Remove(parentCard);
                droppableCardList.Add(card);
                break;
            default:
                Debug.Log("card not added");
                break;
        }
    }

    public void OnDroppedCardMatch(Card card, Card pairingCard, CardConnector cardConnector)
    {
        switch (pairingCard.facing)
        {
            case Card.Facing.Vertical:
                switch (cardConnector.cardConnectorType)
                {
                    case CardConnector.CardConnectorType.Top:
                        switch (pairingCard.droppedPoint)
                        {
                            case Tile.DroppedPoint.Right:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            case Tile.DroppedPoint.Left:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            default:
                                break;
                        }
                        
                        break;

                    case CardConnector.CardConnectorType.TopRight:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Top, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.TopLeft:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Top, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.Bottom:
                        switch (pairingCard.droppedPoint)
                        {
                            case Tile.DroppedPoint.Right:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            case Tile.DroppedPoint.Left:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            default:
                                break;
                        }

                        break;
                    case CardConnector.CardConnectorType.BottomLeft:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Bottom, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.BottomRight:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Bottom, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    default:
                        break;
                }

                break;

            case Card.Facing.Horizontal:
                switch (cardConnector.cardConnectorType)
                {
                    case CardConnector.CardConnectorType.Top:
                        switch (pairingCard.droppedPoint)
                        {
                            case Tile.DroppedPoint.Top:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            case Tile.DroppedPoint.Bottom:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            default:
                                break;
                        }

                        break;
                    case CardConnector.CardConnectorType.TopRight:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Left, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.TopLeft:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Left, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.Bottom:
                        switch (pairingCard.droppedPoint)
                        {
                            case Tile.DroppedPoint.Top:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            case Tile.DroppedPoint.Bottom:
                                GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, pairingCard.droppedPoint, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);
                                break;
                            default:
                                break;
                        }

                        break;
                    case CardConnector.CardConnectorType.BottomLeft:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Right, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    case CardConnector.CardConnectorType.BottomRight:
                        GameplayManager.Instance.SpawnCardOnBoard(card, cardConnector.tileBelow, cardConnector.transform, Tile.DroppedPoint.Right, Tile.TopBottomSpecific.None, OnDoneMatchingCards, cardConnector, pairingCard);

                        break;
                    default:
                        break;
                }

                break;
            default:
                break;
        }
    }

    public void OnDoneMatchingCards(Card card, Card spawnedCard, Card pairingCard)
    {
        card.draggable = false;
        spawnedCard.matchedSide = card.matchedSide;
        spawnedCard.pairingCard = pairingCard;
        pairingCard.matched = true;
        spawnedCard.matched = true;
        spawnedCard.canvasGroup.blocksRaycasts = true;
        spawnedCard.transform.SetParent(spawnedCard.currentTile.transform);

        switch (GameplayManager.Instance.actorGettingTurn.actorRole)
        {
            case ActorBase.Role.Player:
                spawnedCard.canvasGroup.alpha = 1;
                break;
            case ActorBase.Role.Ai:
                spawnedCard.canvasGroup.alpha = 0;
                break;
            default:
                break;
        }

        GameplayManager.Instance.player.handCardGroup.enabled = true;
        GameplayManager.Instance.player.handCardFitter.enabled = true;

        AddCardToDroppableList(pairingCard, spawnedCard);

        switch (pairingCard.facing)
        {
            case Card.Facing.Horizontal:
                if (spawnedCard.currentCardConnector.cardConnectorType == CardConnector.CardConnectorType.Top ||
                    spawnedCard.currentCardConnector.cardConnectorType == CardConnector.CardConnectorType.Bottom)
                {
                    spawnedCard.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }

                else
                {
                    spawnedCard.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                break;
            default:
                break;
        }

        OnAfterCardMatch?.Invoke(card, spawnedCard);
        
    }

    public void HandleAfterCardMatch(Card card, Card spawnedCard)
    {
        //after all handler for matching card done their job, give signal to state controller for shifting turn
        switch (GameplayManager.Instance.actorGettingTurn.actorRole)
        {
            case ActorBase.Role.Player:
                Debug.Log($"On matching end, role {GameplayManager.Instance.actorGettingTurn.actorRole}");
                GameplayManager.Instance.player.handCards.Remove(card);
                LeanPool.Despawn(card);
                ResetDroppableAreas();

                Player player = GameplayManager.Instance.actorGettingTurn.GetComponent<Player>();
                player.OnMatchingCardEnd(spawnedCard);
                break;

            case ActorBase.Role.Ai:
                Debug.Log($"On matching end, role {GameplayManager.Instance.actorGettingTurn.actorRole}");
                GameplayManager.Instance.MoveCardToBoard(card, spawnedCard, spawnedCard.currentCardConnector.transform, 0.6f, ResetDroppableAreas, null, GameplayManager.Instance.RemoveCard);

                Ai ai = GameplayManager.Instance.actorGettingTurn.GetComponent<Ai>();
                ai.OnMatchingCardEnd(spawnedCard, card);
                break;
            default:
                break;
        }
    }


    public void ResetDroppableAreas()
    {
        if (allDroppedCards.Count > 0)
        {
            CardConnector cardConnector;

            foreach (Card card in allDroppedCards)
            {
                card.topCardConnector.gameObject.SetActive(false);
                card.bottomCardConnector.gameObject.SetActive(false);

                if (card.matched)
                {
                    switch (card.matchedSide)
                    {
                        case Card.MatchedSide.Left:
                            card.bottomCardConnector.gameObject.SetActive(true);
                            for (int i = 0; i < card.bottomCardConnector.childCount; i++)
                            {
                                cardConnector = card.bottomCardConnector.GetChild(i).GetComponent<CardConnector>();
                                
                                if (cardConnector.tileBelow != null && !cardConnector.tileBelow._placed)
                                {
                                    cardConnector.gameObject.SetActive(true);
                                    cardConnector.animator.SetTrigger(CardConnector.HighlightType.Droppable.ToString());
                                    cardConnector.droppable = true;
                                    cardConnector.dropped = false;
                                }

                                else
                                {
                                    cardConnector.gameObject.SetActive(false);
                                }
                            }
                            break;

                        case Card.MatchedSide.Right:
                            card.topCardConnector.gameObject.SetActive(true);
                            for (int i = 0; i < card.topCardConnector.childCount; i++)
                            {
                                cardConnector = card.topCardConnector.GetChild(i).GetComponent<CardConnector>();

                                if (cardConnector.tileBelow != null && !cardConnector.tileBelow._placed)
                                {
                                    cardConnector.gameObject.SetActive(true);
                                    cardConnector.animator.SetTrigger(CardConnector.HighlightType.Droppable.ToString());
                                    cardConnector.droppable = true;
                                    cardConnector.dropped = false;
                                }

                                else
                                {
                                    cardConnector.gameObject.SetActive(false);
                                }
                            }
                            break;

                        case Card.MatchedSide.Both:
                            card.flipped = true;
                            card.topCardConnector.gameObject.SetActive(false);
                            card.bottomCardConnector.gameObject.SetActive(false);
                            break;

                        default:
                            break;
                    }
                }

                else
                {
                    card.topCardConnector.gameObject.SetActive(true);
                    card.bottomCardConnector.gameObject.SetActive(true);

                    if (card.mainConnector.transform.childCount > 0)
                    {
                        
                        for (int i = 0; i < card.mainConnector.transform.childCount; i++)
                        {
                            Transform branchConnector = card.mainConnector.transform.GetChild(i).GetComponent<Transform>();
                            if (branchConnector.childCount > 0)
                            {
                                for (int j = 0; j < branchConnector.childCount; j++)
                                {
                                    cardConnector = branchConnector.GetChild(j).GetComponent<CardConnector>();
                                    cardConnector.animator.SetTrigger(CardConnector.HighlightType.Droppable.ToString());
                                    cardConnector.droppable = true;
                                    cardConnector.dropped = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (tilesOnBoard.Count > 0)
        {
            for (int i = 0; i < tilesOnBoard.Count; i++)
            {
                Tile tile = tilesOnBoard[i];
                Gizmos.DrawIcon(tile.transform.GetChild(0).position, "Light Gizmo.tiff", true);
            }
        }
    }

    [Serializable]
    public struct TileData
    {
        public Tile[] tiles;
    }

}
