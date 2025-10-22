using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;

    public enum SceneType
    {
        Title,
        Tutorial,
        Stage1,
        Stage2,
        Stage3
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
        sceneType = SceneType.Tutorial;
    }
}
