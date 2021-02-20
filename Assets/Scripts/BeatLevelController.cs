﻿using UnityEngine;
using CallbackEvents;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelBeatContext : EventContext
{
    public int moves;
    public int merges;
    public int splits;

    public LevelBeatContext(int moves, int merges, int splits)
    {
        this.moves = moves;
        this.merges = merges;
        this.splits = splits;
    }
}

public class BeatLevelController : MonoBehaviour
{
    // editor properties
    [SerializeField] private TMP_Text levelTitle;
    [SerializeField] private TMP_Text movesCount;
    [SerializeField] private TMP_Text mergeCount;
    [SerializeField] private TMP_Text splitCount;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject levelCompletelPanel;

    //internal properties
    int sceneIndex;

    // Start is called before the first frame update
    void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        levelTitle.text = $"level {sceneIndex}";
        if (sceneIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            nextButton.gameObject.SetActive(false);
        }

        EventSystem.Current.RegisterEventListener<LevelBeatContext>(OnLevelBeat);
    }

    private string DisplayScore(int score, int digits)
    {
        int digitCount = 0;
        int scoreTemp = score;

        if (score == 0) return new string('0', digits);

        while (scoreTemp > 0)
        {
            scoreTemp /= 10;
            digitCount++;
        }

        return new string('0', Mathf.Max(digits - digitCount, 0)) + $"{score}";
    }

    private int CalcScore(int moves, int merges, int splits)
    {
        //TODO: MAKE SCORE CALC FORMULA


        return moves + merges + splits;
    }

    private void SaveLevelStats(int moves, int merges, int splits, int score)
    {
        PlayerPrefs.SetInt($"level{sceneIndex}.moves", moves);
        PlayerPrefs.SetInt($"level{sceneIndex}.merges", merges);
        PlayerPrefs.SetInt($"level{sceneIndex}.splits", splits);
        PlayerPrefs.SetInt($"level{sceneIndex}.score", score);
        PlayerPrefs.Save();
    }

    public void OnLevelBeat(LevelBeatContext ctx)
    {
        // calc score and save level stats
        int score = CalcScore(ctx.moves, ctx.merges, ctx.splits);
        SaveLevelStats(ctx.moves, ctx.merges, ctx.splits, score);

        // Set level complete ui to correct values
        movesCount.text = DisplayScore(ctx.moves, 3);
        mergeCount.text = DisplayScore(ctx.merges, 3);
        splitCount.text = DisplayScore(ctx.splits, 3);
        scoreText.text = DisplayScore(score, 6);

        // Display Level Complete Panel
        levelCompletelPanel.SetActive(true);
    }

    public void OnMenu()
    {
        EventSystem.Current.FireEvent(new SwitchSceneContext(0));
    }

    public void OnNextLevel()
    {
        EventSystem.Current.FireEvent(new SwitchSceneContext(sceneIndex + 1));
    }

    public void OnRetryLevel()
    {
        EventSystem.Current.FireEvent(new SwitchSceneContext(sceneIndex));
    }
}
