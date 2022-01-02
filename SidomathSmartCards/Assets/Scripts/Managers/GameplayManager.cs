using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using DG.Tweening;
using System.Linq;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    public GameObject playerGO;
    public GameObject enemyGO;
    public Transform playerPanelParent;
    public Transform aiPanelParent;
    public Player player;
    public int totalEnemy;
    public GameObject cardPrefab;
    public Card cardSpawnedPrefab;
    public GameObject cardConnectorPrefab;
    public Transform dealerDeckHolder;

    [Header("Game Mode Properties")]
    public ModeSO currentGameMode;
    public TurnCycle turnCycle;
    public int currentPlayerTurnID;
    public float distributeCardTime;

    [Header("Level Properties")]
    public int levelID;
    public string levelName;
    public List<Card> mainDecks = new List<Card>();
    public List<Player> allPlayers = new List<Player>();
    public List<Transform> deckHolders = new List<Transform>();

    [Header("State Controller")]
    public bool gamestart = false;
    public bool gameover = false;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        else
        {
            if (Instance != this && Instance != null)
            {
                Destroy(gameObject);
            }
        }

        if (DataManager.Instance != null)
        {
            currentGameMode = DataManager.Instance.gameModeTable["Offline|SinglePlayer"];
            InitGameMode(currentGameMode);
            StartCoroutine(InitLevel(DataManager.Instance.levelTable["0|kakatua"]));
        }
    }

    //first time load gameplay
    public void InitGameMode(ModeSO modeSO)
    {
        allPlayers = new List<Player>();
        deckHolders = new List<Transform>();
        playerGO = modeSO.playerGO;
        enemyGO = modeSO.enemyAiGO;

        //set turn clockwise or counter clockwise
        Array values = Enum.GetValues(typeof(TurnCycle));
        turnCycle = (TurnCycle)values.GetValue(Random.Range(0, values.Length));
        Debug.Log($"Turn cycle type = {turnCycle}");


        //set gamemode from loaded scriptable object
        switch (modeSO.modeType)
        {
            case ModeSO.ModeType.Offline:
                switch (modeSO.playerModeType)
                {
                    case ModeSO.PlayerModeType.SinglePlayer:
                        GameObject _playerGo = LeanPool.Spawn(playerGO, playerPanelParent);
                        player = _playerGo.GetComponent<Player>();
                        RectTransform rect = _playerGo.GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(0, 0);
                        rect.offsetMin = new Vector2(0, 0);
                        allPlayers.Add(player);
                        deckHolders.Add(_playerGo.transform);

                        for (int i = 0; i < modeSO.totalEnemyAI; i++)
                        {
                            GameObject _aiGo = LeanPool.Spawn(enemyGO, aiPanelParent);
                            Player ai = _aiGo.GetComponent<Player>();
                            allPlayers.Add(ai);
                            deckHolders.Add(_aiGo.transform);
                        }

                        //set current turn id for main player
                        switch (turnCycle)
                        {
                            case TurnCycle.CLOCKWISE:
                                currentPlayerTurnID = allPlayers.IndexOf(player);
                                Debug.Log($"main player turn id == {currentPlayerTurnID}");
                                break;
                            case TurnCycle.COUNTER_CLOCKWISE:
                                allPlayers.Reverse();
                                currentPlayerTurnID = allPlayers.IndexOf(player);
                                Debug.Log($"main player turn id == {currentPlayerTurnID}");
                                break;
                            default:
                                break;
                        }
                        
                        

                        break;
                    case ModeSO.PlayerModeType.Multiplayer:
                        break;
                    default:
                        break;
                }

                break;
            case ModeSO.ModeType.Online:
                break;
            default:
                break;
        }
    }

    public IEnumerator InitLevel(LevelSO levelSO)
    {
        bool allCardSpawned = false;

        levelID = levelSO.levelId;
        levelName = levelSO.levelName;

        mainDecks = new List<Card>();
        
        if (levelSO.cardDatas.Count > 0)
        {
            for (int i = 0; i < levelSO.cardDatas.Count; i++)
            {
                LevelSO.CardData cardData = levelSO.cardDatas[i];
                
                for (int j = 0; j < cardData.cardCount; j++)
                {
                    GameObject cardGO = LeanPool.Spawn(cardPrefab, dealerDeckHolder);
                    Card cardComponent = cardGO.GetComponent<Card>();
                    cardComponent.InitCard(cardData.cardId, cardData.cardPairType, cardData.cardSprite);
                    mainDecks.Add(cardComponent);
                    Debug.Log($"current card id :: {cardComponent._cardId}");
                    Debug.Log($"current card pairtype :: {cardComponent._cardPairType}");
                    Debug.Log($"======================================================");
                }

                if (i == levelSO.cardDatas.Count)
                {
                    break;
                }
            }

            allCardSpawned = true;
            Debug.Log("all card spawned");

            yield return new WaitUntil(() => allCardSpawned == true);

            StartCoroutine(ShuffleDeck(DistributeCards, mainDecks));
        }
    }

    public IEnumerator ShuffleDeck(Action<List<Card>, int, int> action, List<Card> decks)
    {
        int shuffledCount = 0;
        if (decks.Count > 0)
        {
            for (int i = 0; i < decks.Count; i++)
            {
                Card tempCard = decks[i];
                int randomIndex = Random.Range(i, decks.Count);
                decks[i] = decks[randomIndex];
                decks[randomIndex] = tempCard;

                shuffledCount++;

                if (shuffledCount == decks.Count)
                {
                    Debug.Log($"Shuffle deck done {shuffledCount}");
                    break;
                }
            }
        }

        yield return new WaitUntil(() => shuffledCount == decks.Count);

        action?.Invoke(decks, 0, 0);
    }

    public void DistributeCards(List<Card> cards, int cardID, int deckHolderID)
    {
        Debug.Log("distribute cards");
        Card card = cards[cardID];

        if (cardID < cards.Count - 1)
        {
            //distribute to each deck holder start from player
            if (deckHolderID < deckHolders.Count)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.Append(card.gameObject.transform.DOMove(deckHolders[deckHolderID].position, distributeCardTime, false));
                sequence.AppendCallback(() =>
                {
                    card.gameObject.transform.SetParent(deckHolders[deckHolderID]);
                    card._cardId = card.transform.GetSiblingIndex();
                    cardID++;
                    deckHolderID++;

                    //Debug.Log($"current card id :: {cardID}");
                    DistributeCards(cards, cardID, deckHolderID);
                });
            }

            else
            {
                deckHolderID = 0;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(card.gameObject.transform.DOMove(deckHolders[deckHolderID].position, distributeCardTime, false));
                sequence.AppendCallback(() =>
                {
                    card.gameObject.transform.SetParent(deckHolders[deckHolderID]);
                    card._cardId = card.transform.GetSiblingIndex();
                    cardID++;
                    deckHolderID++;

                    //Debug.Log($"current card id :: {cardID}");
                    DistributeCards(cards, cardID, deckHolderID);
                });
            }
        }

        else
        {
            //pick random tile on board and place last card from dealer deck as initiator
            int randX = Random.Range(2, BoardManager.Instance.tilesOnBoards.GetLength(0) - 3);
            int randY = Random.Range(2, BoardManager.Instance.tilesOnBoards.GetUpperBound(1) - 3);
            Tile pickedTile = BoardManager.Instance.tilesOnBoards[randX, randY];
            pickedTile.droppedCard = card;
            pickedTile._placed = true;
            pickedTile._droppable = false;

            //pick random position on tiles
            int randomPos = Random.Range(0, pickedTile.tilePoints.Count - 1);
            Transform point = pickedTile.tilePoints[randomPos];

            //spawn card on picked tile and drop last card from dealer deck as initiator
            Card spawnedCard;
            switch (randomPos)
            {
                case 0:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Top, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllPlayers());
                    break;

                case 1:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Bottom, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllPlayers());
                    break;

                case 2:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Right, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllPlayers());
                    break;

                case 3:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Left, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllPlayers());
                    break;

                default:
                    break;
            }
        }
    }

    //spawn picked card on board
    public Card SpawnCardOnBoard(Card card, Tile tile, Transform transformParent, Tile.DroppedPoint droppedPoint, Tile.TopBottomSpecific topBottomSpecific, Action<Card, Card, Card> action = null, CardConnector _cardConnector = null, Card pairingCard = null)
    {
        Card spawnedCard = LeanPool.Spawn(cardSpawnedPrefab, transformParent);
        GameObject cardConnector = LeanPool.Spawn(cardConnectorPrefab, spawnedCard.transform);
        
        spawnedCard.cardType = Card.CardType.OnBoard;
        spawnedCard.currentCardConnector = _cardConnector;
        spawnedCard.mainConnector = cardConnector;
        spawnedCard.topCardConnector = spawnedCard.mainConnector.transform.GetChild(0).GetComponent<Transform>();
        spawnedCard.bottomCardConnector = spawnedCard.mainConnector.transform.GetChild(1).GetComponent<Transform>();
        spawnedCard._cardId = card._cardId;
        spawnedCard.facing = Card.Facing.Vertical;
        spawnedCard._cardPairType = card._cardPairType;
        spawnedCard.cardImage.sprite = card.cardImage.sprite;
        spawnedCard.onHand = false;
        spawnedCard.dropped = true;
        spawnedCard.flipped = false;
        spawnedCard.droppedPoint = droppedPoint;
        spawnedCard.topBottomSpecific = topBottomSpecific;
        spawnedCard.canvasGroup.alpha = 0;
        spawnedCard.currentTile = tile;
        spawnedCard.currentTile._placed = true;
        spawnedCard.currentTile.droppedPoint = droppedPoint;
        spawnedCard.tileIndex = spawnedCard.currentTile.tileIndex;

        BoardManager.Instance.allDroppedCards.Add(spawnedCard);

        switch (droppedPoint)
        {
            case Tile.DroppedPoint.Top:
                spawnedCard.transform.localRotation = Quaternion.Euler(0, 0, 90);
                spawnedCard.facing = Card.Facing.Horizontal;
                BoardManager.Instance.RegisterAllTileOnCardConnectors(spawnedCard);
                action?.Invoke(card, spawnedCard, pairingCard);
                break;
            case Tile.DroppedPoint.Bottom:
                spawnedCard.transform.localRotation = Quaternion.Euler(0, 0, 90);
                spawnedCard.facing = Card.Facing.Horizontal;
                BoardManager.Instance.RegisterAllTileOnCardConnectors(spawnedCard);
                action?.Invoke(card, spawnedCard, pairingCard);
                break;
            case Tile.DroppedPoint.Left:
                BoardManager.Instance.RegisterAllTileOnCardConnectors(spawnedCard);
                action?.Invoke(card, spawnedCard, pairingCard);
                break;
            case Tile.DroppedPoint.Right:
                BoardManager.Instance.RegisterAllTileOnCardConnectors(spawnedCard);
                action?.Invoke(card, spawnedCard, pairingCard);
                break;
            default:
                break;
        }

        return spawnedCard;
    }

    public void MoveCardToBoard(Card card, Card spawnedCard, Transform targetPos, float moveDuration, Action action = null, IEnumerator enumerator = null)
    {
        RectTransform cardRect = card.GetComponent<RectTransform>();
        RectTransform targetRect = spawnedCard.GetComponent<RectTransform>();

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.3f);
        sequence.Append(card.gameObject.transform.DOMove(targetPos.position, moveDuration, false));
        sequence.Join(cardRect.DOSizeDelta(targetRect.sizeDelta, moveDuration, false));
        sequence.Join(card.gameObject.transform.DORotate(spawnedCard.gameObject.transform.eulerAngles, moveDuration));
        sequence.AppendCallback(() =>
        {
            LeanPool.Despawn(card);
            mainDecks.Remove(card);
            spawnedCard.canvasGroup.alpha = 1;
            spawnedCard.transform.SetParent(targetPos.parent.parent);
            action?.Invoke();
            StartCoroutine(enumerator);
        });
    }

    //public void SetCardConnector(Card card)
    //{
    //    if (card.transform.childCount > 0)
    //    {
    //        for (int i = 0; i < card.transform.childCount; i++)
    //        {
    //            if(card.transform.GetChild(i).GetComponent<CardConnector>)
    //        }
    //    }
    //}

    public IEnumerator SetupAllPlayers()
    {
        Debug.Log("setup all player");
        if (allPlayers.Count > 0)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                Player player = allPlayers[i];
                player.RegisterHandCards();
                Debug.Log($"Register card for player :: {i + 1}");

                if (i == allPlayers.Count - 1)
                {
                    gamestart = true;
                    Debug.Log($"all player setup done, gamestart == {gamestart}");
                    break;
                }
            }
        }

        yield return new WaitUntil(() => gamestart == true);

        //player.PickCard(player.handCards[player.handCards.Count - 1]);

        //after all players (player and enemy ai) register their hand card, init player turn
        player.StateController(Player.PlayerState.GET_TURN);
    }

    public void TurnShifter()
    {
        switch (turnCycle)
        {
            case TurnCycle.CLOCKWISE:
                if (currentPlayerTurnID < allPlayers.Count - 1)
                {
                    currentPlayerTurnID++;
                    Player player = allPlayers[currentPlayerTurnID];
                    player.StateController(Player.PlayerState.GET_TURN);
                    Debug.Log($"current player getting turn = {player.playerType}");
                }

                else
                {
                    currentPlayerTurnID = 0;
                    Player player = allPlayers[currentPlayerTurnID];
                    player.StateController(Player.PlayerState.GET_TURN);
                    Debug.Log($"current player getting turn = {player.playerType}");
                }
                break;

            case TurnCycle.COUNTER_CLOCKWISE:
                if (currentPlayerTurnID > 0)
                {
                    currentPlayerTurnID--;
                    Player player = allPlayers[currentPlayerTurnID];
                    player.StateController(Player.PlayerState.GET_TURN);
                    Debug.Log($"current player getting turn = {player.playerType}");
                }

                else
                {
                    currentPlayerTurnID = allPlayers.Count - 1;
                    Player player = allPlayers[currentPlayerTurnID];
                    player.StateController(Player.PlayerState.GET_TURN);
                    Debug.Log($"current player getting turn = {player.playerType}");
                }
                break;
            default:
                break;
        }
    }

    public enum TurnCycle
    {
        CLOCKWISE = 0,
        COUNTER_CLOCKWISE = 1
    }
}
