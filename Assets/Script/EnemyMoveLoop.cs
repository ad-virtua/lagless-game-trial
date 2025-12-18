using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveLoop : MonoBehaviour
{
    [HideInInspector] public int hp;

    private SpriteRenderer spriteRenderer;

    private float startX, startY;
    private int direction; // -1 or +1
    private Vector3 startScale;

    private int startHP;
    private bool isPlayerDistanceRange;

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
        startHP = hp = enemyParameters.hp;

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

        if (!enemyParameters.isCustomAnim) StartCoroutine(EnemyManager.instance.AnimSpeed(spriteRenderer, enemyParameters.run, enemyParameters.runAnimSpeed, animType, EnemyParameters.AnimType.Run));
    }

    // Update is called once per frame
    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove) return;

        if (GetComponent<ScreenRangeChecker>() && !isPlayerDistanceRange)
        {
            if (!GetComponent<ScreenRangeChecker>().IsInScreen()) return;
            else
            {
                EnemyManager.instance.InScreen?.Invoke();
                isPlayerDistanceRange = true;
                if (enemyParameters.atk != null && enemyParameters.atk.Length != 0) StartCoroutine(ATK(3f, 5f));
            }
        }

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
        if (!enemyParameters.isDirectionLock)
        {
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

        if (enemyParameters.rotSpeed != 0)
        {
            transform.Rotate(0, 0, enemyParameters.rotSpeed);
        }
    }

    IEnumerator ATK(float atkIntervalTime, float createIntervalTime)
    {
        List<Vector3> pos = new List<Vector3>();
        List<Quaternion> rot = new List<Quaternion>();

        for (int i = 0; i < transform.childCount; i++)
        {
            pos.Add(transform.GetChild(i).localPosition);
            rot.Add(transform.GetChild(i).localRotation);
        }

        while (true)
        {
            yield return new WaitForSeconds(atkIntervalTime / 2f);
            spriteRenderer.sprite = enemyParameters.atk[0];

            yield return new WaitForSeconds(atkIntervalTime / 2f);
            spriteRenderer.sprite = enemyParameters.atk[1];

            if (hp > (startHP / 2))
            {
                for (int i = 0; i < transform.childCount / 2; i++)
                {
                    transform.GetChild(i).GetComponent<Atk>().StartAtk();
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetComponent<Atk>().StartAtk();
                }
            }

            yield return new WaitForSeconds(createIntervalTime / 2f);
            StartCoroutine(EnemyManager.instance.AnimSpeed(spriteRenderer, enemyParameters.run, enemyParameters.runAnimSpeed, animType, EnemyParameters.AnimType.Run));

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < pos.Count; i++)
            {
                GameObject child = Instantiate(enemyParameters.atkPrefab, transform);
                child.transform.localPosition = pos[i];
                child.transform.localRotation = rot[i];
            }

            yield return new WaitForSeconds(createIntervalTime / 2f);
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
            if (hp < 0)
            {
                if (gameObject.tag == "Boss")
                {
                    StartCoroutine(StageMoveSystem.instance.BossClear());
                }
                Destroy(gameObject);
            }
        }
    }
}
