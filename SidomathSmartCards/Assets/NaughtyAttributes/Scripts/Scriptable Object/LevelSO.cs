using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "Level SO", menuName = "Scriptable Object/Level SO")]
public class LevelSO : ScriptableObject
{
    public int levelId;
    public string levelName;
    public List<CardData> cardDatas;

    [Serializable]
    public class CardData
    {
        public int cardId;
        public string cardPairType;
        public int cardCount;
        public Sprite cardSprite;
    }
}
