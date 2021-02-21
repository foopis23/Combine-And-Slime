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
        if(!initialized)
        {
            tilemap = GameObject.FindGameObjectWithTag("FloorTiles").GetComponent<Tilemap>();
            TileLocation = tilemap.WorldToCell(transform.position);
            Destination = transform.position;
            OccupiedTiles = new HashSet<Vector3Int>();
            Scale = scale;
            UpdateTiles();

            initialized = true;
        }
    }

    protected override void UpdateObject()
    {
        if(atDestination)
        {
            int movingSlimeWidth = slimeController.CurrentSlime.Scale + 1;
            Vector3 peepeepoopoo = slimeController.CurrentSlime.Destination - slimeController.CurrentSlime.transform.position;
            Vector3Int movingSlimeTile = tilemap.WorldToCell(slimeController.CurrentSlime.transform.position + Vector3.up * (peepeepoopoo.y < 0 ? 0.499f : 0.001f));
            Vector3Int movingSlimeDirection = slimeController.CurrentSlime.TileLocation - movingSlimeTile;
            movingSlimeDirection.Clamp(new Vector3Int(-1, -1, 0), new Vector3Int(1, 1, 0));
            if(movingSlimeDirection.sqrMagnitude > 0)
            {
                HashSet<Vector3Int> movingSlimeOccupiedTiles = new HashSet<Vector3Int>();
                for(int h = 0; h < movingSlimeWidth; h++)
                {
                    for(int k = 0; k < movingSlimeWidth; k++)
                    {
                        movingSlimeOccupiedTiles.Add(movingSlimeTile + new Vector3Int(h, k, 0));
                    }
                }

                Vector3Int[] directions = {
                    new Vector3Int( 1,  0, 0),
                    new Vector3Int(-1,  0, 0),
                    new Vector3Int( 0,  1, 0),
                    new Vector3Int( 0, -1, 0)
                };

                bool done = false;
                foreach(Vector3Int direction in directions)
                {
                    Vector3Int perpendicular = direction.x == 0 ? directions[0] : directions[2];
                    for(int i = 0; i < scale + 1; i++)
                    {
                        Vector3Int testPos = TileLocation + direction * (direction.x > 0 || direction.y > 0 ? scale + 1 : 1) + perpendicular * i;   
                        if(movingSlimeOccupiedTiles.Contains(testPos) && movingSlimeDirection == -direction)
                        {
                            Move(TileLocation + (slimeController.CurrentSlime.TileLocation - movingSlimeTile));
                            done = true;
                            break;
                        }
                    }

                    if(done) break;
                }
            }
        }
    }
}
