using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using TMPro;

public class Popup : MonoBehaviour
{
    public Type type;
    public State state;
    public static event Action<Type, State> OnBeforeStateChange;
    public static event Action<Type, State> OnAfterStateChange;

    public TextMeshProUGUI popupName;

    public GameObject buttonPrefab;
    public Button triggerButton;
    public Button closeButton;
    public List<UIManager.MethodName> buttonMethods = new List<UIManager.MethodName>();

    [Header("Button Properties")]
    public Transform buttonHolder;


    public void Init()
    {
        popupName.text = type.ToString();
        gameObject.name = "POPUP :: " + popupName;

        if (buttonMethods.Count > 0)
        {
            Debug.Log($"methods count {buttonMethods.Count}");
            for (int i = 0; i < buttonMethods.Count; i++)
            {
                GameObject buttonGO = LeanPool.Spawn(buttonPrefab, buttonHolder);
                ButtonExtender buttonExtender = buttonGO.GetComponent<ButtonExtender>();

                buttonExtender.Init(buttonMethods[i]);
                Debug.Log($"init button method name {buttonMethods[i]}");
                ////add listener to existing buttons in popup if buttons already exists
                //if (buttonHolder.childCount > 0)
                //{
                //    if (buttonHolder.GetChild(i).TryGetComponent(out ButtonExtender _buttonExtender))
                //    {
                //        if (_buttonExtender.buttonMethodName == buttonMethods[i])
                //        {
                //            _buttonExtender.Init(buttonMethods[i]);

                //        }
                //    }
                //}

                ////spawn and add listener to all buttons if buttons hadn't exist
                //else
                //{
                //    GameObject buttonGO = LeanPool.Spawn(buttonPrefab, buttonHolder);
                //    ButtonExtender buttonExtender = buttonGO.GetComponent<ButtonExtender>();

                //    buttonExtender.Init(buttonMethods[i]);
                //    popupButtons.Add(buttonExtender);
                //}
            }
        }

        //close popup button
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            StateController(type, State.CLOSE);
        });
    }

    private void OnEnable()
    {
        OnBeforeStateChange += UIManager.Instance.HandleOnBeforePopupStateChange;
        OnAfterStateChange += UIManager.Instance.HandleOnAfterPopupStateChange;
    }

    private void OnDisable()
    {
        OnBeforeStateChange -= UIManager.Instance.HandleOnBeforePopupStateChange;
        OnAfterStateChange -= UIManager.Instance.HandleOnAfterPopupStateChange;

        closeButton.onClick.RemoveAllListeners();

        if (buttonHolder.childCount > 0)
        {
            for (int i = buttonHolder.childCount - 1; i >= 0; i--)
            {
                ButtonExtender button = buttonHolder.GetChild(i).GetComponent<ButtonExtender>();
                button.onClick.RemoveAllListeners();

                Debug.Log($"despawned button {button.buttonMethodName}");
                LeanPool.Despawn(button);
            }
        }
    }


    public void Open()
    {
        gameObject.SetActive(true);
        Init();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void StateController(Type _type, State _state)
    {
        OnBeforeStateChange?.Invoke(_type, _state);

        state = _state;

        switch (_state)
        {
            case State.OPEN:
                Open();
                break;
            case State.CLOSE:
                Close();
                break;
            default:
                break;
        }

        OnAfterStateChange?.Invoke(_type, _state);
    }

    public enum State
    {
        OPEN = 0,
        IDLE = 1,
        CLOSE = 2
    }

    public enum Type
    {
        PAUSE = 0,
        MENU = 1
    }
}
