using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FlowUpdate(4f * Time.deltaTime));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FlowUpdate(float interval)
    {
        while(true)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            yield return new WaitForSeconds(interval);

            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(false);
            yield return new WaitForSeconds(interval);

            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(true);
            yield return new WaitForSeconds(interval);
        }
    }
}
