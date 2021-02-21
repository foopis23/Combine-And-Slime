using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicMakeMeLoseControl : MonoBehaviour
{
    [SerializeField] private AudioSource intro;
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioSource puzzleMusic;
    [SerializeField] private Animator animator;

    private bool inMenu;
    private bool finishedIntro;

    private void Awake() {
        if (GameObject.FindGameObjectsWithTag("music").Length > 1) {
            Destroy(this.gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        inMenu = true;
        SceneManager.activeSceneChanged += ActiveSceneChanged;
        puzzleMusic.volume = 0.0f;
        intro.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!intro.isPlaying && !finishedIntro)
        {
            finishedIntro = true;
            menuMusic.Play();
            puzzleMusic.Play();
        }
    }

    void ActiveSceneChanged(Scene current, Scene next)
    {
        inMenu = next.buildIndex == 0;

        if (finishedIntro)
        {
            if (inMenu)
            {
                animator.Play("fadeToMenu");
            }
            else
            {
                animator.Play("fadeToPuzzle");
            }
        }
    }
}
