using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemOwner : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverUI, clearUI;
    [SerializeField]
    private GameObject[] movieScenes;

    public static bool isClear, isGameOver;

    private void LateUpdate()
    {
        CheckPlayerActive();
        CheckClear();
    }

    void CheckPlayerActive()
    {
        if (!isGameOver && !GameObject.FindGameObjectWithTag("Player"))
        {
            isGameOver = true;
            gameOverUI.SetActive(true);
        }
    }

    void CheckClear()
    {
        if (isClear)
        {
            if (!movieScenes[(int)ScenesManagers.SceneType.Stage1 - 1].activeSelf)
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
}
