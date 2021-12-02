using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Tile : MonoBehaviour
{
    [Header("Tile Properties")]
    [SerializeField] private int tileIndex;
    [SerializeField] private bool droppable = true;
    [SerializeField] private bool placed = false;

    [Header("Tile Display Component")]
    public Sprite tileSprite;
    [ReorderableList] public List<Transform> tilePoints;

    [Header("Dropped Card")]
    public Card droppedCard;

    public int _tileIndex { get => tileIndex; set => tileIndex = value; }
    public bool _droppable { get => droppable; set => droppable = value; }
    public bool _placed { get => placed; set => placed = value; }

    public void InitTile(int id)
    {
        _tileIndex = id;
        _droppable = true;
        _placed = false;
    }
}
