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
    public static event Action<GameState> OnBeforeStateChange;
    public static event Action<GameState> OnAfterStateChange;
    public static event Action<ActorBase.Role> OnActorGettingTurn;

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

    [Header("Gameloop Properties")]
    public GameState gameState;
    public int actorTurnID;
    public ActorBase actorGettingTurn;

    [Header("Game Mode Properties")]
    public ModeSO currentGameMode;
    public TurnCycle turnCycle;
    public float distributeCardTime;

    [Header("Level Properties")]
    public int levelID;
    public string levelName;
    public List<Card> mainDecks = new List<Card>();
    public List<ActorBase> allActors = new List<ActorBase>();
    public List<Transform> deckHolders = new List<Transform>();

    [Header("State Controller")]
    public bool gamestart = false;
    public bool gameover = false;

    private void OnEnable()
    {
        OnBeforeStateChange += HandleBeforeStateChange;
        OnAfterStateChange += HandleAfterStateChange;
        OnActorGettingTurn += HandleOnActorGettingTurn;
    }

    private void OnDisable()
    {
        OnBeforeStateChange -= HandleBeforeStateChange;
        OnAfterStateChange -= HandleAfterStateChange;
        OnActorGettingTurn -= HandleOnActorGettingTurn;
    }

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
            StateController(GameState.INIT);
        }
    }

    public void HandleBeforeStateChange(GameState _gameState)
    {
        switch (_gameState)
        {
            case GameState.INIT:
                break;
            case GameState.START_GAME:
                break;
            case GameState.SHIFT_TURN:
                //HandleShiftTurn();
                break;
            case GameState.WIN:
                break;
            case GameState.GAME_OVER:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_gameState), _gameState, null);
        }
    }

    public void HandleAfterStateChange(GameState gameState)
    {

    }

    public void StateController(GameState _gameState)
    {
        OnBeforeStateChange?.Invoke(_gameState);
        
        gameState = _gameState;

        switch (gameState)
        {
            case GameState.INIT:
                HandleInitiation();
                break;
            case GameState.START_GAME:
                player.StateController(Player.PlayerState.GET_TURN);
                break;
            case GameState.SHIFT_TURN:
                StartCoroutine(HandleShiftTurn());
                break;
            case GameState.WIN:
                break;
            case GameState.GAME_OVER:
                HandleGameOver();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_gameState), _gameState, null);
        }

        UIManager.Instance.OnAfterGameplayStateChangeUI(gameState);
        OnAfterStateChange?.Invoke(_gameState);
    }


    public void HandleInitiation()
    {
        currentGameMode = DataManager.Instance.gameModeTable["Offline|SinglePlayer"];
        InitGameMode(currentGameMode);
        StartCoroutine(InitLevel(DataManager.Instance.levelTable["0|kakatua"]));
    }

    public IEnumerator HandleShiftTurn()
    {
        switch (turnCycle)
        {
            case TurnCycle.CLOCKWISE:
                if (actorTurnID < allActors.Count - 1)
                {
                    actorTurnID++;
                    ActorBase actor = allActors[actorTurnID];
                    actorGettingTurn = actor;
                }

                else
                {
                    actorTurnID = 0;
                    ActorBase actor = allActors[actorTurnID];
                    actorGettingTurn = actor;
                }
                break;

            case TurnCycle.COUNTER_CLOCKWISE:
                if (actorTurnID > 0)
                {
                    actorTurnID--;
                    ActorBase actor = allActors[actorTurnID];
                    actorGettingTurn = actor;
                }

                else
                {
                    actorTurnID = allActors.Count - 1;
                    ActorBase actor = allActors[actorTurnID];
                    actorGettingTurn = actor;
                }
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(1.0f);

        Debug.Log("shifting turn");
        OnActorGettingTurn?.Invoke(actorGettingTurn.actorRole);
    }

    public void HandleGameOver()
    {
        gameover = true;
        Debug.Log("GAME OVER");
    }

 #region HANDLE_INITIATION
    //first time load gameplay
    public void InitGameMode(ModeSO modeSO)
    {
        allActors = new List<ActorBase>();
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
                        ActorBase playerActor = _playerGo.GetComponent<ActorBase>();
                        RectTransform rect = _playerGo.GetComponent<RectTransform>();
                        rect.offsetMax = new Vector2(0, 0);
                        rect.offsetMin = new Vector2(0, 0);
                        allActors.Add(playerActor);
                        deckHolders.Add(player.handCardGroup.transform);

                        for (int i = 0; i < modeSO.totalEnemyAI; i++)
                        {
                            GameObject _aiGo = LeanPool.Spawn(enemyGO, aiPanelParent);
                            ActorBase ai = _aiGo.GetComponent<ActorBase>();
                            allActors.Add(ai);
                            deckHolders.Add(ai.handCardGroup.transform);
                        }

                        //set current turn id for main player
                        switch (turnCycle)
                        {
                            case TurnCycle.CLOCKWISE:
                                actorTurnID = allActors.IndexOf(playerActor);
                                Debug.Log($"main player turn id == {actorTurnID}");
                                break;
                            case TurnCycle.COUNTER_CLOCKWISE:
                                allActors.Reverse();
                                actorTurnID = allActors.IndexOf(playerActor);
                                Debug.Log($"main player turn id == {actorTurnID}");
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
            int randomPos = Random.Range(/*0, pickedTile.tilePoints.Count - 1*/0,1);
            Transform point = pickedTile.tilePoints[randomPos];

            //spawn card on picked tile and drop last card from dealer deck as initiator
            Card spawnedCard;
            switch (randomPos)
            {
                case 0:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Top, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllActors(), RemoveCard);
                    break;

                case 1:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Bottom, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllActors(), RemoveCard);
                    break;

                case 2:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Right, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllActors(), RemoveCard);
                    break;

                case 3:
                    spawnedCard = SpawnCardOnBoard(card, pickedTile, point, Tile.DroppedPoint.Left, Tile.TopBottomSpecific.Mid, null, null);
                    MoveCardToBoard(card, spawnedCard, point, 0.6f, BoardManager.Instance.ResetDroppableAreas, SetupAllActors(), RemoveCard);
                    break;

                default:
                    break;
            }
        }
    }
    #endregion

 #region HANDLE_SHIFT TURN
    public void HandleOnActorGettingTurn(ActorBase.Role role)
    {
        UIManager.Instance.ChangeTurnUI();

        switch (role)
        {
            case ActorBase.Role.Player:
                Player player = actorGettingTurn.GetComponent<Player>();
                player.StateController(Player.PlayerState.GET_TURN);
                break;

            case ActorBase.Role.Ai:
                Ai ai = actorGettingTurn.GetComponent<Ai>();
                ai.StateController(Ai.States.GET_TURN);
                break;
            default:
                break;
        }
    }

    public void HandleOnActorEndingTurn(ActorBase.Role role)
    {
        switch (role)
        {
            case ActorBase.Role.Player:
                Player player = actorGettingTurn.GetComponent<Player>();
                player.StateController(Player.PlayerState.END_TURN);
                break;

            case ActorBase.Role.Ai:
                Ai ai = actorGettingTurn.GetComponent<Ai>();
                ai.StateController(Ai.States.END_TURN);
                break;
            default:
                break;
        }
    }
 #endregion

 #region GAME_CORELOOP
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

    public void MoveCardToBoard(Card card, Card spawnedCard, Transform targetPos, float moveDuration, Action action = null, IEnumerator enumerator = null, Action<Card> _action = null)
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
            card.canvasGroup.alpha = 0;
            card.cardType = Card.CardType.OnBoard;
            spawnedCard.canvasGroup.alpha = 1;
            if (BoardManager.Instance.allDroppedCards.Count <= 1)
            {
                spawnedCard.transform.SetParent(targetPos.parent.parent);
            }
            _action?.Invoke(card);
            action?.Invoke();
            StartCoroutine(enumerator);
        });
    }

    public void RemoveCard(Card card)
    {
        LeanPool.Despawn(card);
        mainDecks.Remove(card);
    }
    #endregion

    public IEnumerator SetupAllActors()
    {
        Debug.Log("setup all player");
        if (allActors.Count > 0)
        {
            for (int i = 0; i < allActors.Count; i++)
            {
                ActorBase actor = allActors[i];
                actor.RegisterHandCards();
                Debug.Log($"Register card for player :: {i + 1}");

                if (i == allActors.Count - 1)
                {
                    gamestart = true;
                    Debug.Log($"all player setup done, gamestart == {gamestart}");
                    break;
                }
            }
        }

        yield return new WaitUntil(() => gamestart == true);

        StateController(GameState.START_GAME);
    }

    public enum TurnCycle
    {
        CLOCKWISE = 0,
        COUNTER_CLOCKWISE = 1
    }

    public enum GameState
    {
        INIT = 0,
        START_GAME = 1,
        SHIFT_TURN = 2,
        SKIP_TURN = 3,
        WIN = 4,
        GAME_OVER = 5
    }
}
