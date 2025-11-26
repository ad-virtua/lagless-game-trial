using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    public Transform targetPosition;   // 動いた後の位置
    public float moveSpeed = 2f;

    private Vector3 startPosition;     // 元の位置
    private bool isPlayerOn = false;   // プレイヤーが乗っているか
    private Transform playerParentCache = null;

    public Transform rootPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = true;
            playerParentCache = collision.transform.parent;
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
            collision.transform.SetParent(playerParentCache);
        }
    }

    private void Update()
    {
        if (isPlayerOn)
        {
            // ターゲット座標へ移動
            transform.position = Vector3.MoveTowards(
                transform.position,
                rootPosition.position + targetPosition.position,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            // 元の場所へ戻る
            transform.position = Vector3.MoveTowards(
                transform.position,
                rootPosition.position + startPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
