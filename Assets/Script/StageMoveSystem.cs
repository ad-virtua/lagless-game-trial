using System.Collections;
using UnityEngine;

public class StageMoveSystem : MonoBehaviour
{
    public static StageMoveSystem instance;

    [SerializeField] private GameObject player, eye;
    [SerializeField] private GameObject clearUI;
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
                    if (StageManager.instance.SceneEndAreaChecker(SceneManager.SceneType.Tutorial))
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.BigupObj(eye, 3f, 2.5f));
                    }
                    else
                    {
                        StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                        StartCoroutine(Generic.BigupObj(eye, 0.2f, 2.5f));
                    }
                }
            }
            else if (cameraWasDirection == ScreenRangeChecker.CameraWasDirection.Left)
            {
                isScreenMove = playerScreenRangeChecker.isStop = true;
                stagePosCount.x += 18f;
                StageManager.instance.stageAreaCount--;
                StartCoroutine(ScreenMove(new Vector3(stagePosCount.x, transform.position.y, transform.position.z)));
                StartCoroutine(Generic.Shake(2.5f, 0.1f, Camera.main.gameObject, false));
                StartCoroutine(Generic.SmallupObj(eye, 0.2f, 2.5f));
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

        if (StageManager.instance.SceneEndAreaChecker(SceneManager.instance.sceneType))
        {
            clearUI.SetActive(true);
            yield break;
        }

        isScreenMove = false;
        marginDistance = new Vector2(player.transform.position.x, player.transform.position.z);
    }
}
