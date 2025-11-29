using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesManagers : MonoBehaviour
{
    public static ScenesManagers instance;

    [SerializeField]
    private SceneType startSceneType;

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
        if (sceneType == SceneType.Title) sceneType = startSceneType;
    }

    // Start is called before the first frame update
    void Start()
    {
        StageManager.instance.ChangeStage((int)sceneType);
    }
}
