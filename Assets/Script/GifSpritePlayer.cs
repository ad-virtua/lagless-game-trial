using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GifSpritePlayer : MonoBehaviour
{
    [SerializeField] private GifSpriteAsset gif;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool useUnscaledTime;
    [SerializeField, Min(0.01f)] private float fallbackFrameDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private bool isPlaying;
    private int frameIndex;
    private float frameTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playOnAwake)
        {
            Play();
        }
        else
        {
            ApplyFrame();
        }
    }

    private void Update()
    {
        if (!isPlaying || gif == null || gif.FrameCount == 0) return;

        var delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        frameTimer += delta;

        var duration = gif.GetDuration(frameIndex, fallbackFrameDuration);
        if (duration <= 0f) duration = fallbackFrameDuration;

        while (frameTimer >= duration)
        {
            frameTimer -= duration;
            frameIndex++;
            if (frameIndex >= gif.FrameCount)
            {
                if (loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    frameIndex = gif.FrameCount - 1;
                    isPlaying = false;
                    ApplyFrame();
                    return;
                }
            }

            duration = gif.GetDuration(frameIndex, fallbackFrameDuration);
            if (duration <= 0f) duration = fallbackFrameDuration;
            ApplyFrame();
        }
    }

    public void Play()
    {
        if (gif == null || gif.FrameCount == 0)
        {
            isPlaying = false;
            return;
        }

        isPlaying = true;
        frameIndex = 0;
        frameTimer = 0f;
        ApplyFrame();
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetGif(GifSpriteAsset newGif, bool restart = true)
    {
        gif = newGif;
        if (restart)
        {
            Play();
        }
        else
        {
            ApplyFrame();
        }
    }

    private void ApplyFrame()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (gif == null || gif.FrameCount == 0)
        {
            spriteRenderer.sprite = null;
            return;
        }

        spriteRenderer.sprite = gif.GetFrame(frameIndex);
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (!Application.isPlaying)
        {
            frameIndex = 0;
            ApplyFrame();
        }
    }
}


