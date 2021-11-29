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
    public GameObject aiDeckHolderPrefab;
    public Transform dealerDeckHolder;

    [Header("Level Properties")]
    public int levelID;
    public string levelName;
    public List<Card> mainDecks = new List<Card>();
    public List<Transform> deckHolders = new List<Transform>();
    
    

    [Header("State Controller")]
    public bool gameover;

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
        if (cardID < cards.Count)
        {
            Card card = cards[cardID];
            //cards.RemoveAt(cardID);

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
            cards.Clear();
        }
    }
}
