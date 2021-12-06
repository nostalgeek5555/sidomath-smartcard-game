using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player)), CanEditMultipleObjects]
public class PlayerCustomEditor : Editor
{
    public SerializedProperty
        playerType,
        playerState,
        playerHealth,
        playerTurn,
        playerGameover,
        playerHandCards,
        handCardParent,
        handCardGroup,
        handCardFitter,
        cardHighlight,
        pickedCardId,
        pickedCard;


    private void OnEnable()
    {
        playerType = serializedObject.FindProperty("playerType");
        playerState = serializedObject.FindProperty("playerState");
        playerHealth = serializedObject.FindProperty("health");
        playerTurn = serializedObject.FindProperty("turn");
        playerGameover = serializedObject.FindProperty("gameover");
        playerHandCards = serializedObject.FindProperty("handCards");
        handCardGroup = serializedObject.FindProperty("handGroup");
        handCardFitter = serializedObject.FindProperty("handCardFitter");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(playerType);
        EditorGUILayout.PropertyField(playerState);
        EditorGUILayout.PropertyField(playerHandCards);
        EditorGUILayout.PropertyField(handCardGroup);
        EditorGUILayout.PropertyField(handCardFitter);

        //show player properties based on player type
        Player.PlayerType _playerType = (Player.PlayerType)playerType.enumValueIndex;

        switch (_playerType)
        {
            case Player.PlayerType.Player:
                EditorGUILayout.PropertyField(playerHealth);
                EditorGUILayout.PropertyField(playerTurn);
                EditorGUILayout.PropertyField(playerGameover);
                break;

            case Player.PlayerType.Ai:
                EditorGUILayout.PropertyField(playerTurn);
                break;

            default:
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
