using System.Collections;
using UnityEngine;
using static ScenesManagers;

public class ChsngeBg : MonoBehaviour
{
    [SerializeField] private SpriteRenderer beforeBg;
    [SerializeField] private SpriteRenderer afterBg;
    [SerializeField, Min(0.01f)] private float fadeDuration = 1f;
    [SerializeField, Range(0f, 1f)] private float beforeTargetAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float afterTargetAlpha = 1f;

    [SerializeField] private Transform stageTransform;
    [SerializeField] private Vector3 targetSceneArea;
    bool isChange;

    private Coroutine changeCoroutine;

    // Update is called once per frame
    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove)
        {
            if (targetSceneArea == stageTransform.localPosition)
            {
                if (!isChange)
                {
                    isChange = true;
                    ChangeBgStart();
                }
            }
        }
    }

    public void ChangeBgStart()
    {
        if (beforeBg == null || afterBg == null)
        {
            Debug.LogWarning("ChsngeBg: beforeBg or afterBg is not assigned.", this);
            return;
        }

        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
        }

        changeCoroutine = StartCoroutine(ChangeBg());
    }

    private IEnumerator ChangeBg()
    {
        var beforeColor = beforeBg.color;
        var afterColor = afterBg.color;
        var beforeStartAlpha = beforeColor.a;
        var afterStartAlpha = afterColor.a;

        var elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            var t = elapsed / fadeDuration;
            var beforeAlpha = Mathf.Lerp(beforeStartAlpha, beforeTargetAlpha, t);
            var afterAlpha = Mathf.Lerp(afterStartAlpha, afterTargetAlpha, t);

            beforeBg.color = new Color(beforeColor.r, beforeColor.g, beforeColor.b, beforeAlpha);
            afterBg.color = new Color(afterColor.r, afterColor.g, afterColor.b, afterAlpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        beforeBg.color = new Color(beforeColor.r, beforeColor.g, beforeColor.b, beforeTargetAlpha);
        afterBg.color = new Color(afterColor.r, afterColor.g, afterColor.b, afterTargetAlpha);

        changeCoroutine = null;
        isChange = false;
    }
}