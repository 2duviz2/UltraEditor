namespace UltraEditor.Classes.TempScripts;

using UnityEngine;

public class RadianceWaiter : MonoBehaviour
{
    public float radianceTier = 0;

    public void OnEnable()
    {
        var e = GetComponent<EnemyIdentifier>();
        if (e != null)
        {
            e.radianceTier = radianceTier;
            e.healthBuff = e.speedBuff = e.damageBuff = true;
            e.Invoke("UpdateModifiers", 0);
        }
    }
}