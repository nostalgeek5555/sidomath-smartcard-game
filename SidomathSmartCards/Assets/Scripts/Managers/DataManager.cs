using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public List<ModeSO> gameModeList;
    public Dictionary<string, ModeSO> gameModeTable;
    public List<LevelSO> levelList;
    public Dictionary<string, LevelSO> levelTable;

    //for viewing in editor
    public List<string> gameModeAddedKeys = new List<string>();
    public List<string> levelAddedKeys = new List<string>();
    public List<LevelSO.CardData> levelCardDatas = new List<LevelSO.CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }

        LoadResources();
        LoadMode();
    }

    public void LoadMode()
    {
        gameModeList = new List<ModeSO>(Resources.LoadAll<ModeSO>("Scriptable Object/Mode"));
        gameModeTable = new Dictionary<string, ModeSO>();

        for (int i = 0; i < gameModeList.Count; i++)
        {
            ModeSO modeSO = gameModeList[i];
            string key = modeSO.modeType.ToString() + "|" + modeSO.playerModeType.ToString();

            gameModeTable.Add(key, modeSO);
        }
    }

    private void LoadResources()
    {
        levelList = new List<LevelSO>(Resources.LoadAll<LevelSO>("Scriptable Object/Level"));
        levelTable = new Dictionary<string, LevelSO>();

        for (int i = 0; i < levelList.Count; i++)
        {
            LevelSO levelSO = levelList[i];
            string key = levelSO.levelId + "|" + levelSO.levelName;

            levelTable.Add(key, levelSO);
            Debug.Log($"Level id added :: {levelTable[key].levelId}");
            Debug.Log($"Level name added :: {levelTable[key].levelName}");
            Debug.Log($"=================================================");
        }
    }

#if UNITY_EDITOR
    private void Update()
    {

      if (levelTable.Count > 0)
        {
            gameModeAddedKeys = gameModeTable.Keys.ToList();
            levelAddedKeys = levelTable.Keys.ToList();
            levelCardDatas = levelTable.Values.SelectMany(val => val.cardDatas).ToList();
        }  
    }
#endif
}
