using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelParent : MonoBehaviour
{
    public PanelParentType parentType;
    public List<string> panelNames;
    public Dictionary<string, Panel> panelTable = new Dictionary<string, Panel>();

    private void Start()
    {
        RegisterPanel();
    }

    private void RegisterPanel()
    {
        panelTable = new Dictionary<string, Panel>();

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<Panel>() != null)
                {
                    Panel panel = transform.GetChild(i).GetComponent<Panel>();
                    panel.gameObject.name = panelNames[i];
                    panelTable.Add(panelNames[i], panel);
                }

                else
                {
                    continue;
                }
            }
        }
    }

    public void OpenPanel(string name)
    {
        if (panelTable.ContainsKey(name))
        {
            panelTable[name].gameObject.SetActive(true);
        }
    }

    public void ClosePanel(string name)
    {
        if (panelTable.ContainsKey(name))
        {
            panelTable[name].gameObject.SetActive(false);
        }
    }

    public void OpenClosePanel(string panelOpen, string panelClose)
    {
        if (panelTable.ContainsKey(panelOpen))
        {
            panelTable[panelOpen].gameObject.SetActive(true);
        }

        if (panelTable.ContainsKey(panelClose))
        {
            panelTable[panelClose].gameObject.SetActive(false);
        }
    }

    public void OpenOnlyPanel(string name)
    {

    }

    public enum PanelParentType
    {
        MainMenu = 0,
        LevelSelection = 1,
        Gameplay = 2
    }
}
