using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Core : MonoBehaviour
{
 [Header("移動範囲（初期位置からの最大距離）")]
    public float maxDistanceX = 5f;
    public float maxDistanceY = 5f;

    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.1f;
    public float waitTime = 1.0f; // 到着後の待機時間

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float waitTimer;

    void Start()
    {
        // 初期位置を記録
        startPosition = transform.position;
        transform.position = startPosition + transform.root.position;
        SetNewTarget();
    }

    void Update()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        Vector3 direction = targetPosition - (transform.position - transform.root.position);
        direction.z = 0; // Z軸は固定

        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                SetNewTarget();
            }
        }
    }

    void SetNewTarget()
    {
        // 初期位置からの範囲内でランダムな目標座標を設定
        float randomX = (startPosition.x + transform.root.position.x) + Random.Range(-maxDistanceX, maxDistanceX);
        float randomY = (startPosition.y + transform.root.position.y) + Random.Range(-maxDistanceY, maxDistanceY);
        targetPosition = new Vector3(randomX, randomY, transform.position.z) - transform.root.position;
    }
}
