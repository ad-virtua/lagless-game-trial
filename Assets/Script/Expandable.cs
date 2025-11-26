using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expandable : MonoBehaviour
{
    [Header("伸縮設定")]
    public Vector3 minScale = new Vector3(1f, 1f, 1f);
    public Vector3 maxScale = new Vector3(2f, 2f, 2f);
    public float duration = 1f; // 1往復にかかる時間

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        float t = (Mathf.Sin(timer * Mathf.PI * 2f / duration) + 1f) / 2f;
        // 0〜1をループ
        transform.localScale = Vector3.Lerp(minScale, maxScale, t);
    }
}
