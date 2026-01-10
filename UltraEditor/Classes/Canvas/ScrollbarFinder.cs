using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltraEditor.Classes.Canvas
{
    /// <summary> Made this patch because unity cannot handle multiple objects listening for the same action, in this case, event 
    /// triggers not letting the scrollbar do its thing </summary>
    public class ScrollbarFinder : MonoBehaviour
    {
        public void OnScroll(BaseEventData data)
        {
            var e = (PointerEventData)data;
            var sr = transform.parent.GetComponentInParent<ScrollRect>();
            if (sr) sr.OnScroll(e);
        }

        public void Start()
        {
            EventTrigger et = GetComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Scroll;
            entry.callback.AddListener((data) => OnScroll(data));
            et.triggers.Add(entry);
        }
    }
}