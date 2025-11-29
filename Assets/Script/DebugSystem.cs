using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSystem : MonoBehaviour
{
    void Update()
    {
        // 1キーでシーンリセット
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetScene(1);
        }

        // 2キーでシーンリセット
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResetScene(2);
        }
    }

    public void ResetScene(int stageNum)
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        ScenesManagers.sceneType = (ScenesManagers.SceneType)stageNum;
    }
}
