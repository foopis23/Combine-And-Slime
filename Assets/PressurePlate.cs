using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class ActivateButtonContext : EventContext {
    public TileBase ButtonType;

    public ActivateButtonContext(TileBase ButtonType) {
        this.ButtonType = ButtonType;
    }
}

public class DeactivateButtonContext : EventContext {
    public TileBase ButtonType;

    public DeactivateButtonContext(TileBase ButtonType) {
        this.ButtonType = ButtonType;
    }
}

public class PressurePlate : MonoBehaviour
{
    // editor fields
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private int startSize;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSlimeStartMoving(SlimeStartMovingContext ctx) {
        if (isTriggered) {
            // if slime was on top of gridLocation
                //turn button off
        }
    }

    void OnSlimeFinishedMoving(SlimeFinishMovingContext ctx) {
        if (!isActiveAndEnabled) 
        {
            // if slime is on top of gridLocation
                // foreach cover pos
                    // if slime is not covering return
                // turn button on
        }
    }
}
