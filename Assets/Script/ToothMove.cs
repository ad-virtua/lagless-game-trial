using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToothMove : MonoBehaviour
{
    [Tooltip("移動の終点となる座標（インスペクタで指定）")]
    public Vector3 endPoint;

    [Tooltip("移動の速さ。値が1の場合、片道に1秒かかります。")]
    public float speed = 1.0f;

    // スクリプト開始時の位置（＝開始地点）を保存する変数
    private Vector3 startPoint;

    void Start()
    {
        // 現在の座標を開始地点(startPoint)として記憶します
        startPoint = transform.localPosition;
    }

    void Update()
    {
        // 1. 0～1の間を往復する値を計算します
        // Time.time * speed の値が 0 -> 1 になるまで t は 0 -> 1 (片道)
        // Time.time * speed の値が 1 -> 2 になるまで t は 1 -> 0 (復路)
        float t = Mathf.PingPong(Time.time * speed, 1.0f);

        // 2. Vector3.Lerp を使って、startPoint と endPoint の間を t の値に応じて補間します
        // t=0 のとき startPoint に、t=1 のとき endPoint になります
        transform.localPosition = Vector3.Lerp(startPoint, endPoint, t);
    }
}
