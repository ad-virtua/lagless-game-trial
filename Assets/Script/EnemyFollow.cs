using System.Collections;
using UnityEngine;
using static EnemyParameters;

public class EnemyFollow : MonoBehaviour
{
    [HideInInspector] public int hp;

    private SpriteRenderer spriteRenderer;

    private float startX, startY;
    private int direction; // -1 or +1
    private Vector3 startScale;

    private EnemyTypeSelecter enemyTypeSelecter;
    private EnemyParameters enemyParameters;

    [HideInInspector]
    public AnimType animType;

    private Transform player;
    private Camera mainCam;

    private bool isFollowing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCam = Camera.main;

        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animType = AnimType.Run;

        startX = transform.localPosition.x;
        startY = transform.localPosition.y;

        // 初期位置ランダム
        Vector3 pos = transform.localPosition;
        if (!enemyParameters.isMoveHigh)
            pos.x = Random.Range(startX - enemyParameters.moveLoopDistance, startX + enemyParameters.moveLoopDistance);
        else
            pos.y = Random.Range(startY - enemyParameters.moveLoopDistance, startY + enemyParameters.moveLoopDistance);
        transform.localPosition = pos;

        direction = enemyParameters.moveToLeftFirst ? -1 : 1;
        startScale = transform.localScale;

        StartCoroutine(AnimSpeed(
            enemyParameters.run,
            enemyParameters.runAnimSpeed,
            AnimType.Run));
    }

    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // --- 追尾開始 ---
        if (distance < enemyParameters.followStartRange && IsVisibleOnScreen())
            isFollowing = true;

        // --- 追尾終了（画面外 or 遠距離） ---
        if (distance > enemyParameters.followStopRange || !IsVisibleOnScreen())
        {
            // 追尾解除 → direction を復元
            if (isFollowing)
            {
                if (!enemyParameters.isMoveHigh)
                    direction = transform.position.x > startX ? -1 : 1;
                else
                    direction = transform.position.y > startY ? -1 : 1;
            }

            isFollowing = false;
        }

        // --------- 追尾モード ---------
        if (isFollowing)
        {
            FollowWithOffset();
            return; // 追尾中はループしない
        }

        // --------- 往復移動 ---------
        LoopMove();
    }

    // ======================================================
    // 追尾（蛇行つき・オブジェクトは回転しない）
    // ======================================================
    void FollowWithOffset()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;

        // 横方向の揺れ（蛇行）
        Vector3 side = Vector3.Cross(toPlayer, Vector3.up).normalized;
        float dir = Mathf.Sin(Time.time * 0.8f);

        Vector3 offsetDir = (toPlayer + side * dir * enemyParameters.turnOffset).normalized;

        // 移動のみ（回転しない）
        transform.position += offsetDir * enemyParameters.moveSpeed * Time.deltaTime;

        // sprite の向きだけ調整
        if (offsetDir.x > 0)
            transform.localScale = new Vector3(-startScale.x, startScale.y, startScale.z);
        else
            transform.localScale = startScale;
    }

    // ======================================================
    // 画面内判定
    // ======================================================
    bool IsVisibleOnScreen()
    {
        Vector3 screen = mainCam.WorldToViewportPoint(transform.position);
        return (screen.x > 0 && screen.x < 1 &&
                screen.y > 0 && screen.y < 1 &&
                screen.z > 0);
    }

    // ======================================================
    // 元の左右ループ移動（変化なし）
    // ======================================================
    void LoopMove()
    {
        Vector3 pos = transform.localPosition;

        if (!enemyParameters.isMoveHigh)
        {
            pos.x += enemyParameters.moveSpeed * direction * Time.deltaTime;
            pos.y = Mathf.Lerp(pos.y, startY, enemyParameters.moveSpeed * Time.deltaTime);
            transform.position = pos;

            transform.localPosition = pos;

            if (pos.x >= startX + enemyParameters.moveLoopDistance)
                direction = -1;
            else if (pos.x <= startX - enemyParameters.moveLoopDistance)
                direction = 1;
        }
        else
        {
            pos.y += enemyParameters.moveSpeed * direction * Time.deltaTime;
            pos.x = Mathf.Lerp(pos.x, startX, enemyParameters.moveSpeed * Time.deltaTime);
            transform.localPosition = pos;

            if (pos.y >= startY + enemyParameters.moveLoopDistance)
                direction = -1;
            else if (pos.y <= startY - enemyParameters.moveLoopDistance)
                direction = 1;
        }

        // sprite flip
        if (enemyParameters.isSpriteLeft)
        {
            if (direction == 1)
                transform.localScale = new Vector3(-startScale.x, startScale.y, startScale.z);
            else
                transform.localScale = startScale;
        }
        else
        {
            if (direction == 1)
                transform.localScale = startScale;
            else
                transform.localScale = new Vector3(-startScale.x, startScale.y, startScale.z);
        }
    }

    // ======================================================
    // ダメージ処理
    // ======================================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 4));
            Destroy(collision.gameObject);

            hp--;
            if (hp == 0) Destroy(gameObject);
        }
    }

    public IEnumerator AnimSpeed(Sprite[] targetAnim, float targetSpeed, AnimType targetAnimType, bool isNotLoop = false)
    {
        while (animType == targetAnimType)
        {
            for (int i = 0; i < targetAnim.Length; i++)
            {
                if (animType != targetAnimType) yield break;
                spriteRenderer.sprite = targetAnim[i];

                yield return new WaitForSeconds(targetSpeed);
            }

            if (isNotLoop) yield break;
        }
    }
}
