using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMoveLoop : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float startX, startY;
    private int direction; // -1 or +1
    private Vector3 startScale;

    private int hp;

    private EnemyTypeSelecter enemyTypeSelecter;
    private EnemyParameters enemyParameters;

    [HideInInspector]
    public EnemyParameters.AnimType animType;

    // Start is called before the first frame update
    void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animType = EnemyParameters.AnimType.Run;

        startX = transform.localPosition.x;
        startY = transform.localPosition.y;

        Vector3 pos = transform.localPosition;
        if (!enemyParameters.isMoveHigh) pos.x = Random.Range(startX - enemyParameters.moveLoopDistance, startX + enemyParameters.moveLoopDistance);
        else pos.y = Random.Range(startY - enemyParameters.moveLoopDistance, startY + enemyParameters.moveLoopDistance);
        transform.localPosition = pos;

        direction = enemyParameters.moveToLeftFirst ? -1 : 1;
        startScale = transform.localScale;

        StartCoroutine(EnemyManager.instance.AnimSpeed(spriteRenderer, enemyParameters.run, enemyParameters.runAnimSpeed, animType, EnemyParameters.AnimType.Run));
    }

    // Update is called once per frame
    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove) return;

        Vector3 pos = transform.localPosition;

        // 移動
        if (!enemyParameters.isMoveHigh)
        {
            pos.x += enemyParameters.moveSpeed * direction * Time.deltaTime;
            transform.localPosition = pos;

            // 範囲外に出たら反転
            if (pos.x >= startX + enemyParameters.moveLoopDistance)
            {
                direction = -1;
            }
            else if (pos.x <= startX - enemyParameters.moveLoopDistance)
            {
                direction = 1;
            }
        }
        else
        {
            pos.y += enemyParameters.moveSpeed * direction * Time.deltaTime;
            transform.localPosition = pos;

            // 範囲外に出たら反転
            if (pos.y >= startY + enemyParameters.moveLoopDistance)
            {
                direction = -1;
            }
            else if (pos.y <= startY - enemyParameters.moveLoopDistance)
            {
                direction = 1;
            }
        }

        // 見た目の向き反転
        if(enemyParameters.isSpriteLeft)
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

        if (enemyParameters.rotSpeed != 0)
        {
            transform.Rotate(0, 0, enemyParameters.rotSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            Destroy(collision.gameObject);
            if (hp == 9999) return;
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 4));

            hp--;
            if (hp == 0) Destroy(gameObject);
        }
    }
}
