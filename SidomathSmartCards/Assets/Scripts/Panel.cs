using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public Panel panel;
    public void OpenPanel()
    {
        panel.gameObject.SetActive(true);
    }


    public void ClosePanel()
    {
        panel.gameObject.SetActive(false);
    }
}
