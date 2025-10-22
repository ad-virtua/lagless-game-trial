using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    public int tutorialEndAreaCount, stage1EndAreaCount, stage2EndAreaCount, stage3EndAreaCount;

    [HideInInspector]
    public int stageAreaCount;

    private void Awake()
    {
        instance = this;
    }

    public bool SceneEndAreaChecker(SceneManager.SceneType targetScene)
    {
        switch (targetScene)
        {
            case SceneManager.SceneType.Tutorial:
                if (stageAreaCount == tutorialEndAreaCount) return true;
                break;
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
}
