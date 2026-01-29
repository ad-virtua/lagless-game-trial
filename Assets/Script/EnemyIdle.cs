using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdle : MonoBehaviour
{
    [HideInInspector] public int hp;

    private EnemyTypeSelecter enemyTypeSelecter;
    private EnemyParameters enemyParameters;

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

        if (enemyParameters.atk != null) StartCoroutine(ATK(3f, 5f));
    }

    private void Update()
    {
        if (GameSystemOwner.isGameOver || ScenesManagers.instance.isPause) return;

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

    IEnumerator ATK(float atkIntervalTime, float createIntervalTime)
    {
        if (enemyParameters.atkPrefab == null)
        {
            Debug.LogWarning($"EnemyIdle ATK skipped: atkPrefab not set for {name}");
            yield break;
        }

        List<Vector3> pos = new List<Vector3>();
        List<Quaternion> rot = new List<Quaternion>();

        for (int i = 0; i < transform.childCount; i++)
        {
            pos.Add(transform.GetChild(i).localPosition);
            rot.Add(transform.GetChild(i).localRotation);
        }

        while (true)
        {
            yield return new WaitForSeconds(atkIntervalTime);

            bool isHighHP = hp > (startHP / 2);
            int currentChildCount = transform.childCount;
            int usableCount = Mathf.Min(currentChildCount, pos.Count);
            int atkCount = isHighHP ? usableCount / 2 : usableCount;

            for (int i = 0; i < atkCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<Atk>(out var atk)) atk.StartAtk();
            }

            yield return new WaitForSeconds(createIntervalTime);

            // 古い子を破棄して増殖を防ぐ
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            int spawnCount = isHighHP ? pos.Count / 2 : pos.Count;
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject child = Instantiate(enemyParameters.atkPrefab, transform);
                child.transform.localPosition = pos[i];
                child.transform.localRotation = rot[i];
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
