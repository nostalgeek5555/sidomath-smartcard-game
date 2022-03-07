using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Gameplay UI Elements")]
    public TextMeshProUGUI actorTurnTMP;

    [Header("Player Gameplay UI")]
    public Button playerSkipButton;

    void Start()
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
    }

    public void OnAfterGameplayStateChangeUI(GameplayManager.GameState gameState)
    {
        switch (gameState)
        {
            case GameplayManager.GameState.INIT:
                InitUIGameplay();
                break;
            case GameplayManager.GameState.START_GAME:
                OnStartGameplay();
                break;
            case GameplayManager.GameState.SHIFT_TURN:
                break;
            case GameplayManager.GameState.SKIP_TURN:
                break;
            case GameplayManager.GameState.WIN:
                break;
            case GameplayManager.GameState.GAME_OVER:
                break;
            default:
                break;
        }
    }

    public void InitUIGameplay()
    {
        actorTurnTMP.gameObject.SetActive(false);
    }

    public void OnStartGameplay()
    {
        actorTurnTMP.gameObject.SetActive(true);
    }

    public void ChangeTurnUI()
    {
        if (GameplayManager.Instance.actorGettingTurn.actorRole == ActorBase.Role.Player)
        {
            actorTurnTMP.text = ActorBase.Role.Player.ToString();
        }

        else
        {
            actorTurnTMP.text = "Enemy " + GameplayManager.Instance.actorTurnID;
        }
    }
}
