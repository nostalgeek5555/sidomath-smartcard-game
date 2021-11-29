using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button settingButton;
    public Button exitButton;

    private void OnEnable()
    {
        //start button
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            OnStartButton();
        });

        //setting button
        //settingButton.onClick.RemoveAllListeners();
        //settingButton.onClick.AddListener(() =>
        //{
        //    OnSettingButton();
        //});

        //exit button
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() =>
        {
            OnExitButton();
        });
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        //settingButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
    }

    public void OnStartButton()
    {
        SceneManager.LoadScene(1);
    }

    public void OnSettingButton()
    {

    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}
