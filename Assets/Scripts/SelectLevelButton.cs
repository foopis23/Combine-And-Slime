using UnityEngine;
using CallbackEvents;

public class SelectLevelButton : MonoBehaviour
{
    public int levelNumber = 1; //this should equal to the scene number

    public void OnClick() {
        EventSystem.Current.FireEvent(new SwitchSceneContext(levelNumber));
    }
}
