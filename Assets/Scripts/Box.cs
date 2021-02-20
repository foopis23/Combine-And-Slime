using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class Box : MovableObject
{
    [SerializeField] private SlimeController slimeController;
    [SerializeField] private int scale;

    public override void Init()
    {
        tilemap = GameObject.FindGameObjectWithTag("FloorTiles").GetComponent<Tilemap>();
        TileLocation = tilemap.WorldToCell(transform.position);
        OccupiedTiles = new HashSet<Vector3Int>();
        Scale = scale;
        UpdateTiles();
    }

    protected override void UpdateObject()
    {
        
    }
}
