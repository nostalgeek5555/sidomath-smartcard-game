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

    private void Start()
    {
        InitPlayer();    
    }

    public void InitPlayer()
    {
        playerState = PlayerState.Idle;
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

    public void StateController(PlayerState playerState)
    {
        switch(playerState)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Turn:
                OnGettingTurn();
                break;
            case PlayerState.EndTurn:
                OnEndingTurn();
                break;
            case PlayerState.SkipTurn:
                OnSkipTurn();
                break;
            case PlayerState.GameOver:
                break;
            default:
                break;
        }
    }

    public virtual void PickCard(Card card)
    {
        Debug.Log($"pick card on hand :: {card.cardType.ToString()}");
        pickedCard = card;
        pickedCard.picked = true;
        pickedCard.cardHighlight.SetActive(true);
        pickedCard.overrideCanvas.overrideSorting = true;
        pickedCard.overrideCanvas.sortingOrder = 1;

       foreach (Card _card in handCards)
       {
            if (_card._cardId != card._cardId)
            {
                _card.picked = false;
                _card.cardHighlight.SetActive(false);
                _card.overrideCanvas.sortingOrder = 0;
                _card.overrideCanvas.overrideSorting = false;
            }
       }
    }


    public virtual void OnGettingTurn()
    {
        playerState = PlayerState.Turn;
    }

    public virtual void OnEndingTurn()
    {
        playerState = PlayerState.EndTurn;

    }

    public virtual void OnSkipTurn()
    {
        playerState = PlayerState.SkipTurn;
    }

    public virtual void NavigatingCards()
    {
        if (handCards.Count > 0 && playerState == PlayerState.Turn)
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
        Idle = 0,
        Turn = 1,
        EndTurn = 2,
        SkipTurn = 3,
        GameOver = 4
    }
}
