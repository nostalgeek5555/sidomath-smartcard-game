using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using DG.Tweening;
using NaughtyAttributes;
using System.Linq;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    public Player player;
    public int totalEnemy;
    public GameObject cardPrefab;
    public Card cardSpawnedPrefab;
    public GameObject aiDeckHolderPrefab;
    public Transform dealerDeckHolder;

    [Header("Level Properties")]
    public int levelID;
    public string levelName;
    public List<Card> mainDecks = new List<Card>();
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
            StartCoroutine(InitGameplay(DataManager.Instance.levelTable["0|kakatua"]));
        }
    }

    public IEnumerator InitGameplay(LevelSO levelSO)
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
                sequence.Append(card.gameObject.transform.DOMove(deckHolders[deckHolderID].position, 0.3f, false));
                sequence.AppendCallback(() =>
                {
                    card.gameObject.transform.SetParent(deckHolders[deckHolderID]);
                    cardID++;
                    deckHolderID++;

                    Debug.Log($"current card id :: {cardID}");
                    DistributeCards(cards, cardID, deckHolderID);
                });
            }

            else
            {
                deckHolderID = 0;
                Sequence sequence = DOTween.Sequence();
                sequence.Append(card.gameObject.transform.DOMove(deckHolders[deckHolderID].position, 0.3f, false));
                sequence.AppendCallback(() =>
                {
                    card.gameObject.transform.SetParent(deckHolders[deckHolderID]);
                    cardID++;
                    deckHolderID++;

                    Debug.Log($"current card id :: {cardID}");
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

            //spawn card on picked tile
            Card spawnedCard = LeanPool.Spawn(cardSpawnedPrefab, point.transform);
            spawnedCard.InitCard(card._cardId, card._cardPairType, card.cardImage.sprite);
            spawnedCard.canvasGroup.alpha = 0;
            RectTransform rectCard = card.gameObject.transform.GetComponent<RectTransform>();
            RectTransform rectSpawnedCard = spawnedCard.gameObject.transform.GetComponent<RectTransform>();


            if (randomPos == 0 || randomPos == 1)
            {
                spawnedCard.transform.localRotation = Quaternion.Euler(0,0,90);
            }

            //drop last card to randomly picked tile on board
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.5f);
            sequence.Append(card.gameObject.transform.DOMove(point.position, 0.6f, false));
            sequence.Join(rectCard.DOSizeDelta(rectSpawnedCard.sizeDelta, 0.6f, false));
            sequence.Join(card.gameObject.transform.DORotate(spawnedCard.gameObject.transform.eulerAngles, 0.6f));
            sequence.AppendCallback(() =>
            {
                LeanPool.Despawn(card);
                mainDecks.Remove(card);
                spawnedCard.canvasGroup.alpha = 1;
                spawnedCard.transform.SetParent(pickedTile.transform);
                gamestart = true;
                Debug.Log($"current card id :: {cardID}");
            });
        }
    }
}
