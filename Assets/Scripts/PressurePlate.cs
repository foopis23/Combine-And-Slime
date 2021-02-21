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
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // internal fields
    private GameObject ObjectPressingButton;
    private bool isTriggered;
    private int size;
    private Vector3Int gridLocation;

    // Start is called before the first frame update
    void Start()
    {
        if (groundMap == null)
            groundMap = GameObject.FindGameObjectWithTag("FloorTiles").GetComponent<Tilemap>();
        
        size = startSize;
        spriteRenderer.sprite = sprites[size];
        isTriggered = false;
        Vector3Int pos = groundMap.WorldToCell(transform.position);
        gridLocation = new Vector3Int(pos.x, pos.y, 0);
        transform.position = groundMap.CellToWorld(pos);
        EventSystem.Current.RegisterEventListener<ObjectFinishMovingContext>(OnObjectFinishedMoving);
        EventSystem.Current.RegisterEventListener<SlimeSplitContext>(OnSlimeSplit);
        EventSystem.Current.RegisterEventListener<SlimeMergeContext>(OnSlimeMerge);
    }


    private bool doesThingIsCoverButton(MovableObject thing)
    {
        for (int y = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++)
            {
                if (!thing.OccupiedTiles.Contains(new Vector3Int(gridLocation.x + x, gridLocation.y + y, thing.TileLocation.z)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void activateButton(GameObject buttonPressor) {
        ObjectPressingButton = buttonPressor;
        isTriggered = true;
        EventSystem.Current.FireEvent(new ActivateButtonContext(buttonType));
    }

    private void deactivateButton() {
        ObjectPressingButton = null;
        isTriggered = false;
        EventSystem.Current.FireEvent(new DeactivateButtonContext(buttonType));
    }

    void OnObjectFinishedMoving(ObjectFinishMovingContext ctx)
    {
        bool buttonPressed = doesThingIsCoverButton(ctx.obj);
        
        if (!isTriggered && buttonPressed)
        {
            activateButton(ctx.obj.gameObject);
        }else if (isTriggered && ObjectPressingButton.Equals(ctx.obj.gameObject) && !buttonPressed) {
            deactivateButton();
        }
    }

    void OnSlimeSplit(SlimeSplitContext ctx) {
        if (ObjectPressingButton != null && (ObjectPressingButton.Equals(ctx.NewSlime.gameObject) || ObjectPressingButton.Equals(ctx.OldSlime.gameObject))) {
            deactivateButton();
        }
    }

    void OnSlimeMerge(SlimeMergeContext ctx) {
        if (ObjectPressingButton != null && (ObjectPressingButton.Equals(ctx.Assimilated.gameObject) || ObjectPressingButton.Equals(ctx.Slime.gameObject))) {
            deactivateButton();
        }
    }
}
