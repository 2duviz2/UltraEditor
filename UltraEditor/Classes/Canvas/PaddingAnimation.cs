namespace UltraEditor.Classes.Canvas;

using UnityEngine;
using UnityEngine.UI;

internal class PaddingAnimation : MonoBehaviour
{
    public int leftOffset = -5;
    public int toReset = -10;

    float leftPadding = 0;

    public void Update()
    {
        leftPadding += leftOffset * Time.unscaledDeltaTime;
        leftPadding = Mathf.Repeat(leftPadding, Mathf.Abs(toReset));
        GetComponent<HorizontalLayoutGroup>().padding.left = -(int)leftPadding;

        LayoutRebuilder.MarkLayoutForRebuild(GetComponent<HorizontalLayoutGroup>().GetComponent<RectTransform>());
    }
}
