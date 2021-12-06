using UnityEditor;

[CustomEditor(typeof(ModeSO)), CanEditMultipleObjects]
public class ModeCustomEditor : Editor
{
    public SerializedProperty
        playerModeType,
        modeType,
        player,
        enemyAI,
        totalEnemyAI;

    private void OnEnable()
    {
        playerModeType = serializedObject.FindProperty("playerModeType");
        modeType = serializedObject.FindProperty("modeType");
        player = serializedObject.FindProperty("playerGO");
        enemyAI = serializedObject.FindProperty("enemyAiGO");
        totalEnemyAI = serializedObject.FindProperty("totalEnemyAI");
    }


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        //main properties for game mode
        EditorGUILayout.PropertyField(playerModeType);
        EditorGUILayout.PropertyField(modeType);
        EditorGUILayout.PropertyField(player);
        EditorGUILayout.PropertyField(enemyAI);

        ModeSO.ModeType _modeType = (ModeSO.ModeType)modeType.enumValueIndex;
        ModeSO.PlayerModeType _playerModeType = (ModeSO.PlayerModeType)playerModeType.enumValueIndex;

        switch (_modeType)
        {
            case ModeSO.ModeType.Offline:
                switch (_playerModeType)
                {
                    case ModeSO.PlayerModeType.SinglePlayer:
                        EditorGUILayout.PropertyField(totalEnemyAI);
                        break;
                    case ModeSO.PlayerModeType.Multiplayer:
                        break;
                    default:
                        break;
                }

                break;
            case ModeSO.ModeType.Online:
                break;
            default:
                break;
        }

        serializedObject.ApplyModifiedProperties();

    }
}
