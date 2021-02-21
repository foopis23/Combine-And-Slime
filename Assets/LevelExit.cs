using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class LevelExit : MonoBehaviour
{
    // editor fields
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private int startSize;

    // internal fields
    private int size;
    private Vector3Int gridLocation;

    // Start is called before the first frame update
    void Start()
    {
        size = startSize;
        transform.localScale = new Vector3(size + 1, size + 1, 0);

        Vector3Int pos = groundMap.WorldToCell(transform.position);
        gridLocation = new Vector3Int(pos.x, pos.y, 0);
        transform.position = groundMap.CellToWorld(pos);

        EventSystem.Current.RegisterEventListener<SlimeFinishMovingContext>(OnSlimeFinishedMoving);
    }

    private bool doesSlimeIsCoverExit(Slime slime)
    {
        for (int y = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++)
            {
                if (!slime.OccupiedTiles.Contains(new Vector3Int(gridLocation.x + x, gridLocation.y + y, slime.TileLocation.z)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    void OnSlimeFinishedMoving(SlimeFinishMovingContext ctx)
    {   
        if (doesSlimeIsCoverExit(ctx.Slime))
        {
            EventSystem.Current.FireEvent(new LevelBeatContext(SlimeController.moveCount, SlimeController.mergeCount, SlimeController.splitCount));
        }
    }
}
