using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private Transform target;
    public float speed = 3f;
    public Transform goalPortal;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /// <summary>
    /// target を goalPos まで移動し、移動が終わったら true を返す
    /// </summary>
    public IEnumerator MoveToPosition()
    {
        if (target == null) yield break;

        LuteinBarrierGauge.instance.AllResetParameter();

        // 移動が終わるまでループ
        while (Vector3.Distance(target.position, goalPortal.position) > 0.01f)
        {
            target.position = Vector3.MoveTowards(
                target.position,
                goalPortal.position,
                speed * Time.deltaTime
            );
            yield return null; // 次のフレームまで待つ
        }

        // 最終位置を正確に
        target.position = goalPortal.position;

        target.GetComponent<Player>().isPortal = false;
        target.GetComponent<BoxCollider2D>().isTrigger = false;
        target.GetComponent<SpriteRenderer>().enabled = true;
    }
}
