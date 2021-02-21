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

        EventSystem.Current.RegisterEventListener<ObjectFinishMovingContext>(OnObjectFinishedMoving);
    }

    private bool doesSlimeIsCoverExit(Slime slime)
    {
        return slime.TileLocation == gridLocation && slime.Scale == size;
    }

    void OnObjectFinishedMoving(ObjectFinishMovingContext ctx)
    {   
        if (ctx.obj is Slime && doesSlimeIsCoverExit((Slime) ctx.obj))
        {
            EventSystem.Current.FireEvent(new LevelBeatContext(SlimeController.moveCount, SlimeController.mergeCount, SlimeController.splitCount));
        }
    }
}
