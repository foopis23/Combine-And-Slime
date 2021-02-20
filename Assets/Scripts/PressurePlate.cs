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
    private GameObject ObjectPressingButton;
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
        transform.position = groundMap.CellToWorld(pos);

        EventSystem.Current.RegisterEventListener<SlimeStartMovingContext>(OnSlimeStartMoving);
        EventSystem.Current.RegisterEventListener<SlimeFinishMovingContext>(OnSlimeFinishedMoving);
        EventSystem.Current.RegisterEventListener<SlimeSplitContext>(OnSlimeSplit);
        EventSystem.Current.RegisterEventListener<SlimeMergeContext>(OnSlimeMerge);
    }

    private bool doesSlimeIsCoverButton(Slime slime)
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

    void OnSlimeStartMoving(SlimeStartMovingContext ctx)
    {

    }

    void OnSlimeFinishedMoving(SlimeFinishMovingContext ctx)
    {
        bool buttonPressed = doesSlimeIsCoverButton(ctx.Slime);
        
        if (!isTriggered && buttonPressed)
        {
            ObjectPressingButton = ctx.Slime.gameObject;
            isTriggered = true;
            EventSystem.Current.FireEvent(new ActivateButtonContext(buttonType));
        }else if (isTriggered && ObjectPressingButton.Equals(ctx.Slime.gameObject) && !buttonPressed) {
            isTriggered = false;
            EventSystem.Current.FireEvent(new DeactivateButtonContext(buttonType));
        }
    }

    void OnSlimeSplit(SlimeSplitContext ctx) {
        if (ObjectPressingButton != null && (ObjectPressingButton.Equals(ctx.NewSlime.gameObject) || ObjectPressingButton.Equals(ctx.OldSlime.gameObject))) {
            ObjectPressingButton = null;
            isTriggered = false;
            EventSystem.Current.FireEvent(new DeactivateButtonContext(buttonType));
        }
    }

    void OnSlimeMerge(SlimeMergeContext ctx) {
        if (ObjectPressingButton != null && (ObjectPressingButton.Equals(ctx.Assimilated.gameObject) || ObjectPressingButton.Equals(ctx.Slime.gameObject))) {
            ObjectPressingButton = null;
            isTriggered = false;
            EventSystem.Current.FireEvent(new DeactivateButtonContext(buttonType));
        }
    }
}
