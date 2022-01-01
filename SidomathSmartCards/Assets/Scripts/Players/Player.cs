using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class Player : MonoBehaviour
{
    [Header("Player Properties")]
    public PlayerType playerType;
    public PlayerState playerState;
    public int health = 3;
    public bool turn = false;
    public bool gameover = false;
    public List<Card> handCards = new List<Card>();


    [Header("Player Card Displays")]
    public Transform handCardParent;
    public HorizontalLayoutGroup handGroup;
    public ContentSizeFitter handCardFitter;
    public GameObject cardHighlightPicker;
    [SerializeField] private int pickedCardId;
    [SerializeField] private int pickedCardSiblingIndex;
    public Card prevCard;
    public int prevCardIndex;
    public Card pickedCard;



    public virtual void OnDisable()
    {

    }

    private void Start()
    {
        //BoardManager.Instance.OnCardDropped += 
        InitPlayer();
    }

    public void InitPlayer()
    {
        playerState = PlayerState.PROCESSING;
        turn = false;
        gameover = false;
    }

    public void RegisterHandCards()
    {
        if (playerType == PlayerType.Player)
        {
            handCardFitter.enabled = false;
            handGroup.enabled = false;
        }
        handCards = new List<Card>();
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<Card>() != null)
                {
                    Card card = transform.GetChild(i).GetComponent<Card>();
                    card.player = this;
                    handCards.Add(card);
                    Debug.Log($"card added id :: {card._cardId}");
                    Debug.Log($"card added pair :: {card._cardPairType}");
                    Debug.Log($"========================================");
                }
            }
        }
    }

    public virtual void StateController(PlayerState playerState)
    {
        switch(playerState)
        {
            case PlayerState.PROCESSING:
                break;
            case PlayerState.GET_TURN:
                OnGettingTurn();
                break;
            case PlayerState.END_TURN:
                OnEndingTurn();
                break;
            case PlayerState.SKIP_TURN:
                OnSkipTurn();
                break;
            case PlayerState.WIN:
                break;
            case PlayerState.GAME_OVER:
                break;

            default:
                break;
        }
    }

    public virtual void PickCard(Card card)
    {
        if (card.canBePick)
        {
            Debug.Log($"pick card on hand :: {card.cardType}");
            pickedCard = card;
            pickedCard.picked = true;
            pickedCard.cardHighlight.SetActive(true);
            pickedCard.overrideCanvas.overrideSorting = true;
            pickedCard.overrideCanvas.sortingOrder = 1;
        }


        UnpickCards(card._cardId);
    }

    public void UnpickCards(int cardId)
    {
        foreach (Card _card in handCards)
        {
            if (_card._cardId != cardId)
            {
                _card.picked = false;
                _card.cardHighlight.SetActive(false);
                _card.overrideCanvas.sortingOrder = 0;
                _card.overrideCanvas.overrideSorting = false;
            }
        }
    }

    public void OnCardTweenedBack(int cardId)
    {
        foreach (Card card in handCards)
        {
            if (card._cardId != cardId)
            {
                
                card.canBePick = false;
                card.draggable = false;
                card.cardHighlight.SetActive(false);
            }
        }
    }

    public void OnCardFinishTweenBack(int cardId, Card _card)
    {
        //handCardFitter.enabled = false;
        //handGroup.enabled = false;

        _card.canvasGroup.alpha = 1f;
        _card.transform.localScale = Vector3.one;
        _card.canvasGroup.blocksRaycasts = true;

        foreach (Card card in handCards)
        {
            card.canBePick = true;
            card.draggable = true;

            if (card._cardId == cardId)
            {
                PickCard(card);
            }
        }
    }

    public virtual void OnProcessing()
    {

    }


    public virtual void OnGettingTurn()
    {
        playerState = PlayerState.GET_TURN;

        turn = true;

        if (health > 1)
        {
            if (handCards.Count > 0)
            {
                Card card = handCards[handCards.Count - 1];
                PickCard(card);
            }

            else
            {
                StateController(PlayerState.WIN);
            }
        }

        else
        {
            StateController(PlayerState.GAME_OVER);
        }
        
    }

    public virtual void OnEndingTurn()
    {
        playerState = PlayerState.END_TURN;

    }

    public virtual void OnSkipTurn()
    {
        playerState = PlayerState.SKIP_TURN;
    }

    public virtual void OnWinning()
    {
        playerState = PlayerState.WIN;
    }

    public void OnGameOver()
    {
        playerState = PlayerState.GAME_OVER;
    }

    public virtual void NavigatingCards()
    {
        if (handCards.Count > 0 && playerState == PlayerState.GET_TURN)
        {

        }
    }


    public enum PlayerType
    {
        Player = 0,
        Ai = 1
    }

    public enum PlayerState
    {
        PROCESSING = 0,
        GET_TURN = 1,
        END_TURN = 2,
        SKIP_TURN = 3,
        WIN = 4,
        GAME_OVER = 5
    }
}
