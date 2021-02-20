using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CallbackEvents;
using UnityEngine.SceneManagement;

public class SwitchSceneContext : EventContext {
    public int SceneIndex;

    public SwitchSceneContext(int SceneIndex) {
        this.SceneIndex = SceneIndex;
    }
}

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject panel;
    private int sceneToLoad;

    void Start()
    {
        EventSystem.Current.RegisterEventListener<SwitchSceneContext>(OnTriggerSceneSwitch);
    }

    void OnTriggerSceneSwitch(SwitchSceneContext ctx) {
        sceneToLoad = ctx.SceneIndex;
        panel.SetActive(true);
        animator.Play("FadeOut");
    }

    // called at the end of the "FadeIn" animation
    void OnFadeIn() {
        GameState.PAUSED = false;
        panel.SetActive(false);
    }

    // called at the end of the "FadeOut" animation
    void OnFadeOut() {
        //! If scenes start taking a long time to load, start loading them async before the fade starts
        SceneManager.LoadScene(sceneToLoad);
    }
}
