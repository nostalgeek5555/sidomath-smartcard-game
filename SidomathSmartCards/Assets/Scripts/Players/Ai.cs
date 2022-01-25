using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Random = UnityEngine.Random;
using System.Linq;

public class Ai : ActorBase
{
    public static event Action<States> OnBeforeStateChange;
    public static event Action<States> OnAfterStateChange;
    public static event Action<Card> OnCardMatch;

    public States state;

    [Header("Behavior Properties")]
    [MinMaxSlider(0f, 100f)]
    public Vector2 getTurnChance;

    [MinMaxSlider(-1f, -1f)]
    public Vector2 skipTurnChance;

    [Header("UI elements")]
    public Animator animator;

    private void OnEnable()
    {
        OnCardMatch += OnMatchingCardEnd;
    }

    private void OnDisable()
    {
        OnCardMatch -= OnMatchingCardEnd;
    }

    public void StateController(States states)
    {
        OnBeforeStateChange?.Invoke(states);

        state = states;

        switch (states)
        {
            case States.INIT:
                HandleInitiation();
                break;
            case States.PROCESSING_TURN:
                //HandleProcessingTurn(CheckMatchingCard);
                HandleProcessingTurn(BoardManager.Instance.OnDroppedCardMatch);
                break;
            case States.GET_TURN:
                HandleGetTurn();
                break;
            case States.END_TURN:
                StartCoroutine(HandleEndTurn());
                break;
            case States.SKIP_TURN:
                HandleSkipTurn();
                break;
            case States.WIN:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(states), states, null);
        }

