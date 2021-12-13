using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Lean.Pool;

[Serializable]
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public Transform boardParent;
    public Transform topLeftPoint;
    public Transform bottomLeftPoint;
    public Transform topRightPoint;
    public Tile tilePrefab;
    public List<Tile> tilesOnBoard;
    public Tile[,] tilesOnBoards;
    public TileData[] tileDatas;
    [SerializeField] private int width, height;

    //card properties on board 
    public List<Tile> droppedTile = new List<Tile>();
    public Card mostLeftCard = null;
    public Card mostRightCard = null;

    public event Action OnCardDropped;
    public event Action OnCardMatch;

    
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

        GenerateBoard(topLeftPoint.position, topRightPoint.position, bottomLeftPoint.position);
    }

    private void GenerateBoard(Vector3 _pointA, Vector3 _pointB, Vector3 _pointC)
    {
        tilesOnBoard = new List<Tile>();
        tilesOnBoards = new Tile[width, height];
        tileDatas = new TileData[width];
        

        //GameObject container = new GameObject();
        //GameObject boardContainer = LeanPool.Spawn(new GameObject(), boardParent);
        //boardContainer.name = "Board Container";
        //boardContainer.transform.position = _pointC;
        //float spriteWidth = Vector3.Distance(_pointA, _pointB) / width;
        //float spriteHeight = Vector3.Distance(_pointA, _pointC) / height;

        for (int x = 0; x < tileDatas.Length; x++)
        {
            tileDatas[x].tiles = new Tile[height];

            for (int y = 0; y < tileDatas[x].tiles.Length; y++)
            {
                Tile tile = LeanPool.Spawn(tilePrefab, boardParent);
                string tileId = x + "|" + y;
                tile.InitTile(tileId);
                //tile.transform.localPosition = new Vector3(x * spriteWidth, y * spriteHeight, -1);
                //tile.transform.localScale = new Vector3(spriteWidth, spriteHeight, 1);

                tilesOnBoard.Add(tile);
                tilesOnBoards[x, y] = tile;
                tileDatas[x].tiles[y] = tile;
                //Debug.Log($"Tiles on board {x} {y} :: {tilesOnBoards[x, y]._tileIndex}");
            }
        }
    }

    public void RecordBoard()
    {

    }

    public void CheckBoard()
    {
        if (droppedTile.Count <= 1)
        {

        }

        else
        {

        }
    }


    private void OnDrawGizmos()
    {
        if (tilesOnBoard.Count > 0)
        {
            for (int i = 0; i < tilesOnBoard.Count; i++)
            {
                Tile tile = tilesOnBoard[i];
                Gizmos.DrawIcon(tile.transform.GetChild(0).position, "Light Gizmo.tiff", true);
            }
        }
    }

    [Serializable]
    public struct TileData
    {
        public Tile[] tiles;
    }

}
