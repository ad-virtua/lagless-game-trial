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
    private bool isAfterActive;
    private bool isReverting;
    private Color beforeInitialColor;
    private Color afterInitialColor;
    private bool hasInitialColors;

    private Coroutine changeCoroutine;

    private void Start()
    {
        CacheInitialColors();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.instance != null && Player.instance.isPortal && (isChange || isAfterActive))
        {
            if (!isReverting)
            {
                StartResetFade();
            }
            return;
        }

        if (StageMoveSystem.instance.isScreenMove)
        {
            if (targetSceneArea == stageTransform.localPosition)
            {
                if (!isChange && !isAfterActive)
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

        if (!hasInitialColors)
        {
            CacheInitialColors();
        }

        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
        }

        isReverting = false;
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
        isAfterActive = true;
    }

    private void CacheInitialColors()
    {
        if (beforeBg == null || afterBg == null) return;

        beforeInitialColor = beforeBg.color;
        afterInitialColor = afterBg.color;
        hasInitialColors = true;
    }

    private void StartResetFade()
    {
        if (beforeBg == null || afterBg == null)
        {
            isChange = false;
            isAfterActive = false;
            changeCoroutine = null;
            return;
        }

        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
            changeCoroutine = null;
        }

        if (!hasInitialColors)
        {
            CacheInitialColors();
            if (!hasInitialColors) return;
        }

        isChange = true;
        isReverting = true;
        changeCoroutine = StartCoroutine(ResetBg());
    }

    private IEnumerator ResetBg()
    {
        var beforeColor = beforeBg.color;
        var afterColor = afterBg.color;
        var beforeStartAlpha = beforeColor.a;
        var afterStartAlpha = afterColor.a;
        var beforeTargetAlpha = beforeInitialColor.a;
        var afterTargetAlpha = afterInitialColor.a;

        var elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            var t = elapsed / fadeDuration;
            var beforeAlpha = Mathf.Lerp(beforeStartAlpha, beforeTargetAlpha, t);
            var afterAlpha = Mathf.Lerp(afterStartAlpha, afterTargetAlpha, t);

            beforeBg.color = new Color(beforeInitialColor.r, beforeInitialColor.g, beforeInitialColor.b, beforeAlpha);
            afterBg.color = new Color(afterInitialColor.r, afterInitialColor.g, afterInitialColor.b, afterAlpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        beforeBg.color = new Color(beforeInitialColor.r, beforeInitialColor.g, beforeInitialColor.b, beforeTargetAlpha);
        afterBg.color = new Color(afterInitialColor.r, afterInitialColor.g, afterInitialColor.b, afterTargetAlpha);
        changeCoroutine = null;
        isChange = false;
        isAfterActive = false;
        isReverting = false;
    }
}