        OnAfterStateChange?.Invoke(states);
    }

    public void HandleBeforeStateChange(States states)
    {

    }

    public void HandleAfterStateChange(States states)
    {

    }

    public override void RegisterHandCards()
    {
        base.RegisterHandCards();
    }

    private void HandleInitiation()
    {
        turn = false;
        gameover = false;
        RegisterHandCards();
    }

    public void OnMatchingCardEnd(Card card)
    {
        if (card.matched)
        {
            if (handCards.Count > 0)
            {
                StateController(States.END_TURN);
            }

            else
            {
                GameplayManager.Instance.StateController(GameplayManager.GameState.GAME_OVER);
            }
        }

        else
        {
            StateController(States.END_TURN);
        }
    }

    #region GETTING_TURN
    private void HandleGetTurn()
    {
        SetUIOnGettingTurn();

        Debug.Log($"ai number {GameplayManager.Instance.allActors.IndexOf(GetComponent<ActorBase>())}");
        GameplayManager.Instance.actorGettingTurn = GetComponent<ActorBase>();
        float randomChance = Random.Range(0f, 100f);
        Debug.Log($"ai number {GameplayManager.Instance.allActors.IndexOf(GetComponent<ActorBase>())} {randomChance}");
        if (randomChance >= skipTurnChance.x && randomChance <= skipTurnChance.y)
        {
            Debug.Log($"ai number {GameplayManager.Instance.allActors.IndexOf(GetComponent<ActorBase>())} skip turn");
            StateController(States.SKIP_TURN);
        }

        else
        {
            Debug.Log("processing turn");
            StateController(States.PROCESSING_TURN);
        }
    }

    private void HandleProcessingTurn(Action<Card, Card, CardConnector> action = null)
    {
        Debug.Log("processing turn");
        int randomPickCardSide = Random.Range(0, 1);
        Card pickedCardSide = BoardManager.Instance.droppableCardList[randomPickCardSide];
        ParentCardConnector parentCardConnector = pickedCardSide.mainConnector.GetComponent<ParentCardConnector>();
        string[] cardTokens = pickedCardSide._cardPairType.Split('|');

        //these parameters needed for filtering cards in ai hands which matched current picked token in pairing card
        string pickedToken;
        int randomConnectorPick;
        CardConnector cardConnector;
        IEnumerable<Card> filteredCards;
        List<Card> filteredCardList = new List<Card>();

        switch (pickedCardSide.facing)
        {
            case Card.Facing.Vertical:
                switch (pickedCardSide.matchedSide)
                {
                    case Card.MatchedSide.Left:
                        pickedToken = cardTokens[1];
                        cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();
                        Debug.Log($"get card connector type {cardConnector.cardConnectorType}");

                        if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        else
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        break;

                    case Card.MatchedSide.Right:
                        pickedToken = cardTokens[0];
                        cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();
                        Debug.Log($"get card connector type {cardConnector.cardConnectorType}");

                        if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopRight)
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        else
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        break;
                    default:
                        break;
                }

                break;
            case Card.Facing.Horizontal:

                switch (pickedCardSide.matchedSide)
                {                                                                                                        
                    case Card.MatchedSide.Left:
                        pickedToken = cardTokens[1];
                        cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();
                        Debug.Log($"get card connector type {cardConnector.cardConnectorType}");

                        if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        else
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        break;
                    case Card.MatchedSide.Right:
                        pickedToken = cardTokens[0];
                        cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();
                        Debug.Log($"get card connector type {cardConnector.cardConnectorType}");

                        if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopLeft)
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }

                        else
                        {
                            filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                            filteredCardList = filteredCards.ToList();

                            Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                            randomPickHandCard.matched = true;
                            randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                            pickedCardSide.matchedSide = Card.MatchedSide.Both;

                            action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                        }
                        break;
                    default:
                        break;
                }

                break;
            default:
                break;
        }
    }

    private IEnumerator FindMatchTokenInCards(string token, Card pairingCard, CardConnector cardConnector)
    {
        bool cardMatchFound = false;
        bool cardMatchNotFound = false;
        int totalHandCards = 0;
        Card matchCard = null;

        if (handCards.Count > 0)
        {
            foreach (Card card in handCards)
            {
                totalHandCards++;
                string[] tokens = card._cardPairType.Split('|');
                string handCardToken;

                if (pairingCard.matchedSide == Card.MatchedSide.Left)
                {
                    if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                    {
                        handCardToken = tokens[1];
                        if (handCardToken == token)
                        {
                            Card _matchCard = card;
                            matchCard = _matchCard;
                            matchCard.matchedSide = Card.MatchedSide.Right;
                            pairingCard.matchedSide = Card.MatchedSide.Both;
                            cardMatchFound = true;
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }

                    else
                    {
                        handCardToken = tokens[0];
                        if (handCardToken == token)
                        {
                            Card _matchCard = card;
                            matchCard = _matchCard;
                            cardMatchFound = true;
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }
                }

                else
                {
                    if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopLeft)
                    {
                        handCardToken = tokens[0];
                        if (handCardToken == token)
                        {
                            Card _matchCard = card;
                            matchCard = _matchCard;
                            cardMatchFound = true;
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }

                    else
                    {
                        handCardToken = tokens[1];
                        if (handCardToken == token)
                        {
                            Card _matchCard = card;
                            matchCard = _matchCard;
                            cardMatchFound = true;
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        if (cardMatchFound)
        {
            totalHandCards = handCards.Count;
        }

        Debug.Log($"total hand card count {totalHandCards}");
        yield return new WaitUntil(() => totalHandCards == handCards.Count);

        if (totalHandCards == handCards.Count && cardMatchFound == false)
        {
            Debug.Log($"matched card not found so skip turn");
            StateController(States.SKIP_TURN);
        }

        else
        {
            Debug.Log($"match card found with pair {matchCard._cardPairType}");
            BoardManager.Instance.OnDroppedCardMatch(matchCard, pairingCard, cardConnector);
        }
    }
    #endregion

#region END_TURN
    private IEnumerator HandleEndTurn()
    {
        yield return new WaitForSeconds(1.5f);

        SetUIOnEndingTurn();
        GameplayManager.Instance.StateController(GameplayManager.GameState.SHIFT_TURN);
    }

    #endregion

    #region UI_Handler
    public void SetUIOnGettingTurn()
    {
        //animator.ResetTrigger(States.END_TURN.ToString());
        animator.SetTrigger(States.GET_TURN.ToString());
    }

    public void SetUIOnEndingTurn()
    {
        //animator.ResetTrigger(States.GET_TURN.ToString());
        animator.SetTrigger(States.END_TURN.ToString());
    }

    #endregion

    private void HandleSkipTurn()
    {
        StateController(States.END_TURN);
    }


    public enum States
    {
        INIT = 0,
        PROCESSING_TURN = 1,
        GET_TURN = 2,
        END_TURN = 3,
        SKIP_TURN = 4,
        WIN = 5
    }
}
