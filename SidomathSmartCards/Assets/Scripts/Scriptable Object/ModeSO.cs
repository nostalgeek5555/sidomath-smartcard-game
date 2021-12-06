using UnityEngine;

[CreateAssetMenu(fileName = "Mode SO", menuName = "Scriptable Object/Mode")]
public class ModeSO : ScriptableObject
{
    public PlayerModeType playerModeType;
    public ModeType modeType;
    public GameObject playerGO;
    public GameObject enemyAiGO;

    //properties for offline mode
    public int totalEnemyAI;

    //properties for online mode
    public enum PlayerModeType
    {
        SinglePlayer = 0,
        Multiplayer = 1
    }

    public enum ModeType
    {
        Offline = 0,
        Online = 1
    }
}
