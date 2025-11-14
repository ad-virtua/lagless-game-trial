using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShutter : MonoBehaviour
{
    private int hp;

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
        if (StageMoveSystem.instance.isScreenMove) return;

        if (transform.position.y > startPos.y)
        {
            transform.Translate(0f, -0.01f, 0f);
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

        }
    }
}
