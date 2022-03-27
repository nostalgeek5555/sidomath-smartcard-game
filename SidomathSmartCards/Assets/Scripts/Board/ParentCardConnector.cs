using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParentCardConnector : MonoBehaviour
{
    public List<CardConnector> allCardConnector = new List<CardConnector>();
    public List<CardConnector> topCardConnectors = new List<CardConnector>();
    public List<CardConnector> bottomCardConnectors = new List<CardConnector>();

    public List<CardConnector> GetActiveConnectors()
    {
        IEnumerable<CardConnector> connectors = allCardConnector.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeConnectors = new List<CardConnector>(connectors.ToList());

        if (activeConnectors.Count > 0)
        {
            return activeConnectors;
        }

        else
        {
            return null;
        }
    }

    public List<CardConnector> GetShuffledActiveConnectors()
    {
        int shuffledCount = 0;
        IEnumerable<CardConnector> connectors = allCardConnector.Where(connector => connector.gameObject.active == true);
        List<CardConnector> activeConnectors = new List<CardConnector>(connectors.ToList());

        for (int i = 0; i < activeConnectors.Count; i++)
        {
            CardConnector tempConnector = activeConnectors[i];
            int randomIndex = Random.Range(i, activeConnectors.Count);
            activeConnectors[i] = activeConnectors[randomIndex];
            activeConnectors[randomIndex] = tempConnector;

            shuffledCount++;

            if (shuffledCount == activeConnectors.Count)
            {
                Debug.Log($"Shuffle connectors done {shuffledCount}");
                break;
            }
        }

        return activeConnectors;
    }


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

    public CardConnector GetRandomBotConnectorWithoutType(CardConnector.CardConnectorType connectorType)
    {
        IEnumerable<CardConnector> botConn = bottomCardConnectors.Where(connector => connector.gameObject.active == true).Where(connector => connector.cardConnectorType != connectorType);
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

    public CardConnector GetRandomTopConnectorWithoutType(CardConnector.CardConnectorType connectorType)
    {
        IEnumerable<CardConnector> topConn = topCardConnectors.Where(connector => connector.gameObject.active == true).Where(connector => connector.cardConnectorType != connectorType);
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
}
