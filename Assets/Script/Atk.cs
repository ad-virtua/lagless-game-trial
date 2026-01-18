using System.Collections;
using UnityEngine;

public class Atk : MonoBehaviour
{
    public float launchForce = 10f;
    private ScreenRangeChecker screenRangeChecker;

    private void Start()
    {
        screenRangeChecker = GetComponent<ScreenRangeChecker>();
    }

    public IEnumerator StandbyLerp(Vector3 target, float duration)
    {
        if (duration <= 0f)
        {
            transform.localPosition = target;
            yield break;
        }

        Vector3 start = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null) yield break; // 破棄対策
            if (GameSystemOwner.isClear) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }

    public void StartAtk()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 1. オブジェクトの現在の「上方向」（ローカルY軸）を取得
            // このベクトル（transform.up）は、すでにインスペクターで設定された
            // Z軸回転（Transform.rotation）を完全に反映しています。
            Vector2 launchDirection = transform.up;

            // 2. その方向へ指定した強さの力を加える
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

            // Debug.Logを有効にして動作を確認する
            // Debug.Log($"発射方向: {launchDirection}, 角度: {transform.rotation.eulerAngles.z}度");
        }
        else
        {
            Debug.LogError("Rigidbody2Dコンポーネントが見つかりません。このスクリプトはRigidbody2Dが必要です。", this);
        }

        //一定時間後にオブジェクトを削除する
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (!screenRangeChecker) return;

        if (!screenRangeChecker.IsInScreen() ||
            StageMoveSystem.instance.isPlayerScreenMove ||
             StageMoveSystem.instance.isScreenMove)
        {
            Destroy(gameObject);
        }
    }
}