namespace UltraEditor.Classes.Canvas;

using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaintImage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public TextureObject textureObj;
    public RawImage image;
    public Slider brushSlider;

    bool painting;

    public void OnPointerDown(PointerEventData eventData)
    {
        painting = true;
        Paint(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        painting = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (painting)
            Paint(eventData);
    }

    void Paint(PointerEventData eventData)
    {
        RectTransform rectTransform = image.rectTransform;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
            return;

        Rect rect = rectTransform.rect;

        float x = (localPoint.x - rect.x) / rect.width;
        float y = (localPoint.y - rect.y) / rect.height;

        Texture2D tex = textureObj.colonThree;

        int pixelX = Mathf.FloorToInt(x * tex.width);
        int pixelY = Mathf.FloorToInt(y * tex.height);

        if (pixelX < 0 || pixelY < 0 || pixelX >= tex.width || pixelY >= tex.height)
            return;

        Color brushColor = textureObj.previewImage.color;

        int half = (int)brushSlider.value / 2;

        for (int bx = -half; bx <= half; bx++)
        {
            for (int by = -half; by <= half; by++)
            {
                int px = pixelX + bx;
                int py = pixelY + by;

                if (px < 0 || py < 0 || px >= tex.width || py >= tex.height)
                    continue;

                tex.SetPixel(px, py, brushColor);
            }
        }

        tex.Apply();
    }
}
