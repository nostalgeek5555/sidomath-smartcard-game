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

    public States state;

    [Header("Behavior Properties")]
    [MinMaxSlider(0f, 100f)]
    public Vector2 getTurnChance;

    [MinMaxSlider(0f, 100f)]
    public Vector2 skipTurnChance;

    [Header("UI elements")]
    public Animator animator;

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

    public void OnMatchingCardEnd(Card card, Card parentCard)
    {
        handCards.Remove(parentCard);

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
        float randomChance = Random.Range(0, 100);
        Debug.Log($"ai number {GameplayManager.Instance.allActors.IndexOf(GetComponent<ActorBase>())} {randomChance}");
        if (randomChance >= getTurnChance.x && randomChance <= getTurnChance.y)
        {
            Debug.Log("processing turn");
            StateController(States.PROCESSING_TURN);
        }

        else if (randomChance <= skipTurnChance.y)
        {
            Debug.Log($"ai number {GameplayManager.Instance.allActors.IndexOf(GetComponent<ActorBase>())} skip turn, chance == {randomChance}");
            StateController(States.SKIP_TURN);
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
                    case Card.MatchedSide.None:
                        int randomSide = Random.Range(0, 1);

                        //if randomSide == 0, then pick top side connectors
                        if (randomSide == 0)
                        {
                            cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();

                            if (cardConnector != null)
                            {
                                if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopRight)
                                {
                                    pickedToken = cardTokens[0];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Right;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                        StateController(States.SKIP_TURN);
                                    }
                                }

                                else
                                {
                                    pickedToken = cardTokens[1];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Left;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                        StateController(States.SKIP_TURN);
                                    }
                                }
                            }

                            else
                            {
                                Debug.Log($"skip turn because of no card connector found");
                                StateController(States.SKIP_TURN);
                            }
                        }

                        //else if randomSide == 1, then pick bottom side connectors
                        else
                        {
                            cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();

                            if (cardConnector != null)
                            {
                                if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
                                {
                                    pickedToken = cardTokens[1];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Left;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                        StateController(States.SKIP_TURN);
                                    }

                                }

                                else
                                {
                                    pickedToken = cardTokens[0];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Right;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                        StateController(States.SKIP_TURN);
                                    }
                                }
                            }

                            else
                            {
                                Debug.Log($"skip turn because of no card connector found");
                                StateController(States.SKIP_TURN);
                            }
                        }
                        break;

                    case Card.MatchedSide.Left:
                        pickedToken = cardTokens[1];
                        cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();

                        if (cardConnector != null)
                        {
                            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomLeft)
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }

                            }

                            else
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }
                        }

                        else
                        {
                            Debug.Log($"skip turn because of no card connector found");
                            StateController(States.SKIP_TURN);
                        }

                        break;

                    case Card.MatchedSide.Right:
                        pickedToken = cardTokens[0];
                        cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();

                        if (cardConnector != null)
                        {
                            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopRight)
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }

                            else
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }
                        }

                        else
                        {
                            Debug.Log($"skip turn because of no card connector found");
                            StateController(States.SKIP_TURN);
                        }
                        break;
                    default:
                        break;
                }

                break;
            case Card.Facing.Horizontal:

                switch (pickedCardSide.matchedSide)
                {
                    case Card.MatchedSide.None:
                        int randomSide = Random.Range(0, 1);

                        //if randomside == 0, then pick top connectors
                        if (randomSide == 0)
                        {
                            cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();

                            if (cardConnector != null)
                            {
                                if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopLeft)
                                {
                                    pickedToken = cardTokens[0];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Right;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        StateController(States.SKIP_TURN);
                                    }
                                }

                                else
                                {
                                    pickedToken = cardTokens[1];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Left;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        StateController(States.SKIP_TURN);
                                    }
                                }
                            }

                            else
                            {
                                StateController(States.SKIP_TURN);
                            }
                        }
                        //else if randomside == 1, then pick bottom connectors
                        else
                        {
                            cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();

                            if (cardConnector != null)
                            {
                                if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                                {
                                    pickedToken = cardTokens[1];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Left;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        StateController(States.SKIP_TURN);
                                    }

                                }

                                else
                                {
                                    pickedToken = cardTokens[0];
                                    filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                    filteredCardList = filteredCards.ToList();

                                    if (filteredCardList.Count > 0)
                                    {
                                        Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                        randomPickHandCard.matched = true;
                                        randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                        pickedCardSide.matchedSide = Card.MatchedSide.Right;

                                        action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                    }

                                    else
                                    {
                                        StateController(States.SKIP_TURN);
                                    }
                                }
                            }

                            else
                            {
                                StateController(States.SKIP_TURN);
                            }
                        }
                        break;
                    case Card.MatchedSide.Left:
                        pickedToken = cardTokens[1];
                        cardConnector = parentCardConnector.GetRandomFromActiveBotConnectors();

                        if (cardConnector != null)
                        {
                            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.BottomRight)
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }

                            }

                            else
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }
                        }

                        else
                        {
                            Debug.Log($"skip turn because of no card connector found");
                            StateController(States.SKIP_TURN);
                        }
                        break;

                    case Card.MatchedSide.Right:
                        pickedToken = cardTokens[0];
                        cardConnector = parentCardConnector.GetRandomFromActiveTopConnectors();

                        if (cardConnector != null)
                        {
                            if (cardConnector.cardConnectorType == CardConnector.CardConnectorType.TopLeft)
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(0) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Left;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }

                            else
                            {
                                filteredCards = handCards.OrderBy(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken).Where(_card => _card._cardPairType.Split('|').ElementAt(1) == pickedToken);
                                filteredCardList = filteredCards.ToList();

                                if (filteredCardList.Count > 0)
                                {
                                    Card randomPickHandCard = filteredCardList[Random.RandomRange(0, filteredCardList.Count - 1)];
                                    randomPickHandCard.matched = true;
                                    randomPickHandCard.matchedSide = Card.MatchedSide.Right;
                                    pickedCardSide.matchedSide = Card.MatchedSide.Both;

                                    action?.Invoke(randomPickHandCard, pickedCardSide, cardConnector);
                                }

                                else
                                {
                                    Debug.Log($"skip turn because of none matched card found == {filteredCardList.Count}");
                                    StateController(States.SKIP_TURN);
                                }
                            }
                        }

                        else
                        {
                            Debug.Log($"skip turn because of no card connector found");
                            StateController(States.SKIP_TURN);
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
