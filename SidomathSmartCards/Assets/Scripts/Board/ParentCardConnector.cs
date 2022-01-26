using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParentCardConnector : MonoBehaviour
{
    public List<CardConnector> topCardConnectors = new List<CardConnector>();
    public List<CardConnector> bottomCardConnectors = new List<CardConnector>();


    public List<CardConnector> GetTopActiveConnectors()
    {
        IEnumerable<CardConnector> topConn = topCardConnectors.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeTopConn = new List<CardConnector>(topConn.ToList());

        return activeTopConn;
    }

    public List<CardConnector> GetBottomActiveConnectors()
    {
        IEnumerable<CardConnector> bottomConn = bottomCardConnectors.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeBotConn = new List<CardConnector>(bottomConn.ToList());

        return activeBotConn;
    }

    public CardConnector GetRandomFromActiveTopConnectors()
    {
        IEnumerable<CardConnector> topConn = topCardConnectors.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeTopConn = new List<CardConnector>(topConn.ToList());

        if (activeTopConn.Count > 0)
        {
            int randomPick = Random.RandomRange(0, activeTopConn.Count - 1);
            CardConnector picked = activeTopConn[randomPick];

            return picked;
        }

        else
        {
            return null;
        }
    }

    public CardConnector GetRandomFromActiveBotConnectors()
    {
        IEnumerable<CardConnector> botConn = bottomCardConnectors.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeBotConn = new List<CardConnector>(botConn.ToList());

        if (activeBotConn.Count > 0)
        {
            int randomPick = Random.RandomRange(0, activeBotConn.Count - 1);
            CardConnector picked = activeBotConn[randomPick];

            return picked;
        }

        else
        {
            return null;
        }
        
    }
}
