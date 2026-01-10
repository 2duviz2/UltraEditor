using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Libraries
{
    public static class TimePatcher
    {
        static float maxValue = 0.1f;

        public static float DeltaTime()
        {
            return Mathf.Min(Time.deltaTime, maxValue);
        }

        public static float UnscaledDeltaTime()
        {
            return Mathf.Min(Time.unscaledDeltaTime, maxValue);
        }
    }
}
