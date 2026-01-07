using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class PopupObject : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text contentText;
        
        public void SetPopupInfo(string title, string content)
        {
            titleText.text = title;
            contentText.text = content;
        }

        public void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}
