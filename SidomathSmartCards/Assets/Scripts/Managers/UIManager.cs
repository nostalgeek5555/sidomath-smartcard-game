using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Gameplay UI Elements")]
    public TextMeshProUGUI actorTurnTMP;

    [Header("Popups")]
    public List<Popup> popups = new List<Popup>();
    public Dictionary<Popup.Type, Popup> popupTable = new Dictionary<Popup.Type, Popup>();

    [Header("Buttons")]
    public Button pauseButton;

    [Header("Pause UI")]
    public Image pauseLayerBG;

    [Header("Method Storage")]
    public Dictionary<MethodName, Action> NoCallbackMethods = new Dictionary<MethodName, Action>();
    public Dictionary<Delegate, object[]> Callbacks = new Dictionary<Delegate, object[]>();

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

        RegisterPopups();
        RegisterMethods();
    }

    public void RegisterMethods()
    {
        //register all method to using MethodName as key
        NoCallbackMethods = new Dictionary<MethodName, Action>();
        NoCallbackMethods.Add(MethodName.RESTART, RestartGame);
        NoCallbackMethods.Add(MethodName.HOME, Home);

        //pause button
        pauseButton.onClick.RemoveAllListeners();
        pauseButton.onClick.AddListener(() =>
        {
            Pause();
        });
    }

    public void RegisterPopups()
    {
        popupTable = new Dictionary<Popup.Type, Popup>();

        if (popups.Count > 0)
        {
            foreach (Popup popup in popups)
            {
                popupTable.Add(popup.type, popup);
            }
        }
    }

    private void OnDisable()
    {
        pauseButton.onClick.RemoveAllListeners();
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
        pauseButton.interactable = true;
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


    public void Pause()
    {
        Popup popup = popupTable[Popup.Type.PAUSE];
        popup.StateController(popup.type, Popup.State.OPEN);

        GameplayManager.Instance.StateController(GameplayManager.GameState.PAUSE);
    }



    public void RestartGame()
    {
        DOTween.KillAll();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void Home()
    {
        DOTween.KillAll();
        Time.timeScale = 1;
        SceneManager.LoadScene("HomeScene");
    }


    public enum ButtonWithPopup
    {
        PAUSE = 0
    }

    public enum MethodName
    {
        RESTART = 0,
        HOME = 1
    }

    #region POPUP UI HANDLERS

    public void HandleOnBeforePopupStateChange(Popup.Type type, Popup.State state)
    {
        Debug.Log($"Handle before state change, popup type = {type}, state = {state}");
        switch (type)
        {
            case Popup.Type.PAUSE:
                switch (state)
                {
                    case Popup.State.OPEN:
                        pauseLayerBG.gameObject.SetActive(true);
                        break;
                    case Popup.State.CLOSE:
                        pauseLayerBG.gameObject.SetActive(false);
                        GameplayManager.Instance.StateController(GameplayManager.GameState.RESUME);
                        break;
                    default:
                        break;
                }

                break;
            case Popup.Type.MENU:
                break;
            default:
                break;
        }
    }

    public void HandleOnAfterPopupStateChange(Popup.Type type, Popup.State state)
    {
        Debug.Log($"Handle after state change, popup type = {type}, state = {state}");
        switch (type)
        {
            case Popup.Type.PAUSE:
                switch (state)
                {
                    case Popup.State.OPEN:
                        pauseLayerBG.gameObject.SetActive(true);
                        break;
                    case Popup.State.CLOSE:
                        pauseLayerBG.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }

                break;
            case Popup.Type.MENU:
                break;
            default:
                break;
        }
    }

    #endregion
}
