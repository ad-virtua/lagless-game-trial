using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShutter : MonoBehaviour
{
    [HideInInspector] public int hp;

    private EnemyTypeSelecter enemyTypeSelecter;
    private EnemyParameters enemyParameters;

    [HideInInspector]
    public EnemyParameters.AnimType animType;

    private Vector3 startPos = new Vector3();

    // Start is called before the first frame update
    private void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;
        startPos = transform.position;
    }

    private void Update()
    {
        if (StageMoveSystem.instance.isScreenMove)
        {
            ResetPos();
            return;
        }

        if (transform.position.y > startPos.y)
        {
            transform.Translate(0f, -0.6f * Time.deltaTime, 0f);
            ResetPos();
        }
    }

    private void ResetPos()
    {
        if (transform.position.y < startPos.y)
        {
            Vector3 pos = transform.position;
            pos.y = startPos.y;
            transform.position = pos;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 4));

            Vector3 pos = transform.position;
            pos.y += 1f;
            transform.position = pos;
            Destroy(collision.transform.gameObject);

            if (hp == 9999) return;
            hp--;
            if (hp == 0) Destroy(gameObject);
        }
    }
}
