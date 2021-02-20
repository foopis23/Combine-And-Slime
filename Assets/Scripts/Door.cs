using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CallbackEvents;

public class Door : MonoBehaviour
{
    //public properties
    public bool IsActive {
        get {
            return activated > 0;
        }
    }

    //editor properties
    [SerializeField] private TileBase buttonType; //the button to respond to

    //internal properties
    private int activated = 0;

    private void Start() {
        EventSystem.Current.RegisterEventListener<ActivateButtonContext>(OnActivateButton);
        EventSystem.Current.RegisterEventListener<DeactivateButtonContext>(OnDeactiveButton);    
    }

    public void OnActivateButton(ActivateButtonContext ctx) {
        if (ctx.ButtonType == null) return;

        if (ctx.ButtonType.Equals(buttonType)) {
            ++activated;

            Debug.Log("THIS IS A TEST");

            if (activated == 1) {
                //open door trans
            }
        }
    }

    public void OnDeactiveButton(DeactivateButtonContext ctx) {
        if (ctx.ButtonType == null) return;

        if (ctx.ButtonType.Equals(buttonType)) {
            --activated;

            Debug.Log("THIS IS A TEST FOR BEING STUPID");

            if (activated == 0) {
                //close door trans
            }

            activated = Mathf.Max(activated, 0);
        }
    }
}
