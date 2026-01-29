using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    [SerializeField] private Text timerUI;
    [SerializeField] private GameObject player;

    public float time = 10f;   // カウントダウン秒数
    private float startTime;
    private bool isRunning;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startTime = time;
        StartTimer();
    }

    void Update()
    {
        if (GameSystemOwner.isGameOver || ScenesManagers.instance.isPause) return;
        if (GameSystemOwner.isClear || GameSystemOwner.instance.IsPlayMovie()) return;
        if (StageMoveSystem.instance && StageMoveSystem.instance.isScreenMove) return;

        if (!isRunning) return;

        time -= Time.deltaTime;
        timerUI.text = "Timer:" + time.ToString("F0");

        if (time <= 0f)
        {
            time = 0f;
            isRunning = false;
            OnTimeUp();
        }
    }

    void StartTimer()
    {
        isRunning = true;
    }

    void OnTimeUp()
    {
        Debug.Log("時間切れ！");
        GameSystemOwner.isGameOver = true;
        player.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void ResetTime()
    {
        time = startTime;
        StartTimer();
    }
}
