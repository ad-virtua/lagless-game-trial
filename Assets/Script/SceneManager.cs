using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagers : MonoBehaviour
{
    public static ScenesManagers instance;
    public bool isTitleScene;
    public bool isPause;
    public static bool IsPaused => instance != null && instance.isPause;
    private static bool showStageSelectOnGameStart;

    public static IEnumerator WaitWhilePaused()
    {
        while (IsPaused)
        {
            yield return null;
        }
    }

    public static IEnumerator WaitForSecondsPause(float seconds)
    {
        if (seconds <= 0f)
        {
            yield return WaitWhilePaused();
            yield return null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (IsPaused)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return WaitWhilePaused();
    }

    [SerializeField]
    private SceneType startSceneType;

    [SerializeField]
    private GameObject gameSelectButtonSaveDataNone, gameSelectButtonSaveDataActive;

    public enum SceneType
    {
        Title = 0,
        Stage1 = 1,
        Stage2 = 2,
        Stage3 = 3,
        NULL = -1
    }

    public static SceneType sceneType { get; set; }

    private void Awake()
    {
        instance = this;
        sceneType = StageProgressSave.GetStartStage(startSceneType);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isTitleScene)
        {
            if (sceneType == SceneType.Title) gameSelectButtonSaveDataNone.SetActive(true);
            else gameSelectButtonSaveDataActive.SetActive(true);
            return;
        }

        if (showStageSelectOnGameStart)
        {
            showStageSelectOnGameStart = false;
            if (StageManager.instance != null && StageManager.instance.map != null)
            {
                StageManager.instance.map.SetActive(true);
                SetPause(true);
                return;
            }
        }

        if (sceneType != SceneType.Title) StageManager.instance.ChangeStage((int)sceneType);
    }

    public void ResetProgress()
    {
        StageProgressSave.ResetProgress();
    }

    public void OnClickGame()
    {
        showStageSelectOnGameStart = StageProgressSave.TryGetSavedStage(out _);
        SceneManager.LoadScene("Game");
    }

    public void OnClickTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void SetPause(bool isEnable)
    {
        isPause = isEnable;
    }

    public IEnumerator BackTitle(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Title");
    }
}
