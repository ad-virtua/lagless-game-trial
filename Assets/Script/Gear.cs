using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float moveTime;
    [SerializeField] private Sprite changeSprite;

    private void Start()
    {
        StartCoroutine(SlowlyMove());
    }

    IEnumerator SlowlyMove()
    {
        float rand = Random.Range(2f, 4f);
        float time = 0.0f;

        yield return new WaitForSeconds(rand);

        while (time < moveTime)
        {
            time += Time.deltaTime;
            transform.Rotate(0, 0, speed, 0);
            yield return null;
        }
        StartCoroutine(SlowlyMove());
    }

    public void ChangeImage()
    {
        GetComponent<SpriteRenderer>().sprite = changeSprite;
    }
}
