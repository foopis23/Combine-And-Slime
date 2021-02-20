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
        OptionsAnimator = Options.GetComponent<Animator>();
        OptionsAnimator.Play("SlideRightOut");
        LevelSelectAnimator.Play("SlideRightOut");
    }

    public void OnPlayClicked()
    {
        MainMenuAnimator.Play("SlideLeftOut");
        LevelSelectAnimator.Play("SlideLeftIn");
    }

    public void OnOptionsClicked()
    {
        MainMenuAnimator.Play("SlideLeftOut");
        OptionsAnimator.Play("SlideLeftIn");
    }

    public void OnOptionsBackClicked()
    {
        MainMenuAnimator.Play("SlideRightIn");
        OptionsAnimator.Play("SlideRightOut");
    }

    public void OnLevelSelectBackClicked()
    {
        MainMenuAnimator.Play("SlideRightIn");
        LevelSelectAnimator.Play("SlideRightOut");
    }

    public void OnQuitClicked() {
        Application.Quit(0);
    }
}
