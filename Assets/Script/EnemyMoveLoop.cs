using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static EnemyParameters;
using static UnityEditor.PlayerSettings;

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
    public AnimType animType;

    // Start is called before the first frame update
    void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;
        startHP = hp = enemyParameters.hp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animType = AnimType.Run;

        startX = transform.localPosition.x;
        startY = transform.localPosition.y;

        Vector3 pos = transform.localPosition;
        if (!enemyParameters.isMoveHigh) pos.x = Random.Range(startX - enemyParameters.moveLoopDistance, startX + enemyParameters.moveLoopDistance);
        else pos.y = Random.Range(startY - enemyParameters.moveLoopDistance, startY + enemyParameters.moveLoopDistance);
        transform.localPosition = pos;

        direction = enemyParameters.moveToLeftFirst ? -1 : 1;
        startScale = transform.localScale;

        StartCoroutine(AnimSpeed(enemyParameters.run, enemyParameters.runAnimSpeed, AnimType.Run));
    }

    // Update is called once per frame
    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove || GameSystemOwner.isClear) return;

        if (GetComponent<ScreenRangeChecker>() && !isPlayerDistanceRange)
        {
            isPlayerDistanceRange = true;
            if (enemyParameters.atk != null && enemyParameters.atk.Length != 0) StartCoroutine(ATK(3f, 5f));
            if (!GetComponent<ScreenRangeChecker>().IsInScreen()) return;
            else
            {
                if (transform.tag == "Boss") EnemyManager.instance.InScreen?.Invoke();
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
            animType = AnimType.ATK;
            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;
            spriteRenderer.sprite = enemyParameters.atk[0];

            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;
            spriteRenderer.sprite = enemyParameters.atk[1];

            if (hp > (startHP / 2))
            {
                for (int i = 0; i < transform.childCount / 2; i++)
                {
                    if (transform.GetChild(i).GetComponent<Atk>()) transform.GetChild(i).GetComponent<Atk>().StartAtk();
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).GetComponent<Atk>()) transform.GetChild(i).GetComponent<Atk>().StartAtk();
                }
            }

            yield return new WaitForSeconds(createIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;
            animType = AnimType.Run;
            StartCoroutine(AnimSpeed(enemyParameters.run, enemyParameters.runAnimSpeed, AnimType.Run));

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < pos.Count; i++)
            {
                GameObject child = Instantiate(enemyParameters.atkPrefab, transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = rot[i];
                StartCoroutine(child.GetComponent<Atk>().StandbyLerp(pos[i], 1f));
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(createIntervalTime / 4f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            Destroy(collision.gameObject);
            if (hp == 9999) return;
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 4));
            for (int i = 0; i < transform.childCount; i++)
            {
                StartCoroutine(Generic.DamageFlash(transform.GetChild(i).GetComponent<SpriteRenderer>(), 0.05f, 4));
            }

            hp--;
            if (hp < 0)
            {
                if (gameObject.tag == "Boss")
                {
                    StartCoroutine(StageMoveSystem.instance.BossClear(gameObject));
                }
                else Destroy(gameObject);
            }
        }
    }

    public IEnumerator AnimSpeed(Sprite[] targetAnim, float targetSpeed, AnimType targetAnimType, bool isNotLoop = false)
    {
        while (animType == targetAnimType)
        {
            for (int i = 0; i < targetAnim.Length; i++)
            {
                if (animType != targetAnimType) yield break;
                if (GameSystemOwner.isClear) yield break;
                spriteRenderer.sprite = targetAnim[i];

                yield return new WaitForSeconds(targetSpeed);
            }

            if (isNotLoop) yield break;
        }
    }
}
