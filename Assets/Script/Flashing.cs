using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Flashing : MonoBehaviour
{
    TilemapRenderer renderer;
    TilemapCollider2D collider2D;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<TilemapRenderer>();
        collider2D = GetComponent<TilemapCollider2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(FlashingUpdate());
    }

    IEnumerator FlashingUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(4f);
            renderer.enabled = false;
            collider2D.enabled = false;
            animator.enabled = false;
            yield return new WaitForSeconds(2f);
            renderer.enabled = true;
            collider2D.enabled = true;
            animator.enabled = true;
        }

    }
}
