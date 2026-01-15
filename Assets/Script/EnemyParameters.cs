using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Parameters")]
public class EnemyParameters : ScriptableObject
{
    public Sprite[] idle, run, jump, atk, end;
    public float moveSpeed, jumpForce;
    public float idleAnimSpeed, runAnimSpeed, jumpAnimSpeed;
    public float moveLoopDistance = 3f;
    public float rotSpeed = 0f;
    public float followStartRange = 8f;
    public float followStopRange = 12f;
    public float turnOffset = 0.8f;
    public float followAtkInterval = 3f;
    public bool followOnlyX = false;
    public LayerMask followBlockMask;
    public bool moveToLeftFirst = true;
    public bool idelMotion = false;
    public bool isSpriteLeft = true;
    public bool isDirectionLock = true;
    public bool isMoveHigh = false;
    public bool isCustomAnim = false;
    public int hp;
    public GameObject atkPrefab;

    public enum AnimType { Idle, Run, Jump, ATK }
}
