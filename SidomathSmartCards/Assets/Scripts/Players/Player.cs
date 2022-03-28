using System;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class Player : ActorBase
{
    [Header("Player Properties")]
    public PlayerType playerType;
    public PlayerState playerState;


    [Header("Player Card Displays")]
    public Transform handCardParent;
    public GameObject cardHighlightPicker;
    [SerializeField] private int pickedCardId;
    [SerializeField] private int pickedCardSiblingIndex;
    public Card prevCard;
    public int prevCardIndex;
    public Card pickedCard;

    public Button skipButton;

    private void Start()
    {
        InitPlayer();
    }

    private void OnDisable()
    {
        skipButton.onClick.RemoveAllListeners();
    }

    public void InitPlayer()
    {
        playerState = PlayerState.PROCESSING;
        health = 3;
        turn = false;
        gameover = false;

        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() =>
        {
            HandleSkipTurn();
        });
    }

    public override void RegisterHandCards()
    {
        handCardFitter.enabled = false;
        handCardGroup.enabled = false;

        base.RegisterHandCards();

    }



    public void StateController(PlayerState playerState)
    {
        switch(playerState)
        {

            case PlayerState.GET_TURN:
                HandleGettingTurn();
                break;
            case PlayerState.END_TURN:
                HandleEndingTurn();
                break;
            case PlayerState.SKIP_TURN:
                HandleEndingTurn();
                break;
            case PlayerState.WIN:
                HandleWinning();
                break;
            case PlayerState.GAME_OVER:
                HandleGameOver();
                break;
            case PlayerState.PAUSE:
                HandlePause();
                break;
            case PlayerState.RESUME:
                HandleResume();
                break;
            case PlayerState.RESHUFFLE:
                HandleOnReshuffle();
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
        _card.onTweeningBack = false;
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

    public void OnMatchingCardEnd(Card card)
    {
        if (!card.matched)
        {
            health--;

            if (health > 0)
            {
                Debug.Log($"card not match, current player health {health}");
                StateController(PlayerState.END_TURN);
            }

            else
            {
                StateController(PlayerState.GAME_OVER);
            }
        }

        else
        {
            Debug.Log("card match");
            StateController(PlayerState.END_TURN);
        }
    }

    private void HandleGettingTurn()
    {
        playerState = PlayerState.GET_TURN;
        GameplayManager.Instance.actorGettingTurn = GetComponent<ActorBase>();

        pickedCard = null;
        turn = true;
        skipButton.enabled = true;
        
        foreach (Card card in handCards)
        {
            card.canBePick = true;
            card.draggable = true;
            card.overrideCanvas.enabled = true;
            card.overrideCanvas.sortingOrder = 0;
        }
        


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

    private void HandleSkipTurn()
    {
        skipButton.enabled = false;
        foreach (Card card in handCards)
        {
            card.canBePick = false;
            card.draggable = false;
            card.cardHighlight.SetActive(false);
        }

        Debug.Log("skip turn");
        if (playerState == PlayerState.GET_TURN)
        {
            playerState = PlayerState.SKIP_TURN;
            StateController(PlayerState.END_TURN);
        }
    }
    private void HandleEndingTurn()
    {
        Debug.Log("player ending turn");
        playerState = PlayerState.END_TURN;
        turn = false;
        GameplayManager.Instance.StateController(GameplayManager.GameState.SHIFT_TURN);
    }

    private void HandleWinning()
    {
        playerState = PlayerState.WIN;
        GameplayManager.Instance.StateController(GameplayManager.GameState.WIN);
    }

    private void HandleGameOver()
    {
        playerState = PlayerState.GAME_OVER;
        GameplayManager.Instance.StateController(GameplayManager.GameState.GAME_OVER);
    }

    private void HandlePause()
    {
        pickedCard.overrideCanvas.enabled = false;
        pickedCard.overrideCanvas.sortingOrder = 0;
        skipButton.enabled = false;
        foreach (Card card in handCards)
        {
            card.canBePick = false;
            card.draggable = false;
            card.cardHighlight.SetActive(false);
        }
    }

    private void HandleResume()
    {
        if (pickedCard != null)
        {
            pickedCard.overrideCanvas.enabled = true;
            pickedCard.overrideCanvas.sortingOrder = 1;
            pickedCard.cardHighlight.SetActive(true);
        }
        
        skipButton.enabled = true;

        foreach (Card card in handCards)
        {
            card.canBePick = true;
            card.draggable = true;
        }
    }

    private void HandleOnReshuffle()
    {
        handCardFitter.enabled = true;
        handCardGroup.enabled = true;
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
        GAME_OVER = 5,
        PAUSE = 6,
        RESUME = 7,
        RESHUFFLE = 8
    }
}
