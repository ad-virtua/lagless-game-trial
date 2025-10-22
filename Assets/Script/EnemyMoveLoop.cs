using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMoveLoop : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float startX;
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

        Vector3 pos = transform.localPosition;
        pos.x = Random.Range(startX - enemyParameters.moveLoopDistance, startX + enemyParameters.moveLoopDistance);
        transform.localPosition = pos;

        direction = enemyParameters.moveToLeftFirst ? -1 : 1;
        startScale = transform.localScale;

        StartCoroutine(EnemyManager.instance.AnimSpeed(spriteRenderer, enemyParameters.run, enemyParameters.runAnimSpeed, animType, EnemyParameters.AnimType.Run));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.localPosition;

        // 移動
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
    }

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
}
