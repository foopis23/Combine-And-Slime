using UnityEngine;
using UnityEngine.SceneManagement;
using CallbackEvents;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    //internal properties
    int sceneIndex;

    void Start()
    {
        pausePanel.SetActive(false);
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        GameState.PAUSED = !GameState.PAUSED;
        pausePanel.SetActive(GameState.PAUSED);
    }

    public void OnRetryClicked()
    {
        EventSystem.Current.FireEvent(new SwitchSceneContext(sceneIndex));
    }

    public void OnMenuClicked()
    {
        EventSystem.Current.FireEvent(new SwitchSceneContext(0));
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
