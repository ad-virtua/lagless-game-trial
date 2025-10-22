using UnityEngine;

public class ScreenRangeChecker : MonoBehaviour
{
    private Camera mainCamera;
    private bool isInScreen = true;
    private bool wasInScreen = true;
    private Vector3 viewPos;

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
    }

    void Update()
    {
        if (isStop)
        {
            direction = CameraWasDirection.None;
            wasInScreen = true;
            return;
        }

        viewPos = mainCamera.WorldToViewportPoint(transform.position);

        isInScreen = (viewPos.x >= 0f && viewPos.x <= 1f &&
                      viewPos.y >= 0f && viewPos.y <= 1f &&
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
        if (viewPos.x < 0f) return CameraWasDirection.Left;
        if (viewPos.x > 1f) return CameraWasDirection.Right;
        if (viewPos.y < 0f) return CameraWasDirection.Down;
        if (viewPos.y > 1f) return CameraWasDirection.Up;

        // 万が一全部範囲内なら中央扱い
        return CameraWasDirection.Center;
    }

    public bool IsInScreen()
    {
        Debug.Log(isInScreen);
        return isInScreen;
    }

    public CameraWasDirection GetCameraWasDirection()
    {
        return direction;
    }
}
