using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LuteinBarrierGauge : MonoBehaviour
{
    public static LuteinBarrierGauge instance;
    public GameObject player;
    public GameObject luteinBarrierCircle;

    public float waitSeconds = 3f;
    public bool isPlayBarrier;

    Slider slider;
    private bool isMax;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        slider = GetComponent<Slider>();
        ResetParameter();
    }

    public void Skill()
    {
        if (!isMax || StageMoveSystem.instance.isScreenMove) return;
        StartCoroutine(LuteinBarrier());
        ResetParameter();
    }

    IEnumerator LuteinBarrier()
    {
        isPlayBarrier = true;
        luteinBarrierCircle.SetActive(true);
        player.layer = 10;
        yield return new WaitForSeconds(5f);

        isPlayBarrier = false;
        luteinBarrierCircle.SetActive(false);
        player.layer = 6;
    }

    IEnumerator AnimateSliderToMax(float duration)
    {
        float startValue = slider.value;
        float endValue = slider.maxValue;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // ■ 画面移動中は待機する（スライダー更新しない）
            while (StageMoveSystem.instance.isScreenMove || ScenesManagers.instance.isPause)
            {
                yield return null;
            }

            // ■ 通常の経過時間処理
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, endValue, elapsed / duration);

            yield return null;
        }

        slider.value = endValue;
        isMax = true;
    }

    void ResetParameter()
    {
        isMax = false;
        slider.value = 0;
        StartCoroutine(AnimateSliderToMax(waitSeconds));
    }

    public void AllResetParameter()
    {
        isPlayBarrier = false;
        luteinBarrierCircle.SetActive(false);
        player.layer = 6;
    }
}
