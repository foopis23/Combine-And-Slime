using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CallbackEvents;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeatLevelUIController : MonoBehaviour
{
    // editor properties
    [SerializeField] private TMP_Text levelTitle;
    [SerializeField] private TMP_Text movesCount;
    [SerializeField] private TMP_Text mergeCount;
    [SerializeField] private TMP_Text splitCount;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button nextButton;

    //internal properties
    int sceneIndex;

    // Start is called before the first frame update
    void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        levelTitle.text = $"level {sceneIndex}";
        if (sceneIndex + 1 >= SceneManager.sceneCountInBuildSettings) {
            nextButton.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMenu() {
        EventSystem.Current.FireEvent(new SwitchSceneContext(0));
    }

    public void OnNextLevel() {
        //TODO: ADD CHECK TO MAKE SURE MORE LEVELS EXIST
        EventSystem.Current.FireEvent(new SwitchSceneContext(sceneIndex + 1));
    }

    public void OnRetryLevel() {
        EventSystem.Current.FireEvent(new SwitchSceneContext(sceneIndex));
    }
}
