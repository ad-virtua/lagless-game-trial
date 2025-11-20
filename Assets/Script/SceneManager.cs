using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;

    public enum SceneType
    {
        Title = 0,
        Stage1 = 1,
        Stage2 = 2,
        Stage3 = 3,
        NULL = -1
    }

    [HideInInspector]
    public SceneType sceneType { get; set; }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        sceneType = SceneType.Stage1;
        StageManager.instance.ChangeStage((int)sceneType);
    }
}
