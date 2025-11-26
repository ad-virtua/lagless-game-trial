using UnityEngine;

/// <summary>
/// 2D空間で真上にRayを飛ばし、
/// "Player"タグと自分自身以外のオブジェクトに最初に当たった距離を計測します。
/// </summary>
public class PlayerDistanceChecker2D : MonoBehaviour
{
    [Header("Rayの設定")]
    [Tooltip("Rayの最大距離")]
    public float maxDistance = 100f;

    [Tooltip("この距離以下になったら処理を実行する（上下両方）")]
    public float triggerDistance = 1.0f;

    [Header("計測結果（デバッグ用）")]
    [Tooltip("上方向の検出距離（未検出: -1）")]
    [SerializeField]
    private float distanceUp = -1f;

    [Tooltip("下方向の検出距離（未検出: -1）")]
    [SerializeField]
    private float distanceDown = -1f;

    // 処理を毎フレーム実行するかどうか（falseなら1回だけ実行）
    [Header("処理の実行")]
    [Tooltip("true: 条件を満たしている間、毎フレーム処理を実行\nfalse: 条件を満たした瞬間に1回だけ処理を実行")]
    public bool executePerFrame = false;

    // 1回だけ実行するためのフラグ
    private bool actionTriggered = false;


    void Update()
    {
        // 1. 上下の距離をそれぞれ計測
        distanceUp = MeasureDistance(Vector2.up);
        distanceDown = MeasureDistance(Vector2.down);

        // 2. 条件判定
        // 上方向がヒットしており、かつtriggerDistance以下か？
        bool isUpClose = (distanceUp > 0 && distanceUp <= triggerDistance);
        // 下方向がヒットしており、かつtriggerDistance以下か？
        bool isDownClose = (distanceDown > 0 && distanceDown <= triggerDistance);

        float gameOverOffset = 0.1f;
        bool isGameOverCloseUp = (distanceUp > 0 && distanceUp <= (triggerDistance - gameOverOffset));
        bool isGameOverCloseDown = (distanceDown > 0 && distanceDown <= (triggerDistance - gameOverOffset));

        // 3. 上下両方が条件を満たした場合
        if (isGameOverCloseUp && isGameOverCloseDown)
        {
            if (executePerFrame)
            {
                // 毎フレーム実行する場合
                OnBothSidesClose();
            }
            else if (!actionTriggered)
            {
                // 1回だけ実行する場合
                OnBothSidesClose();
                actionTriggered = true; // 実行済みフラグを立てる
            }
        }
        else
        {
            // 条件を満たさなくなったら、フラグをリセット（再び範囲内に入ったら実行できるようにする）
            actionTriggered = false;
        }

        if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) < 0.1f && isDownClose)
        {
            GetComponent<Player>().isGrounded = true;
        }
    }

    /// <summary>
    /// 指定した方向にRayを飛ばし、"Player"と自分以外との最短距離を返す
    /// </summary>
    /// <param name="direction">Rayの方向 (Vector2.up または Vector2.down)</param>
    /// <returns>ヒットした距離。見つからなければ -1 を返す</returns>
    private float MeasureDistance(Vector2 direction)
    {
        // 指定方向にRayを飛ばし、当たったもの全てを取得
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, maxDistance);

        // ヒットしたものを近い順にチェック
        foreach (RaycastHit2D hit in hits)
        {
            // 1. 自分自身のコライダーに当たった場合は無視する
            if (hit.collider.gameObject == this.gameObject)
            {
                continue; // 次のチェックへ
            }

            // 2. "Player" タグが付いている場合も無視する
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy"))
            {
                continue; // 次のチェックへ
            }

            // 3.「自分以外」かつ「Player以外」で最初に見つかったオブジェクトの距離を返す
            return hit.distance;
        }

        // 何も見つからなかった場合
        return -1f;
    }

    // --- ここに関数を追加しました ---

    /// <summary>
    /// 上下の距離が両方とも triggerDistance 以下になった時に呼び出される関数
    /// </summary>
    private void OnBothSidesClose()
    {
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        // ★
        // ★  ここに、上下が接近した時に実行したい処理を記述します
        // ★
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

        // (実行例) デバッグログにメッセージを表示
        Debug.Log("上下が接近しました！ 処理を実行します。 (上:" + distanceUp + ", 下:" + distanceDown + ")");

        GetComponent<Player>().DamageHP(9999);
    }


    /// <summary>
    /// （ギズモ描画用）シーンビューで上下のRayを可視化します
    /// </summary>
    void OnDrawGizmos()
    {
        // 上方向のRayを描画
        DrawRayGizmo(Vector2.up, distanceUp);

        // 下方向のRayを描画
        DrawRayGizmo(Vector2.down, distanceDown);
    }

    /// <summary>
    /// GizmoでRayを描画するための補助関数
    /// </summary>
    private void DrawRayGizmo(Vector2 direction, float hitDistance)
    {
        float distanceToDraw = maxDistance;
        Color gizmoColor = Color.gray; // デフォルト（未検出）

        if (hitDistance > 0)
        {
            // 何かにヒットした場合
            distanceToDraw = hitDistance;

            // 閾値（triggerDistance）以下かどうかで色分け
            if (hitDistance <= triggerDistance)
            {
                gizmoColor = Color.red; // 閾値以下（実行条件を満たしている）
            }
            else
            {
                gizmoColor = Color.green; // 閾値より遠い（ヒットはしている）
            }
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawRay(transform.position, direction * distanceToDraw);
    }
}