using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonExtender : Button
{
    public Type type;
    public UIManager.MethodName buttonMethodName;
    public TextMeshProUGUI buttonNameText;

    public void Init(UIManager.MethodName methodName)
    {
        buttonMethodName = methodName;
        buttonNameText.text = buttonMethodName.ToString();
        
        //init button in popup and add listener to method from UIManager which matched with current button method name
        if (UIManager.Instance.NoCallbackMethods.Count > 0)
        {
            if (UIManager.Instance.NoCallbackMethods.ContainsKey(buttonMethodName))
            {
                onClick.RemoveAllListeners();
                onClick.AddListener(() =>
                {
                    Action action = UIManager.Instance.NoCallbackMethods[buttonMethodName];
                    action.Invoke();

                    Debug.Log($"Invoke button action {buttonMethodName}");
                });
            }
        }
    }

    public enum Type
    {
        NO_POPUP = 0,
        WITH_POPUP = 1
    }
}
