namespace UltraEditor.Classes;

using UnityEngine;
using UnityEngine.EventSystems;

internal class WindowDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform parentWindow;
    RectTransform canvasRect;
    Vector2 offset;

    void Start()
    {
        canvasRect = GetComponentInParent<UnityEngine.Canvas>().GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerOnCanvas
        );

        offset = parentWindow.anchoredPosition - pointerOnCanvas;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerOnCanvas
        );

        var newPos = pointerOnCanvas + offset;

        parentWindow.anchoredPosition = newPos;
    }
}