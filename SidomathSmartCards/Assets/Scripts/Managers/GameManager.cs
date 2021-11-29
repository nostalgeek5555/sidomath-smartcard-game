using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            if (Instance != null && Instance != null)
            {
                Destroy(gameObject);
            }
        }

        DontDestroyOnLoad(gameObject);
    }
}
