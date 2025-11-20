using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    public int stage1EndAreaCount, stage2EndAreaCount, stage3EndAreaCount;

    public GameObject map;

    public GameObject[] stage1Parts, stage2Parts, stage3Parts;

    [HideInInspector]
    public int stageAreaCount;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (GameSystemOwner.isClear)
        {
            switch (SceneManager.instance.sceneType)
            {
                case SceneManager.SceneType.Stage1:
                    if (stageAreaCount != stage1EndAreaCount) stageAreaCount = stage1EndAreaCount;
                    break;
                case SceneManager.SceneType.Stage2:
                    if (stageAreaCount != stage2EndAreaCount) stageAreaCount = stage1EndAreaCount;
                    break;
                case SceneManager.SceneType.Stage3:
                    if (stageAreaCount != stage3EndAreaCount) stageAreaCount = stage1EndAreaCount;
                    break;
            }
        }
    }

    public bool SceneEndAreaChecker(SceneManager.SceneType targetScene = SceneManager.SceneType.NULL)
    {
        if (targetScene == SceneManager.SceneType.NULL)
        {
            targetScene = SceneManager.instance.sceneType;
        }

        switch (targetScene)
        {
            case SceneManager.SceneType.Stage1:
                if (stageAreaCount == stage1EndAreaCount) return true;
                break;
            case SceneManager.SceneType.Stage2:
                if (stageAreaCount == stage2EndAreaCount) return true;
                break;
            case SceneManager.SceneType.Stage3:
                if (stageAreaCount == stage3EndAreaCount) return true;
                break;
        }
        return false;
    }

    public void ChangeStage(int nextStage)
    {
        map.SetActive(false);
        Player.instance.ResetPosition();
        GameSystemOwner.isClear = GameSystemOwner.isGameOver = false;
        StageMoveSystem.instance.ResetMove();

        switch ((SceneManager.SceneType)nextStage)
        {
            case SceneManager.SceneType.Stage1:
                foreach (var item in stage1Parts)
                {
                    item.SetActive(true);
                }
                foreach (var item in stage2Parts)
                {
                    item.SetActive(false);
                }
                stageAreaCount = 1;
                break;
            case SceneManager.SceneType.Stage2:
                foreach (var item in stage1Parts)
                {
                    item.SetActive(false);
                }
                foreach (var item in stage2Parts)
                {
                    item.SetActive(true);
                }
                stageAreaCount = 1;
                break;
            case SceneManager.SceneType.Stage3:
                break;
        }
    }

    public void ActiveMap()
    {
        map.SetActive(true);
    }
}
