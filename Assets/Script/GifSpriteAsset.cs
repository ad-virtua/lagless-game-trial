using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Sprites/GIF Sprite Asset")]
public class GifSpriteAsset : ScriptableObject
{
    [SerializeField] private Sprite[] frames = Array.Empty<Sprite>();
    [SerializeField] private float[] frameDurations = Array.Empty<float>();
    [SerializeField] private int width;
    [SerializeField] private int height;

    public int Width => width;
    public int Height => height;
    public int FrameCount => frames.Length;
    public Sprite[] Frames => frames;
    public float[] FrameDurations => frameDurations;

    public void Initialize(Sprite[] newFrames, float[] newDurations, int newWidth, int newHeight)
    {
        frames = newFrames ?? Array.Empty<Sprite>();
        frameDurations = newDurations ?? Array.Empty<float>();
        width = newWidth;
        height = newHeight;
    }

    public Sprite GetFrame(int index)
    {
        if (frames == null || frames.Length == 0) return null;
        if (index < 0) index = 0;
        if (index >= frames.Length) index = frames.Length - 1;
        return frames[index];
    }

    public float GetDuration(int index, float fallbackSeconds)
    {
        if (frameDurations == null || frameDurations.Length == 0) return fallbackSeconds;
        if (index < 0) index = 0;
        if (index >= frameDurations.Length) index = frameDurations.Length - 1;
        var duration = frameDurations[index];
        return duration > 0f ? duration : fallbackSeconds;
    }
}

