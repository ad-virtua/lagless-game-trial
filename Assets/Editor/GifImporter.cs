using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

[ScriptedImporter(1, "gifbytes")]
public class GifImporter : ScriptedImporter
{
    [SerializeField, Min(1f)] private float pixelsPerUnit = 100f;
    [SerializeField] private bool createAnimationClip = true;
    [SerializeField] private bool loopAnimation = true;
    [SerializeField] private FilterMode filterMode = FilterMode.Point;
    [SerializeField] private bool useBackgroundColor;
    [SerializeField] private bool flipVertical = true;
    [SerializeField] private bool hideTextureAssets = true;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        GifAnimation animation;
        try
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            animation = GifDecoder.Decode(bytes, useBackgroundColor);
        }
        catch (Exception ex)
        {
            ctx.LogImportError($"GIF decode failed: {ex.Message}");
            return;
        }

        if (animation == null || animation.Frames.Count == 0)
        {
            ctx.LogImportError("GIF has no frames.");
            return;
        }

        var sprites = new Sprite[animation.Frames.Count];
        var durations = new float[animation.Frames.Count];
        var assetName = Path.GetFileNameWithoutExtension(ctx.assetPath);
        var ppu = Mathf.Max(1f, pixelsPerUnit);

        for (var i = 0; i < animation.Frames.Count; i++)
        {
            var frame = animation.Frames[i];
            var texture = new Texture2D(animation.Width, animation.Height, TextureFormat.RGBA32, false);
            texture.name = $"{assetName}_tex_{i:000}";
            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Clamp;
            ApplyTextureHideFlags(texture, hideTextureAssets);
            var pixels = flipVertical
                ? FlipVertical(frame.Pixels, animation.Width, animation.Height)
                : frame.Pixels;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            ctx.AddObjectToAsset(texture.name, texture);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, animation.Width, animation.Height),
                new Vector2(0.5f, 0.5f),
                ppu,
                0,
                SpriteMeshType.FullRect);
            sprite.name = $"{assetName}_frame_{i:000}";
            ctx.AddObjectToAsset(sprite.name, sprite);
            sprites[i] = sprite;
            durations[i] = frame.DelaySeconds;
        }

        var gifAsset = ScriptableObject.CreateInstance<GifSpriteAsset>();
        gifAsset.name = assetName;
        gifAsset.Initialize(sprites, durations, animation.Width, animation.Height);
        ctx.AddObjectToAsset("gif", gifAsset);
        ctx.SetMainObject(gifAsset);

        if (createAnimationClip)
        {
            var clip = CreateAnimationClip(assetName, sprites, durations);
            ctx.AddObjectToAsset($"{assetName}_anim", clip);
        }
    }

    private AnimationClip CreateAnimationClip(string assetName, Sprite[] sprites, float[] durations)
    {
        var clip = new AnimationClip
        {
            name = $"{assetName}_anim",
            frameRate = 60f
        };

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        var time = 0f;
        for (var i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe { time = time, value = sprites[i] };
            var duration = i < durations.Length ? durations[i] : 0f;
            if (duration <= 0f) duration = 0.1f;
            time += duration;
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        SetClipLoop(clip, loopAnimation);
        return clip;
    }

    private static void SetClipLoop(AnimationClip clip, bool loop)
    {
        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Default;
        var serializedClip = new SerializedObject(clip);
        var settings = serializedClip.FindProperty("m_AnimationClipSettings");
        if (settings != null)
        {
            settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
            serializedClip.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ApplyTextureHideFlags(Texture2D texture, bool hide)
    {
        if (texture == null) return;
        texture.hideFlags = hide
            ? HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable
            : HideFlags.None;
    }

    private static Color32[] FlipVertical(Color32[] pixels, int width, int height)
    {
        if (pixels == null || pixels.Length == 0) return pixels;
        if (width <= 0 || height <= 0) return pixels;
        if (pixels.Length < width * height) return pixels;

        var flipped = new Color32[pixels.Length];
        for (var y = 0; y < height; y++)
        {
            var srcRow = y * width;
            var dstRow = (height - 1 - y) * width;
            Array.Copy(pixels, srcRow, flipped, dstRow, width);
        }
        return flipped;
    }

    private sealed class GifAnimation
    {
        public int Width;
        public int Height;
        public readonly List<GifFrame> Frames = new List<GifFrame>();
    }

    private sealed class GifFrame
    {
        public Color32[] Pixels;
        public float DelaySeconds;
    }

    private struct GifGraphicControl
    {
        public int DisposalMethod;
        public bool Transparency;
        public byte TransparentIndex;
        public float DelaySeconds;

        public static GifGraphicControl Default => new GifGraphicControl
        {
            DisposalMethod = 0,
            Transparency = false,
            TransparentIndex = 0,
            DelaySeconds = 0f
        };
    }

    private static class GifDecoder
    {
        public static GifAnimation Decode(byte[] data, bool useBackgroundColor)
        {
            var reader = new GifReader(data);
            var header = reader.ReadString(6);
            if (header != "GIF87a" && header != "GIF89a")
            {
                throw new InvalidDataException("Not a GIF file.");
            }

            var width = reader.ReadUInt16();
            var height = reader.ReadUInt16();
            var packed = reader.ReadByte();
            var hasGlobalColorTable = (packed & 0x80) != 0;
            var globalColorTableSize = 2 << (packed & 0x07);
            var backgroundColorIndex = reader.ReadByte();
            reader.ReadByte(); // Pixel aspect ratio

            Color32[] globalColorTable = null;
            if (hasGlobalColorTable)
            {
                globalColorTable = ReadColorTable(reader, globalColorTableSize);
            }

            var backgroundColor = new Color32(0, 0, 0, 0);
            if (useBackgroundColor && globalColorTable != null && backgroundColorIndex < globalColorTable.Length)
            {
                backgroundColor = globalColorTable[backgroundColorIndex];
            }

            var animation = new GifAnimation { Width = width, Height = height };
            var canvas = new Color32[width * height];
            Fill(canvas, backgroundColor);

            var graphicControl = GifGraphicControl.Default;

            while (reader.CanRead)
            {
                var introducer = reader.ReadByte();
                if (introducer == 0x3B)
                {
                    break;
                }

                if (introducer == 0x21)
                {
                    var label = reader.ReadByte();
                    if (label == 0xF9)
                    {
                        graphicControl = ReadGraphicControlExtension(reader);
                    }
                    else if (label == 0xFF)
                    {
                        ReadApplicationExtension(reader);
                    }
                    else
                    {
                        reader.SkipSubBlocks();
                    }
                    continue;
                }

                if (introducer == 0x2C)
                {
                    var frame = ReadImageDescriptor(reader, width, height, globalColorTable, backgroundColor, canvas, graphicControl);
                    if (frame != null)
                    {
                        animation.Frames.Add(frame);
                    }
                    graphicControl = GifGraphicControl.Default;
                    continue;
                }

                break;
            }

            return animation;
        }

        private static GifGraphicControl ReadGraphicControlExtension(GifReader reader)
        {
            var blockSize = reader.ReadByte();
            var packed = reader.ReadByte();
            var delay = reader.ReadUInt16();
            var transparentIndex = reader.ReadByte();
            if (blockSize > 4)
            {
                reader.SkipBytes(blockSize - 4);
            }
            reader.ReadByte(); // Block terminator

            var disposalMethod = (packed >> 2) & 0x07;
            var transparency = (packed & 0x01) != 0;

            return new GifGraphicControl
            {
                DisposalMethod = disposalMethod,
                Transparency = transparency,
                TransparentIndex = transparentIndex,
                DelaySeconds = delay / 100f
            };
        }

        private static void ReadApplicationExtension(GifReader reader)
        {
            var blockSize = reader.ReadByte();
            reader.SkipBytes(blockSize);
            reader.SkipSubBlocks();
        }

        private static GifFrame ReadImageDescriptor(
            GifReader reader,
            int canvasWidth,
            int canvasHeight,
            Color32[] globalColorTable,
            Color32 backgroundColor,
            Color32[] canvas,
            GifGraphicControl graphicControl)
        {
            var left = reader.ReadUInt16();
            var top = reader.ReadUInt16();
            var width = reader.ReadUInt16();
            var height = reader.ReadUInt16();
            var packed = reader.ReadByte();
            var hasLocalColorTable = (packed & 0x80) != 0;
            var interlaced = (packed & 0x40) != 0;
            var localColorTableSize = 2 << (packed & 0x07);

            var colorTable = hasLocalColorTable
                ? ReadColorTable(reader, localColorTableSize)
                : globalColorTable;

            if (colorTable == null)
            {
                throw new InvalidDataException("Missing color table.");
            }

            var lzwMinimumCodeSize = reader.ReadByte();
            var imageData = reader.ReadSubBlocks();
            var indices = LzwDecode(imageData, lzwMinimumCodeSize, width * height);

            Color32[] previousCanvas = null;
            if (graphicControl.DisposalMethod == 3)
            {
                previousCanvas = (Color32[])canvas.Clone();
            }

            BlitIndexedImage(
                canvas,
                canvasWidth,
                canvasHeight,
                indices,
                left,
                top,
                width,
                height,
                colorTable,
                interlaced,
                graphicControl.Transparency,
                graphicControl.TransparentIndex);

            var framePixels = (Color32[])canvas.Clone();

            if (graphicControl.DisposalMethod == 2)
            {
                ClearRect(canvas, canvasWidth, canvasHeight, left, top, width, height, backgroundColor);
            }
            else if (graphicControl.DisposalMethod == 3 && previousCanvas != null)
            {
                Array.Copy(previousCanvas, canvas, previousCanvas.Length);
            }

            return new GifFrame
            {
                Pixels = framePixels,
                DelaySeconds = graphicControl.DelaySeconds
            };
        }

        private static void BlitIndexedImage(
            Color32[] canvas,
            int canvasWidth,
            int canvasHeight,
            byte[] indices,
            int left,
            int top,
            int width,
            int height,
            Color32[] colorTable,
            bool interlaced,
            bool transparency,
            byte transparentIndex)
        {
            var srcIndex = 0;

            if (!interlaced)
            {
                for (var y = 0; y < height; y++)
                {
                    var destY = top + y;
                    if (destY < 0 || destY >= canvasHeight)
                    {
                        srcIndex += width;
                        continue;
                    }

                    var rowStart = destY * canvasWidth;
                    for (var x = 0; x < width; x++)
                    {
                        if (srcIndex >= indices.Length)
                        {
                            srcIndex++;
                            continue;
                        }

                        var destX = left + x;
                        var colorIndex = indices[srcIndex++];
                        if (transparency && colorIndex == transparentIndex) continue;
                        if (destX < 0 || destX >= canvasWidth) continue;
                        if (colorIndex >= colorTable.Length) continue;
                        canvas[rowStart + destX] = colorTable[colorIndex];
                    }
                }

                return;
            }

            var passStarts = new[] { 0, 4, 2, 1 };
            var passSteps = new[] { 8, 8, 4, 2 };

            for (var pass = 0; pass < 4; pass++)
            {
                for (var y = passStarts[pass]; y < height; y += passSteps[pass])
                {
                    var destY = top + y;
                    if (destY < 0 || destY >= canvasHeight)
                    {
                        srcIndex += width;
                        continue;
                    }

                    var rowStart = destY * canvasWidth;
                    for (var x = 0; x < width; x++)
                    {
                        if (srcIndex >= indices.Length)
                        {
                            srcIndex++;
                            continue;
                        }

                        var destX = left + x;
                        var colorIndex = indices[srcIndex++];
                        if (transparency && colorIndex == transparentIndex) continue;
                        if (destX < 0 || destX >= canvasWidth) continue;
                        if (colorIndex >= colorTable.Length) continue;
                        canvas[rowStart + destX] = colorTable[colorIndex];
                    }
                }
            }
        }

        private static void ClearRect(
            Color32[] canvas,
            int canvasWidth,
            int canvasHeight,
            int left,
            int top,
            int width,
            int height,
            Color32 clearColor)
        {
            for (var y = 0; y < height; y++)
            {
                var destY = top + y;
                if (destY < 0 || destY >= canvasHeight) continue;
                var rowStart = destY * canvasWidth;
                for (var x = 0; x < width; x++)
                {
                    var destX = left + x;
                    if (destX < 0 || destX >= canvasWidth) continue;
                    canvas[rowStart + destX] = clearColor;
                }
            }
        }

        private static void Fill(Color32[] canvas, Color32 color)
        {
            for (var i = 0; i < canvas.Length; i++)
            {
                canvas[i] = color;
            }
        }

        private static Color32[] ReadColorTable(GifReader reader, int size)
        {
            var table = new Color32[size];
            for (var i = 0; i < size; i++)
            {
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                table[i] = new Color32(r, g, b, 255);
            }
            return table;
        }

        private static byte[] LzwDecode(byte[] data, int minCodeSize, int expectedSize)
        {
            if (minCodeSize < 2) minCodeSize = 2;

            var clearCode = 1 << minCodeSize;
            var endCode = clearCode + 1;
            var codeSize = minCodeSize + 1;

            var dictionary = new List<byte[]>(4096);
            InitDictionary(dictionary, clearCode);

            var output = new List<byte>(Mathf.Max(expectedSize, 0));
            var reader = new GifBitReader(data);
            var previousCode = -1;

            while (true)
            {
                var code = reader.ReadBits(codeSize);
                if (code < 0) break;
                if (code == clearCode)
                {
                    InitDictionary(dictionary, clearCode);
                    codeSize = minCodeSize + 1;
                    previousCode = -1;
                    continue;
                }
                if (code == endCode)
                {
                    break;
                }

                byte[] entry;
                if (code < dictionary.Count && dictionary[code] != null)
                {
                    entry = dictionary[code];
                }
                else if (code == dictionary.Count && previousCode >= 0)
                {
                    var previousEntry = dictionary[previousCode];
                    entry = Concat(previousEntry, previousEntry[0]);
                }
                else
                {
                    break;
                }

                output.AddRange(entry);

                if (previousCode >= 0 && dictionary.Count < 4096)
                {
                    var previousEntry = dictionary[previousCode];
                    dictionary.Add(Concat(previousEntry, entry[0]));
                    if (dictionary.Count == (1 << codeSize) && codeSize < 12)
                    {
                        codeSize++;
                    }
                }

                previousCode = code;

                if (expectedSize > 0 && output.Count >= expectedSize)
                {
                    break;
                }
            }

            return output.ToArray();
        }

        private static void InitDictionary(List<byte[]> dictionary, int clearCode)
        {
            dictionary.Clear();
            for (var i = 0; i < clearCode; i++)
            {
                dictionary.Add(new[] { (byte)i });
            }
            dictionary.Add(null); // Clear code placeholder
            dictionary.Add(null); // End code placeholder
        }

        private static byte[] Concat(byte[] prefix, byte suffix)
        {
            var result = new byte[prefix.Length + 1];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            result[result.Length - 1] = suffix;
            return result;
        }
    }

    private sealed class GifReader
    {
        private readonly byte[] data;
        private int index;

        public GifReader(byte[] data)
        {
            this.data = data ?? Array.Empty<byte>();
            index = 0;
        }

        public bool CanRead => index < data.Length;

        public byte ReadByte()
        {
            if (index >= data.Length)
            {
                throw new EndOfStreamException("Unexpected end of GIF data.");
            }
            return data[index++];
        }

        public ushort ReadUInt16()
        {
            var lo = ReadByte();
            var hi = ReadByte();
            return (ushort)(lo | (hi << 8));
        }

        public string ReadString(int length)
        {
            if (length <= 0) return string.Empty;
            if (index + length > data.Length)
            {
                throw new EndOfStreamException("Unexpected end of GIF data.");
            }
            var text = System.Text.Encoding.ASCII.GetString(data, index, length);
            index += length;
            return text;
        }

        public void SkipBytes(int count)
        {
            if (count <= 0) return;
            index = Math.Min(index + count, data.Length);
        }

        public byte[] ReadSubBlocks()
        {
            var bytes = new List<byte>();
            while (true)
            {
                var size = (int)ReadByte();
                if (size == 0) break;
                if (index + size > data.Length)
                {
                    size = data.Length - index;
                }
                if (size > 0)
                {
                    for (var i = 0; i < size; i++)
                    {
                        bytes.Add(data[index + i]);
                    }
                    index += size;
                }
            }
            return bytes.ToArray();
        }

        public void SkipSubBlocks()
        {
            while (true)
            {
                var size = ReadByte();
                if (size == 0) break;
                SkipBytes(size);
            }
        }
    }

    private sealed class GifBitReader
    {
        private readonly byte[] data;
        private int byteIndex;
        private int bitIndex;

        public GifBitReader(byte[] data)
        {
            this.data = data ?? Array.Empty<byte>();
        }

        public int ReadBits(int count)
        {
            var value = 0;
            var bitsRead = 0;

            while (bitsRead < count)
            {
                if (byteIndex >= data.Length)
                {
                    return -1;
                }

                var available = 8 - bitIndex;
                var toRead = Math.Min(count - bitsRead, available);
                var mask = (1 << toRead) - 1;
                var bits = (data[byteIndex] >> bitIndex) & mask;
                value |= bits << bitsRead;
                bitsRead += toRead;
                bitIndex += toRead;

                if (bitIndex >= 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return value;
        }
    }
}

