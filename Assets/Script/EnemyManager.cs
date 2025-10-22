using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public enum EnemyType
    {
        MoveLoop
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
        }
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
}
