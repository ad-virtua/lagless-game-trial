using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// MovieTextGeneration
/// テキストを1文字ずつ表示する（ノベルゲー等で使う）スクリプト。
/// レガシー Text コンポーネントを使用します。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MovieTextGeneration : MonoBehaviour
{
    [Header("References")]
    public Text textComponent; // 表示先
    public List<string> textList;

    public GameObject fadeOut;

    [Header("Typing settings")]
    public float charInterval = 0.03f;
    public bool playSoundForWhitespace = false;

    [Header("Sound")]
    public bool useSound = true; // ★ 音声の有無を切り替えるフラグ
    public AudioClip typeSound;
    [Range(0f, 1f)]
    public float typeSoundVolume = 0.6f;

    [Header("Events")]
    public UnityEvent onComplete;

    AudioSource _audioSource;
    Coroutine _typingCoroutine;
    string _fullText = "";
    int textListCount;

    public bool IsPlaying { get; private set; } = false;

    void Awake()
    {
        if (textComponent == null)
        {
            Debug.LogError("MovieTextGeneration: Text が割り当てられていません。");
        }
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        textListCount = 0;
        StartCoroutine(StartTyping(textList[textListCount], 1f));
    }

    void Update()
    {
        if (fadeOut.activeSelf) return;

        //★ スペースとクリックでスキップ
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (IsPlaying)
            {
                Skip();
            }
            else
            {
                textListCount++;
                if (textListCount == textList.Count)
                {
                    StartCoroutine(EndProcess(1f));
                }
                else StartCoroutine(StartTyping(textList[textListCount]));
            }
        }
    }

    public IEnumerator StartTyping(string text, float intervalTime = 0)
    {
        yield return new WaitForSeconds(intervalTime);

        if (textComponent == null) yield break;
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

        _fullText = text ?? "";
        _typingCoroutine = StartCoroutine(TypeTextCoroutine(_fullText));
    }

    public void StopTyping()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = null;
        IsPlaying = false;
    }

    public void Skip()
    {
        if (textComponent == null) return;
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

        _fullText = _fullText.Replace("(改行)", "\n");
        textComponent.text = _fullText;
        _typingCoroutine = null;
        IsPlaying = false;
    }

    IEnumerator TypeTextCoroutine(string text)
    {
        // ★ "\n" を実際の改行に変換
        text = text.Replace("(改行)", "\n");

        IsPlaying = true;
        textComponent.text = string.Empty;

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int tagEnd = text.IndexOf('>', i);
                if (tagEnd == -1)
                {
                    textComponent.text += text.Substring(i);
                    break;
                }
                int len = tagEnd - i + 1;
                textComponent.text += text.Substring(i, len);
                i += len;
                continue;
            }

            char c = text[i];
            textComponent.text += c;

            // ★ useSound フラグ追加
            if (useSound && typeSound != null && (playSoundForWhitespace || !char.IsWhiteSpace(c)))
            {
                _audioSource.PlayOneShot(typeSound, typeSoundVolume);
            }

            i++;

            yield return new WaitForSeconds(charInterval);
        }

        IsPlaying = false;
        _typingCoroutine = null;
    }

    public void ForceSetText(string text)
    {
        if (textComponent == null) return;
        StopTyping();
        _fullText = text ?? "";
        textComponent.text = _fullText;
    }

    public IEnumerator EndProcess(float intervalTime = 0)
    {
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(intervalTime);

        fadeOut.SetActive(false);
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}