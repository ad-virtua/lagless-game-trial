using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagers : MonoBehaviour
{
    public static ScenesManagers instance;
    public bool isTitleScene;

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

        if (sceneType != SceneType.Title) StageManager.instance.ChangeStage((int)sceneType);
    }

    public void ResetProgress()
    {
        StageProgressSave.ResetProgress();
    }

    public void OnClickGame()
    {
        SceneManager.LoadScene("Game");
    }
}
