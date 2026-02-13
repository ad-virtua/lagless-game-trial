using System.Collections;
using UnityEngine;
using static ScenesManagers;
using static ScreenRangeChecker;
using static UnityEngine.GraphicsBuffer;

public class StageMoveSystem : MonoBehaviour
{
    public static StageMoveSystem instance;

    [SerializeField] private GameObject player, mouth, inMouth;
    [SerializeField] private GameObject tutorialStarted, tutorialLatter;
    [SerializeField] private float directionSpeed;
    [SerializeField] private GameObject flash;

    private ScreenRangeChecker playerScreenRangeChecker;
    [HideInInspector] public bool isPlayerScreenMove;

    private Vector3 stagePosCount;
    private Vector2 specialPosCount;
    private float margin = 1f;
    private Vector2 marginDistanceX, marginDistanceY;
    private CameraWasDirection beforeCameraWasDirection;

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
        if (ScenesManagers.instance.isPause) return;

        if (playerScreenRangeChecker && !isPlayerScreenMove && (marginDistanceX == Vector2.zero || marginDistanceY == Vector2.zero))
        {
            // ここで取得と同時にリセット
            var cameraWasDirection = playerScreenRangeChecker.GetCameraWasDirection();
            beforeCameraWasDirection = cameraWasDirection;

            if (cameraWasDirection == CameraWasDirection.Right)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += -18f;
                StageManager.instance.stageAreaCount++;

                if (sceneType == SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount == 5)
                    {
                        specialPosCount.y += 3f;
                    }
                    else if (StageManager.instance.stageAreaCount == 6)
                    {
                        specialPosCount.x += 7f;
                        specialPosCount.y += 9f;
                    }
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == CameraWasDirection.Left)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += 18f;
                StageManager.instance.stageAreaCount--;

                if (sceneType == SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount == 5)
                    {
                        specialPosCount.y -= 3f;
                    }
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == CameraWasDirection.Down)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                StageManager.instance.stageAreaCount++;

                if (sceneType == SceneType.Stage2 || sceneType == SceneType.Stage3)
                {
                    if (player.GetComponent<Player>().isSpecialScreenMove)
                    {
                        stagePosCount.y += player.GetComponent<Player>().specialMove;
                        StageManager.instance.stageAreaCount++;
                    }
                    else stagePosCount.y += 9.75f;
                }
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x + specialPosCount.x, stagePosCount.y + specialPosCount.y, transform.position.z)));
            }
            else if (cameraWasDirection == CameraWasDirection.Up)
            {
                isPlayerScreenMove = playerScreenRangeChecker.isStop = true;
                StageManager.instance.stageAreaCount--;

                if (sceneType == SceneType.Stage1)
                {
                    if (StageManager.instance.stageAreaCount == 6)
                    {
                        stagePosCount.x += 18f;
                        specialPosCount.x -= 7f;
                        specialPosCount.y -= 9f;
                    }
                }
                if (sceneType == SceneType.Stage2 || sceneType == SceneType.Stage3)
                {
                    if (player.GetComponent<Player>().isSpecialScreenMove)
                    {
                        stagePosCount.y -= player.GetComponent<Player>().specialMove;
                        StageManager.instance.stageAreaCount--;
                    }
                    else stagePosCount.y -= 9.75f;
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

        if (!player.GetComponent<Player>().isPortal &&
            !Player.instance.isGrounded &&
            beforeCameraWasDirection != CameraWasDirection.Up &&
            beforeCameraWasDirection != CameraWasDirection.Down)
        {
            marginDistanceY = new Vector2(player.transform.position.y, player.transform.position.z);
        }
        else
        {
            if (marginDistanceY != Vector2.zero &&
                Vector2.Distance(marginDistanceY, new Vector2(player.transform.position.y, player.transform.position.z)) > margin)
            {
                marginDistanceY = Vector2.zero;
                playerScreenRangeChecker.isStop = false;
            }
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
        gameObject.layer = 12;
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
        Player.instance.SavePos();
        player.GetComponent<Player>().isSpecialScreenMove = false;
        MoveEnd();
    }

    public void MoveEnd()
    {
        gameObject.layer = 6;
        player.GetComponent<Player>().isPortal = false;
        player.GetComponent<BoxCollider2D>().isTrigger = false;
        player.GetComponent<SpriteRenderer>().enabled = true;
    }


    IEnumerator StageChangeBigupBlackout(GameObject beforeObj, GameObject afterObj)
    {
        yield return StartCoroutine(Generic.BigupObj(beforeObj, 3.0f, 1f));
        yield return StartCoroutine(Generic.BlackOut(beforeObj.GetComponent<SpriteRenderer>(), 0.02f));
        beforeObj.SetActive(false);
        afterObj.SetActive(true);
    }

    public IEnumerator BossClear(GameObject enemy)
    {
        isPlayerScreenMove = true;
        GameSystemOwner.isClear = true;
        StartCoroutine(Generic.Shake(0.5f, 0.1f, Camera.main.gameObject, false));
        yield return new WaitForSeconds(2f);

        flash.SetActive(false);
        flash.SetActive(true);
        yield return new WaitForSeconds(2f);

        enemy.GetComponent<SpriteRenderer>().enabled = false;

        var gears = GameObject.FindGameObjectsWithTag("Gear");
        foreach (var gear in gears)
        {
            gear.GetComponent<Gear>().ChangeImage();
        }
        var atks = GameObject.FindGameObjectsWithTag("Atk");
        foreach (var atk in atks)
        {
            Destroy(atk);
        }

        flash.GetComponent<Animator>().SetTrigger("isFadeOut");
        isPlayerScreenMove = false;

        yield return new WaitForSeconds(3f);
        flash.SetActive(false);

        yield return new WaitForSeconds(2f);
        StageManager.instance.ChangeStage(3);
    }
}
