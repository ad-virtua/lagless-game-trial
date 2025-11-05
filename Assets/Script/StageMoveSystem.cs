using System.Collections;
using UnityEngine;

public class StageMoveSystem : MonoBehaviour
{
    public static StageMoveSystem instance;

    [SerializeField] private GameObject player, mouth, inMouth;
    [SerializeField] private GameObject tutorialStarted, tutorialLatter;
    [SerializeField] private float directionSpeed;

    private ScreenRangeChecker playerScreenRangeChecker;
    [HideInInspector] public bool isScreenMove;

    private Vector3 stagePosCount;
    private float margin = 1f;
    private Vector2 marginDistance;

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
        if (playerScreenRangeChecker && !isScreenMove && marginDistance == Vector2.zero)
        {
            // ここで取得と同時にリセット
            var cameraWasDirection = playerScreenRangeChecker.GetCameraWasDirection();

            if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Right)
            {
                isScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += -18f;
                StageManager.instance.stageAreaCount++;
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));

                if (SceneManager.instance.sceneType == SceneManager.SceneType.Tutorial)
                {
                    if (StageManager.instance.stageAreaCount == 4 && !tutorialLatter.activeSelf)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(StageChangeBigupBlackout(mouth, inMouth));
                    }
                    else if (StageManager.instance.stageAreaCount < 4)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.BigupObj(mouth, 0.2f, 2.5f));
                    }
                }
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Left)
            {
                isScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += 18f;
                StageManager.instance.stageAreaCount--;
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));

                if (SceneManager.instance.sceneType == SceneManager.SceneType.Tutorial)
                {
                    if (StageManager.instance.stageAreaCount < 4)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.SmallupObj(mouth, 0.2f, 2.5f));
                    }
                }
            }
        }

        if (marginDistance != Vector2.zero &&
            Vector2.Distance(marginDistance, new Vector2(player.transform.position.x, player.transform.position.z)) > margin)
        {
            marginDistance = Vector2.zero;
            playerScreenRangeChecker.isStop = false;
        }
    }


    IEnumerator ScreenMove(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, directionSpeed * Time.deltaTime);
            yield return null; // 次のフレームまで待つ
        }

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

        isScreenMove = false;
        marginDistance = new Vector2(player.transform.position.x, player.transform.position.z);
    }

    IEnumerator StageChangeBigupBlackout(GameObject beforeObj, GameObject afterObj)
    {
        yield return StartCoroutine(Generic.BigupObj(beforeObj, 3.0f, 1.75f));
        yield return StartCoroutine(Generic.BlackOut(beforeObj.GetComponent<SpriteRenderer>(), 0.02f));
        beforeObj.SetActive(false);
        afterObj.SetActive(true);
    }
}
