using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTypeSelecter : MonoBehaviour
{
    [SerializeField]
    private EnemyManager.EnemyType selectEnemyType;

    public EnemyParameters enemyParameters;

    private void Start()
    {
        EnemyManager.instance.CreateEnemyScript(gameObject, selectEnemyType);
    }
}
