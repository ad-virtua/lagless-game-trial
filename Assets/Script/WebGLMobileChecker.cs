using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLMobileChecker : MonoBehaviour
{
    // JavaScriptから呼び出す関数を定義
    [DllImport("__Internal")]
    public static extern bool IsMobile();

    [SerializeField] private GameObject mobileCanvas;

    void Start()
    {
        // WebGLビルドでのみJavaScript関数を呼び出す
#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsMobile())
        {
             mobileCanvas.SetActive(true);
            Debug.Log("WebGL: モバイルデバイスと判定");
        }
        else
        {
            mobileCanvas.SetActive(false);
            Debug.Log("WebGL: PCまたはタブレットと判定");
        }
#else
        // WebGL以外のプラットフォームでの動作
        if (Application.isMobilePlatform)
        {
            mobileCanvas.SetActive(true);
            Debug.Log("Unity: モバイルデバイスと判定");
        }
        else
        {
            mobileCanvas.SetActive(false);
            Debug.Log("Unity: PCと判定");
        }
#endif
    }
}