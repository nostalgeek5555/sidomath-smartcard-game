using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorBase : MonoBehaviour
{
    public Role actorRole;

    [Header("Base Stats")]
    public int health;
    public bool turn = false;
    public bool gameover = false;


    [Header("Hand Card Properties")]
    public List<Card> handCards = new List<Card>();
    public HorizontalLayoutGroup handCardGroup;
    public ContentSizeFitter handCardFitter;


    public virtual void RegisterHandCards()
    {
        Debug.Log("register cards");
        handCards = new List<Card>();
        if (handCardGroup.transform.childCount > 0)
        {
            Debug.Log($"register for card total {handCardGroup.transform.childCount}");
            for (int i = 0; i < handCardGroup.transform.childCount; i++)
            {
                if (handCardGroup.transform.GetChild(i).GetComponent<Card>() != null)
                {
                    Card card = handCardGroup.transform.GetChild(i).GetComponent<Card>();
                    if (actorRole == Role.Player)
                    {
                        card.player = gameObject.GetComponent<Player>();
                    }

                    else
                    {
                        card.player = null;
                    }
                    handCards.Add(card);
                    Debug.Log($"card added id :: {card._cardId}");
                    Debug.Log($"card added pair :: {card._cardPairType}");
                    Debug.Log($"========================================");
                }
            }
        }
    }

    public enum Role
    {
        Player = 0,
        Ai = 1
    }
}
