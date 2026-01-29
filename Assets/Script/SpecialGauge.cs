using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialGauge : MonoBehaviour
{
    public static SpecialGauge instance;

    public float waitSeconds = 3f;
    public GameObject flash;
    public int flashDamage;

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


    public void Skill()
    {
        if (!isMax || StageMoveSystem.instance.isScreenMove) return;

        StartCoroutine(Flash());
        ResetParameter();
    }

    IEnumerator Flash()
    {
        flash.SetActive(false);
        flash.SetActive(true);
        yield return new WaitForSeconds(2f);
        flash.SetActive(false);

        var enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemys)
        {
            if (enemy.GetComponent<ScreenRangeChecker>() &&
                enemy.GetComponent<ScreenRangeChecker>().IsInScreen())
            {
                StartCoroutine(EnemyManager.instance.Damage(enemy, flashDamage));
            }
        }
    }

    void ResetParameter()
    {
        isMax = false;
        slider.value = 0;
        StartCoroutine(AnimateSliderToMax(waitSeconds));
    }
}
