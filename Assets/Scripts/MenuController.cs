using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject MainMenuUI;
    private Animator MainMenuAnimator;
    [SerializeField] private GameObject LevelSelect;
    [SerializeField] private GameObject LevelSelectUI;
    private Animator LevelSelectAnimator;
    [SerializeField] private GameObject Options;
    private Animator OptionsAnimator;

    // Start is called before the first frame update
    void Start()
    {
        MainMenuAnimator = MainMenu.GetComponent<Animator>();
        LevelSelectAnimator = LevelSelect.GetComponent<Animator>();
        // OptionsAnimator = LevelSelect.GetComponent<Animator>();
    }

    public void OnPlayClicked()
    {
        MainMenuAnimator.Play("SlideLeftOut");
        LevelSelectAnimator.Play("SlideLeftIn");
        LevelSelectUI.SetActive(true);
        //MainMenuUI.SetActive(false);
    }

    void OnOptionsClicked()
    {

    }

    void OnOptionsBackClicked()
    {

    }

    void OnLevelSelectBackClicked()
    {

    }
}
