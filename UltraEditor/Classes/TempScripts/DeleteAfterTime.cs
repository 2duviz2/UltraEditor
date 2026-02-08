namespace UltraEditor.Classes.TempScripts;

using UnityEngine;

public class DeleteAfterTime : MonoBehaviour
{
    public float time = 1;

    public void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0)
            Destroy(gameObject);
    }
}