using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static EnemyParameters;

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
    private ScreenRangeChecker screenRangeChecker;

    // ATK用に初期位置とコンポーネントをキャッシュしておく（毎ループのInstantiate/Destroyを回避）
    private readonly List<Atk> atkChildren = new List<Atk>();
    private readonly List<Vector3> atkStartPos = new List<Vector3>();
    private readonly List<Quaternion> atkStartRot = new List<Quaternion>();

    [HideInInspector]
    public AnimType animType;

    // ── Boss トゲ回転攻撃用 ──
    [Header("Boss Spike Rotation")]
    [SerializeField] private float spikeRotSpeed = 90f;        // 通常時の回転速度 (度/秒)
    [SerializeField] private float spikeRotSpeedFast = 180f;   // HP半分以下の回転速度 (度/秒)

    // Boss > Rot > ATKPoints 構造用
    private bool isBoss;
    private bool isSpikeRotating;
    private bool bossDeathTriggered; // BossClear の多重呼び出しを防ぐ
    private Transform rotGroup;      // Boss の子 "Rot" オブジェクト
    private readonly List<Vector3> spikeStartPos = new List<Vector3>();
    private readonly List<Quaternion> spikeStartRot = new List<Quaternion>();

    // Start is called before the first frame update
    void Start()
    {
        enemyTypeSelecter = GetComponent<EnemyTypeSelecter>();
        enemyParameters = enemyTypeSelecter.enemyParameters;
        hp = enemyParameters.hp;
        startHP = hp = enemyParameters.hp;

        spriteRenderer = GetComponent<SpriteRenderer>();
        screenRangeChecker = GetComponent<ScreenRangeChecker>();
        animType = AnimType.Run;

        isBoss = transform.CompareTag("Boss");

        if (isBoss)
        {
            CacheRotGroup();

            // キャッシュ完了後、参照用に配置した ATKPoint を削除
            if (rotGroup != null)
                foreach (Transform child in rotGroup)
                    Destroy(child.gameObject);

            isSpikeRotating = true; // ゲーム開始直後から回転
        }
        else
        {
            CacheAtkChildren();
        }

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
        if (GameSystemOwner.isGameOver || ScenesManagers.instance.isPause) return;

        if (StageMoveSystem.instance.isScreenMove || GameSystemOwner.isClear || GameSystemOwner.instance.IsPlayMovie()) return;

        if (screenRangeChecker && !isPlayerDistanceRange)
        {
            if (!screenRangeChecker.IsInScreen()) return;
            else
            {
                if (transform.tag == "Boss") EnemyManager.instance.InScreen?.Invoke();
                isPlayerDistanceRange = true;
                if (enemyParameters.atk != null && enemyParameters.atk.Length != 0)
                {
                    if (isBoss)
                        StartCoroutine(BossATK(3f, 5f));
                    else
                        StartCoroutine(ATK(3f, 5f));
                }
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

        // ── Boss: Rot グループの回転処理 ──
        if (isBoss && isSpikeRotating && rotGroup != null)
        {
            float speed = (hp > startHP / 2) ? spikeRotSpeed : spikeRotSpeedFast;
            rotGroup.Rotate(0, 0, speed * Time.deltaTime);
        }
    }

    // ── Boss 用: Rot グループと ATKPoint のキャッシュ ──
    private void CacheRotGroup()
    {
        if (transform.childCount == 0) return;

        rotGroup = transform.GetChild(0); // "Rot" オブジェクト

        spikeStartPos.Clear();
        spikeStartRot.Clear();

        for (int i = 0; i < rotGroup.childCount; i++)
        {
            spikeStartPos.Add(rotGroup.GetChild(i).localPosition);
            spikeStartRot.Add(rotGroup.GetChild(i).localRotation);
        }
    }

    // ── Boss 用 ATK コルーチン ──
    IEnumerator BossATK(float atkIntervalTime, float createIntervalTime)
    {
        // 初回: ATKPoint を生え変わりアニメから開始
        isSpikeRotating = false;
        if (rotGroup != null) rotGroup.localRotation = Quaternion.identity;
        yield return StartCoroutine(RespawnSpikes(createIntervalTime));

        while (true)
        {
            // ムービー中は待機
            yield return StartCoroutine(WaitWhileMovie());

            animType = AnimType.ATK;
            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            spriteRenderer.sprite = enemyParameters.atk[0];

            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            spriteRenderer.sprite = enemyParameters.atk[1];

            // ── 回転停止 → 全 ATKPoint 発射 ──
            yield return StartCoroutine(WaitWhileMovie());
            float rand = Random.Range(0.0f, 1.0f);
            yield return new WaitForSeconds(rand);
            isSpikeRotating = false;

            if (rotGroup != null)
            {
                for (int i = 0; i < rotGroup.childCount; i++)
                {
                    Atk atk = rotGroup.GetChild(i).GetComponent<Atk>();
                    if (atk) atk.StartAtk();
                }
            }

            yield return new WaitForSeconds(createIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            animType = AnimType.Run;
            StartCoroutine(AnimSpeed(enemyParameters.run, enemyParameters.runAnimSpeed, AnimType.Run));

            // ── 古い ATKPoint を破棄 ──
            if (rotGroup != null)
            {
                foreach (Transform child in rotGroup) Destroy(child.gameObject);
                rotGroup.localRotation = Quaternion.identity;
            }

            // ── ATKPoint 再生成 ──
            yield return StartCoroutine(RespawnSpikes(createIntervalTime));
        }
    }

    // ── ATKPoint 再生成ヘルパー ──
    IEnumerator RespawnSpikes(float createIntervalTime)
    {
        for (int i = 0; i < spikeStartPos.Count; i++)
        {
            yield return StartCoroutine(WaitWhileMovie());
            while (StageMoveSystem.instance.isScreenMove || ScenesManagers.instance.isPause)
                yield return null;

            GameObject child = Instantiate(enemyParameters.atkPrefab, rotGroup);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = spikeStartRot[i];
            StartCoroutine(child.GetComponent<Atk>().StandbyLerp(spikeStartPos[i], 1f));

            yield return new WaitForSeconds(0.3f);
        }

        // 再生成完了 → 回転再開
        isSpikeRotating = true;

        yield return new WaitForSeconds(createIntervalTime / 4f);
    }

    // ── 通常敵用 ATK コルーチン (従来のロジック) ──
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
            // ムービー中は待機
            yield return StartCoroutine(WaitWhileMovie());

            animType = AnimType.ATK;
            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            spriteRenderer.sprite = enemyParameters.atk[0];

            yield return new WaitForSeconds(atkIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            spriteRenderer.sprite = enemyParameters.atk[1];

            // 攻撃発射
            yield return StartCoroutine(WaitWhileMovie());

            if (hp > (startHP / 2))
            {
                for (int i = 0; i < transform.childCount / 2; i++)
                {
                    Atk atk = transform.GetChild(i).GetComponent<Atk>();
                    if (atk) atk.StartAtk();
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Atk atk = transform.GetChild(i).GetComponent<Atk>();
                    if (atk) atk.StartAtk();
                }
            }

            yield return new WaitForSeconds(createIntervalTime / 4f);
            if (GameSystemOwner.isClear) yield break;

            yield return StartCoroutine(WaitWhileMovie());
            animType = AnimType.Run;
            StartCoroutine(
                AnimSpeed(enemyParameters.run, enemyParameters.runAnimSpeed, AnimType.Run)
            );

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < pos.Count; i++)
            {
                yield return StartCoroutine(WaitWhileMovie());

                // ■ 画面移動中は待機する
                while (StageMoveSystem.instance.isScreenMove || ScenesManagers.instance.isPause)
                {
                    yield return null;
                }

                GameObject child = Instantiate(enemyParameters.atkPrefab, transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = rot[i];

                StartCoroutine(
                    child.GetComponent<Atk>().StandbyLerp(pos[i], 1f)
                );

                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(createIntervalTime / 4f);
        }
    }

    IEnumerator WaitWhileMovie()
    {
        while (GameSystemOwner.instance != null &&
               GameSystemOwner.instance.IsPlayMovie())
        {
            if (GameSystemOwner.isClear) yield break;
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Shot")
        {
            Destroy(collision.gameObject);
            if (hp == 9999) return;
            if (isBoss && bossDeathTriggered) return; // 死亡演出中は点滅・HP減算をしない
            StartCoroutine(Generic.DamageFlash(spriteRenderer, 0.05f, 4));

            // Boss: Rot 以下の全 SpriteRenderer をダメージフラッシュ
            if (isBoss)
            {
                foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sr != spriteRenderer)
                        StartCoroutine(Generic.DamageFlash(sr, 0.05f, 4));
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    StartCoroutine(Generic.DamageFlash(transform.GetChild(i).GetComponent<SpriteRenderer>(), 0.05f, 4));
                }
            }

            hp--;
            if (hp < 0)
            {
                if (isBoss)
                {
                    if (!bossDeathTriggered)
                    {
                        bossDeathTriggered = true;
                        StartCoroutine(StageMoveSystem.instance.BossClear(gameObject));
                    }
                }
                else Destroy(gameObject);
            }
        }
    }

    private void CacheAtkChildren()
    {
        atkChildren.Clear();
        atkStartPos.Clear();
        atkStartRot.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Atk atk = child.GetComponent<Atk>();
            if (atk == null) continue;

            atkChildren.Add(atk);
            atkStartPos.Add(child.localPosition);
            atkStartRot.Add(child.localRotation);
        }
    }

    private void EnsureAtkPool()
    {
        for (int i = 0; i < atkChildren.Count; i++)
        {
            if (atkChildren[i] == null)
            {
                CreateAtkAtIndex(i);
            }
        }
    }

    private Atk CreateAtkAtIndex(int index)
    {
        if (enemyParameters.atkPrefab == null || index < 0 || index >= atkStartRot.Count) return null;

        GameObject child = Instantiate(enemyParameters.atkPrefab, transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = atkStartRot[index];

        Atk atk = child.GetComponent<Atk>();
        if (index < atkChildren.Count)
        {
            atkChildren[index] = atk;
        }
        return atk;
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
