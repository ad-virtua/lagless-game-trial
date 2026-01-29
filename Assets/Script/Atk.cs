using System.Collections;
using UnityEngine;

public class Atk : MonoBehaviour
{
    public float launchForce = 10f;
    private ScreenRangeChecker screenRangeChecker;
    private Rigidbody2D rb;
    private bool isRbPaused;
    private Vector2 savedVelocity;
    private float savedAngularVelocity;
    private Coroutine destroyRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        screenRangeChecker = GetComponent<ScreenRangeChecker>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
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

            // ■ 画面移動中は待機する
            while (StageMoveSystem.instance.isScreenMove || ScenesManagers.instance.isPause)
            {
                yield return null;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
    }

    public void StartAtk()
    {
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

        if (ScenesManagers.instance.isPause) PauseRigidbody();

        //一定時間後にオブジェクトを削除する（ポーズ中は停止）
        if (destroyRoutine != null) StopCoroutine(destroyRoutine);
        destroyRoutine = StartCoroutine(DestroyAfterSecondsPause(5f));
    }

    private void Update()
    {
        if (ScenesManagers.instance.isPause)
        {
            PauseRigidbody();
            return;
        }

        ResumeRigidbody();

        if (!screenRangeChecker) return;

        if (!screenRangeChecker.IsInScreen() ||
            StageMoveSystem.instance.isPlayerScreenMove ||
             StageMoveSystem.instance.isScreenMove)
        {
            Destroy(gameObject);
        }
    }

    private void PauseRigidbody()
    {
        if (rb == null || isRbPaused) return;
        savedVelocity = rb.velocity;
        savedAngularVelocity = rb.angularVelocity;
        rb.simulated = false;
        isRbPaused = true;
    }

    private void ResumeRigidbody()
    {
        if (rb == null || !isRbPaused) return;
        rb.simulated = true;
        rb.velocity = savedVelocity;
        rb.angularVelocity = savedAngularVelocity;
        isRbPaused = false;
    }

    private IEnumerator DestroyAfterSecondsPause(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (ScenesManagers.instance.isPause)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}