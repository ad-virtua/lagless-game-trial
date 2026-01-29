using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemOwner : MonoBehaviour
{
    public static GameSystemOwner instance;

    [SerializeField]
    private GameObject gameOverUI, clearUI;
    [SerializeField]
    private GameObject[] movieScenes;

    public static bool isClear, isGameOver;

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        CheckPlayerActive();
        CheckClear();
    }

    void CheckPlayerActive()
    {
        if (isGameOver &&
            GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>().enabled)
        {
            isGameOver = false;
            gameOverUI.SetActive(false);
        }

        if (isGameOver && !gameOverUI.activeSelf)
        {
            gameOverUI.SetActive(true);

        }
    }

    void CheckClear()
    {
        if (isClear)
        {
            StageProgressSave.SaveClearedStage(ScenesManagers.sceneType);
            Debug.Log(ScenesManagers.sceneType);

            if (!movieScenes[(int)ScenesManagers.SceneType.Stage1 - 1].activeSelf &&
                ScenesManagers.sceneType != ScenesManagers.SceneType.Stage2)
            {
                movieScenes[(int)ScenesManagers.SceneType.Stage1 - 1].SetActive(true);
            }
        }
        else
        {
            foreach (var movieScene in movieScenes)
            {
                movieScene.SetActive(false);
            }
        }
    }

    public bool IsPlayMovie()
    {
        return GameObject.FindGameObjectsWithTag("Movie").Length != 0;
    }
}
