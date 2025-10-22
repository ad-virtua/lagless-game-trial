using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    [SerializeField] private float speed;

    public float playerX;
    private ScreenRangeChecker screenRangeChecker;

    private void Start()
    {
        screenRangeChecker = GetComponent<ScreenRangeChecker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerX > 0) transform.Translate(speed, 0, 0);
        else transform.Translate(-speed, 0, 0);

        if (screenRangeChecker)
        {
            if (screenRangeChecker.IsInScreen() == false) Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Ground")
        {
            Destroy(gameObject);
        }
    }
}
