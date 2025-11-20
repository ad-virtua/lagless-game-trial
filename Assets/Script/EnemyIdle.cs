using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdle : MonoBehaviour
{
    private int hp;

    private EnemyTypeSelecter enemyTypeSelecter;
    private EnemyParameters enemyParameters;

    [HideInInspector]
    public EnemyParameters.AnimType animType;

    private float minScale = 0.95f;  // 最小スケール
    private float maxScale = 1.05f;  // 最大スケール
    private float speed = 2f;     // スピード

    private Vector3 baseScale;

    // Start is called before the first frame update
    void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;

        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (enemyParameters.idelMotion)
        {
            // 0〜1を往復する値
            float t = Mathf.PingPong(Time.time * speed, 1f);
            // tを使ってスケールを補間
            float scale = Mathf.Lerp(minScale, maxScale, t);
            transform.localScale = baseScale * scale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 4));
            Destroy(collision.gameObject);

            if (hp == 9999) return;
            hp--;
            if (hp == 0) Destroy(gameObject);
        }
    }
}
