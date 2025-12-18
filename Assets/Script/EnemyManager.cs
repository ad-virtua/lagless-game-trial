using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Events")]
    public UnityEvent InScreen;

    public enum EnemyType
    {
        MoveLoop,
        Idle,
        Shutter,
        Follow
    }

    private void Awake()
    {
        instance = this;
    }

    public void CreateEnemyScript(GameObject enemy, EnemyType type)
    {
        switch(type)
        {
            case EnemyType.MoveLoop:
                enemy.AddComponent<EnemyMoveLoop>();
                break;
            case EnemyType.Idle:
                enemy.AddComponent<EnemyIdle>();
                break;
            case EnemyType.Shutter:
                enemy.AddComponent<EnemyShutter>();
                break;
            case EnemyType.Follow:
                enemy.AddComponent<EnemyFollow>();
                break;
        }
        enemy.AddComponent<ScreenRangeChecker>();
    }

    public IEnumerator AnimSpeed(SpriteRenderer renderer, Sprite[] targetAnim, float targetSpeed, EnemyParameters.AnimType animType, EnemyParameters.AnimType targetAnimType, bool isNotLoop = false)
    {
        while (animType == targetAnimType)
        {
            for (int i = 0; i < targetAnim.Length; i++)
            {
                if (animType != targetAnimType) yield break;
                renderer.sprite = targetAnim[i];

                yield return new WaitForSeconds(targetSpeed);
            }

            if (isNotLoop) yield break;
        }
    }

    public IEnumerator Damage(GameObject enemy, int damage)
    {
        int hp = 0;

        if (enemy.GetComponent<EnemyMoveLoop>()) hp = enemy.GetComponent<EnemyMoveLoop>().hp -= damage;
        if (enemy.GetComponent<EnemyIdle>()) hp = enemy.GetComponent<EnemyIdle>().hp -= damage;
        if (enemy.GetComponent<EnemyShutter>()) hp = enemy.GetComponent<EnemyShutter>().hp -= damage;
        if (enemy.GetComponent<EnemyFollow>()) hp = enemy.GetComponent<EnemyFollow>().hp -= damage;

        // ★ DamageFlash が終わるまで待つ
        yield return StartCoroutine(Generic.DamageFlash(
            enemy.GetComponent<SpriteRenderer>(), 0.05f, 4
        ));

        // ★ Flash 終了後に Destroy 判定
        if (hp <= 0)
        {
            Destroy(enemy);
        }
    }

}
