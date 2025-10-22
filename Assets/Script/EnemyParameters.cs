using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Parameters")]
public class EnemyParameters : ScriptableObject
{
    public Sprite[] idle, run, jump;
    public float moveSpeed, jumpForce;
    public float idleAnimSpeed, runAnimSpeed, jumpAnimSpeed;
    public float moveLoopDistance = 3f;
    public bool moveToLeftFirst = true;
    public bool isSpriteLeft = true;
    public int hp;

    public enum AnimType { Idle, Run, Jump }
}
