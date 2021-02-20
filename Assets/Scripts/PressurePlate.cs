using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public enum ButtonType
{
    RED = 0,
    GREEN = 1,
    BLUE = 2,
    PURPLE = 3,
    BLACK = 4

}

public class ActivateButtonContext : EventContext
{
    public ButtonType ButtonType;

    public ActivateButtonContext(ButtonType ButtonType)
    {
        this.ButtonType = ButtonType;
    }
}

public class DeactivateButtonContext : EventContext
{
    public ButtonType ButtonType;

    public DeactivateButtonContext(ButtonType ButtonType)
    {
        this.ButtonType = ButtonType;
    }
}

public class PressurePlate : MonoBehaviour
{
    // editor fields
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private int startSize;
    [SerializeField] private ButtonType buttonType;

    // internal fields
    private bool isTriggered;
    private int size;
    private Vector3Int gridLocation;

    // Start is called before the first frame update
    void Start()
    {
        size = startSize;
        transform.localScale = new Vector3(size + 1, size + 1, 0);
        isTriggered = false;
        Vector3Int pos = groundMap.WorldToCell(transform.position);
        gridLocation = new Vector3Int(pos.x, pos.y, 0);
    }

    void OnSlimeStartMoving(SlimeStartMovingContext ctx)
    {
        if (isTriggered && ctx.Slime.OccupiedTiles.Contains(new Vector3Int(gridLocation.x, gridLocation.y, ctx.Slime.TileLocation.z)))
        {
            isTriggered = false;
            EventSystem.Current.FireEvent(new DeactivateButtonContext(buttonType));
        }
    }

    void OnSlimeFinishedMoving(SlimeFinishMovingContext ctx)
    {
        Debug.Log("PISS BABY");
        
        if (!isTriggered)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!ctx.Slime.OccupiedTiles.Contains(new Vector3Int(gridLocation.x + x, gridLocation.y + y, ctx.Slime.TileLocation.z)))
                    {
                        return;
                    }
                }
            }

            isTriggered = true;
            EventSystem.Current.FireEvent(new ActivateButtonContext(buttonType));
        }
    }
}
