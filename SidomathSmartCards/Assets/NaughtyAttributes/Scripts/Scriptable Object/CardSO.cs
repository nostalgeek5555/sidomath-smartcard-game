using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Card SO", menuName = "Scriptable Object/Card So")]
public class CardSO : ScriptableObject
{
    public int cardId;
    public string cardName;
    public string cardPairType;
    public List<Sprite> sprites;

}
