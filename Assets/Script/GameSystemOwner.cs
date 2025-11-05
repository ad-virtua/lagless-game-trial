using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystemOwner : MonoBehaviour
{
    [SerializeField]
    private GameObject gameOverUI, clearUI;

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
            if (!clearUI.activeSelf) clearUI.SetActive(true);
        }
    }
}
