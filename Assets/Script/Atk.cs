using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atk : MonoBehaviour
{
    private Transform parent;       // 親オブジェクト
    public float speed = 5f;       // 移動スピード

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
    }

    void Update()
    {
        Vector3 dir = parent.TransformDirection(Vector3.up).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

}
