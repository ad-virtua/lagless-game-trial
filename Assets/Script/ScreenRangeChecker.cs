using UnityEngine;

public class ScreenRangeChecker : MonoBehaviour
{
    private Camera mainCamera;
    private bool isInScreen = true;
    private bool wasInScreen = true;
    private Vector3 viewPos;
    private Vector3 range;

    public enum CameraWasDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Center,
        In
    }
    private CameraWasDirection direction = CameraWasDirection.None;
    public bool isStop { get; set; }

    void Start()
    {
        mainCamera = Camera.main;
        range.x = 0.05f;
        range.y = 0.05f;
    }

    void Update()
    {
        if (ScenesManagers.instance.isPause) return;

        if (isStop)
        {
            direction = CameraWasDirection.None;
            wasInScreen = true;
            return;
        }

        viewPos = mainCamera.WorldToViewportPoint(transform.position);

        isInScreen = (viewPos.x >= 0f - range.x && viewPos.x <= 1f + range.x &&
                      viewPos.y >= 0f - range.y && viewPos.y <= 1f + range.y &&
                      viewPos.z > 0f);

        if (wasInScreen && !isInScreen)
        {
            direction = GetExitDirection();
            Debug.Log($"{gameObject.name} が画面外（{direction}）に出ました！");
        }
        else if (!wasInScreen && isInScreen)
        {
            Debug.Log($"{gameObject.name} が画面内に戻りました！");
        }

        wasInScreen = isInScreen;
    }

    private CameraWasDirection GetExitDirection()
    {
        // どの方向に出たかを判定
        if (viewPos.x < 0f - range.x) return CameraWasDirection.Left;
        if (viewPos.x > 1f + range.x) return CameraWasDirection.Right;
        if (viewPos.y < 0f - range.y) return CameraWasDirection.Down;
        if (viewPos.y > 1f + range.y) return CameraWasDirection.Up;

        // 万が一全部範囲内なら中央扱い
        return CameraWasDirection.Center;
    }

    public bool IsInScreen()
    {
        return isInScreen;
    }

    public CameraWasDirection GetCameraWasDirection()
    {
        return direction;
    }
}
