using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    private int blinkCount;

    // Start is called before the first frame update
    void Start()
    {
        blinkCount = 0;
        StartCoroutine(BlinkUpdate(1f * Time.deltaTime));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator BlinkUpdate(float interval)
    {
        while (true)
        {
            float rand = Random.Range(3.0f, 7.0f);

            yield return new WaitForSeconds(rand);

            while (true)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (i == blinkCount) transform.GetChild(blinkCount).gameObject.SetActive(true);
                    else transform.GetChild(i).gameObject.SetActive(false);
                }
                if (blinkCount == transform.childCount - 1) break;
                blinkCount++;

                yield return new WaitForSeconds(interval);
            }

            while (true)
            {
                for (int i = transform.childCount - 1; i > 0; i--)
                {
                    if (i == blinkCount) transform.GetChild(blinkCount).gameObject.SetActive(true);
                    else transform.GetChild(i).gameObject.SetActive(false);
                }
                if (blinkCount == 0) break;
                blinkCount--;

                yield return new WaitForSeconds(interval);
            }
        }
    }
}
