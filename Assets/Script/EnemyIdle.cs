using System.Collections;
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
    private int startHP;

    private Vector3 baseScale;

    // Start is called before the first frame update
    void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        startHP = hp = enemyParameters.hp;

        baseScale = transform.localScale;

        if (enemyParameters.atk != null) StartCoroutine(ATK(3f));
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

        if (enemyParameters.rotSpeed != 0)
        {
            transform.Rotate(0, 0, enemyParameters.rotSpeed);
        }
    }

    IEnumerator ATK(float intervalTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalTime);

            if (hp > (startHP / 2))
            {
                Instantiate(enemyParameters.atk, transform.GetChild(Random.Range(0, transform.childCount)));
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Instantiate(enemyParameters.atk, transform.GetChild(i));
                }
            }
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
