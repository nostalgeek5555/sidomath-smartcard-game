using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using NaughtyAttributes;

public class AiHands : MonoBehaviour
{
    [Header("Ai Properties")]
    public int id;
    public List<Card> handCards = new List<Card>();
    public bool getTurn = false;
    public bool cardDepleted = false;


    public void InitAi(int _id)
    {
        id = _id;
        getTurn = false;
        cardDepleted = false;
    }
}
