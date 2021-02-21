using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CallbackEvents;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource slimeMove;
    [SerializeField] private AudioSource slimeArrive;
    [SerializeField] private AudioSource slimeSelect;
    [SerializeField] private AudioSource slimeSelectCancel;


    // Start is called before the first frame update
    void Start()
    {
        EventSystem.Current.RegisterEventListener<ObjectStartMovingContext>(OnObjectStartMoving);
        EventSystem.Current.RegisterEventListener<ObjectFinishMovingContext>(OnObjectFinishMoving);
        EventSystem.Current.RegisterEventListener<SlimeSelectedContext>(OnSlimeSelected);
        EventSystem.Current.RegisterEventListener<SlimeSelectCancelledContext>(OnSlimeDeselect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnObjectStartMoving(ObjectStartMovingContext ctx) {
        if (ctx.obj is Slime) {
            slimeMove.Play();
        }
    }

    void OnObjectFinishMoving(ObjectFinishMovingContext ctx) {
        if (ctx.obj is Slime) {
            slimeArrive.Play();
        }
    }

    void OnSlimeSelected(SlimeSelectedContext ctx) {
        slimeSelect.Play();
    }

    void OnSlimeDeselect(SlimeSelectCancelledContext ctx) {
        slimeSelectCancel.Play();
    }
}
