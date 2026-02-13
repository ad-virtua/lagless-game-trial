using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private Transform target;
    public float speed = 3f;
    public Transform goalPortal;
    public bool isSpecialScreenMove;
    public float specialMove;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public Transform GetGoalPortal()
    {
        return goalPortal;
    }
}
