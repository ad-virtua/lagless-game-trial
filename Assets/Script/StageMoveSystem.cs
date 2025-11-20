using System.Collections;
using UnityEngine;

public class StageMoveSystem : MonoBehaviour
{
    public static StageMoveSystem instance;

    [SerializeField] private GameObject player, mouth, inMouth;
    [SerializeField] private GameObject tutorialStarted, tutorialLatter;
    [SerializeField] private float directionSpeed;

    private ScreenRangeChecker playerScreenRangeChecker;
    [HideInInspector] public bool isPlayerScreenMove;

    private Vector3 stagePosCount;
    private float margin = 1f;
    private Vector2 marginDistance;

    [HideInInspector] public bool isScreenMove;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StageManager.instance.stageAreaCount = 1;
        stagePosCount = new Vector3(0, 0, 0);
        marginDistance = Vector2.zero;
        playerScreenRangeChecker = player.GetComponent<ScreenRangeChecker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScreenRangeChecker && !isPlayerScreenMove && marginDistance == Vector2.zero)
        {
            // ここで取得と同時にリセット
            var cameraWasDirection = playerScreenRangeChecker.GetCameraWasDirection();

            if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Right)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += -18f;
                StageManager.instance.stageAreaCount++;
               

                if (SceneManager.instance.sceneType == SceneManager.SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount == 4 && !tutorialLatter.activeSelf)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(StageChangeBigupBlackout(mouth, inMouth));
                    }
                    else if (StageManager.instance.stageAreaCount < 4)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.BigupObj(mouth, 0.2f, 1.5f));
                    }
                    if (StageManager.instance.stageAreaCount == 8)
                    {
                        StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y + 3f, transform.position.z)));
                    }
                    else if (StageManager.instance.stageAreaCount == 9)
                    {
                        StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + 7f, transform.position.y + 9f, transform.position.z)));
                    }
                    else StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));
                }
                else StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Left || cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Up)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += 18f;
                StageManager.instance.stageAreaCount--;

                if (SceneManager.instance.sceneType == SceneManager.SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount < 4)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.SmallupObj(mouth, 0.2f, 1.5f));
                    }
                    if (StageManager.instance.stageAreaCount == 7)
                    {
                        StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y - 3f, transform.position.z)));
                    }
                    else if (StageManager.instance.stageAreaCount == 8)
                    {
                        StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y - 9f, transform.position.z)));
                    }
                    else StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));
                }
                else StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));
            }
        }

        if (marginDistance != Vector2.zero &&
            Vector2.Distance(marginDistance, new Vector2(player.transform.position.x, player.transform.position.z)) > margin)
        {
            marginDistance = Vector2.zero;
            playerScreenRangeChecker.isStop = false;
        }
    }

    public void ResetMove()
    {
        if (GameSystemOwner.isClear)
        {
            inMouth.SetActive(false);
            transform.position = Vector3.zero;
            stagePosCount = new Vector3(0, 0, 0);
            marginDistance = Vector2.zero;
        }
    }

    IEnumerator ScreenMove(Vector3 targetPos)
    {
        isScreenMove = true;
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, directionSpeed * Time.deltaTime);
            yield return null; // 次のフレームまで待つ
        }
        isScreenMove = false;

        if (StageManager.instance.stageAreaCount == 4 && !tutorialLatter.activeSelf)
        {
            tutorialStarted.SetActive(false);
            tutorialLatter.SetActive(true);
        }

        if (StageManager.instance.SceneEndAreaChecker(SceneManager.instance.sceneType))
        {
            GameSystemOwner.isClear = true;
            yield break;
        }

        isPlayerScreenMove = false;
        marginDistance = new Vector2(player.transform.position.x, player.transform.position.z);
    }

    IEnumerator StageChangeBigupBlackout(GameObject beforeObj, GameObject afterObj)
    {
        yield return StartCoroutine(Generic.BigupObj(beforeObj, 3.0f, 1f));
        yield return StartCoroutine(Generic.BlackOut(beforeObj.GetComponent<SpriteRenderer>(), 0.02f));
        beforeObj.SetActive(false);
        afterObj.SetActive(true);
    }
}
