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
    private Vector2 specialPosCount;
    private float margin = 1f;
    private Vector2 marginDistanceX, marginDistanceY;

    [HideInInspector] public bool isScreenMove;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StageManager.instance.stageAreaCount = 1;
        stagePosCount = new Vector3(0, 0, 0);
        specialPosCount = new Vector3(0, 0, 0);
        marginDistanceX = marginDistanceY = Vector2.zero;
        playerScreenRangeChecker = player.GetComponent<ScreenRangeChecker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScreenRangeChecker && !isPlayerScreenMove && (marginDistanceX == Vector2.zero || marginDistanceY == Vector2.zero))
        {
            // ここで取得と同時にリセット
            var cameraWasDirection = playerScreenRangeChecker.GetCameraWasDirection();

            if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Right)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += -18f;
                StageManager.instance.stageAreaCount++;  

                if (ScenesManagers.sceneType == ScenesManagers.SceneType.Stage1)
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
                        specialPosCount.y += 3f;
                    }
                    else if (StageManager.instance.stageAreaCount == 9)
                    {
                        specialPosCount.x += 7f;
                        specialPosCount.y += 9f;
                    }
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Left)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += 18f;
                StageManager.instance.stageAreaCount--;

                if (ScenesManagers.sceneType == ScenesManagers.SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount < 4)
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.SmallupObj(mouth, 0.2f, 1.5f));
                    }
                    if (StageManager.instance.stageAreaCount == 7)
                    {
                        specialPosCount.y -= 3f;
                    }
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Down)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                StageManager.instance.stageAreaCount++;

                if (ScenesManagers.sceneType == ScenesManagers.SceneType.Stage2)
                {
                    stagePosCount.y += 9.75f;
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Up)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                StageManager.instance.stageAreaCount--;

                if (ScenesManagers.sceneType == ScenesManagers.SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount == 8)
                    {
                        stagePosCount.x += 18f;
                        specialPosCount.x -= 7f;
                        specialPosCount.y -= 9f;
                    }
                }
                if (ScenesManagers.sceneType == ScenesManagers.SceneType.Stage2)
                {
                    stagePosCount.y -= 9.75f;
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
        }

        if (marginDistanceX != Vector2.zero &&
            Vector2.Distance(marginDistanceX, new Vector2(player.transform.position.x, player.transform.position.z)) > margin)
        {
            marginDistanceX = Vector2.zero;
            playerScreenRangeChecker.isStop = false;
        }
        if (marginDistanceY != Vector2.zero &&
            Vector2.Distance(marginDistanceY, new Vector2(player.transform.position.y, player.transform.position.z)) > margin)
        {
            marginDistanceY = Vector2.zero;
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
            marginDistanceX = marginDistanceY = Vector2.zero;
            Player.instance.ResetPosition();
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

        if (StageManager.instance.SceneEndAreaChecker(ScenesManagers.sceneType))
        {
            GameSystemOwner.isClear = true;
            yield break;
        }

        isPlayerScreenMove = false;
        marginDistanceX = new Vector2(player.transform.position.x, player.transform.position.z);
        marginDistanceY = new Vector2(player.transform.position.y, player.transform.position.z);
    }

    IEnumerator StageChangeBigupBlackout(GameObject beforeObj, GameObject afterObj)
    {
        yield return StartCoroutine(Generic.BigupObj(beforeObj, 3.0f, 1f));
        yield return StartCoroutine(Generic.BlackOut(beforeObj.GetComponent<SpriteRenderer>(), 0.02f));
        beforeObj.SetActive(false);
        afterObj.SetActive(true);
    }
}
