using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image bg;
    public Image knob;

    private Vector2 inputVector;
    private bool isTouching = false;
    public bool IsTouching => isTouching;

    public Vector2 InputDirection => inputVector;

    private int activePointerId = -1;

    void Start()
    {
        if (bg == null) bg = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouching = true;
        activePointerId = eventData.pointerId; // この指だけ追う
        OnDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return; // 他の指は無視

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var pos
        );

        pos.x /= bg.rectTransform.sizeDelta.x;
        pos.y /= bg.rectTransform.sizeDelta.y;

        inputVector = new Vector2(pos.x * 2, pos.y * 2);
        inputVector = inputVector.magnitude > 1 ? inputVector.normalized : inputVector;

        knob.rectTransform.anchoredPosition = new Vector2(
            inputVector.x * (bg.rectTransform.sizeDelta.x / 2),
            inputVector.y * (bg.rectTransform.sizeDelta.y / 2)
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return; // 他の指なら無視

        isTouching = false;
        inputVector = Vector2.zero;
        knob.rectTransform.anchoredPosition = Vector2.zero;
        activePointerId = -1;
    }
}