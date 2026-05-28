namespace UltraEditor.Classes.Canvas;

using UltraEditor.Libraries;
using UnityEngine;

public class MusicAnimator : MonoBehaviour
{
    public Animator animator;
    public AudioSource s1, s2;
    public string battleString = "battle";

    public void Start()
    {
        s1.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        s2.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        s1.enabled = SceneHelper.CurrentScene == EditorManager.EditorSceneName;
        s2.enabled = SceneHelper.CurrentScene == EditorManager.EditorSceneName;

        e = SceneHelper.CurrentScene == EditorManager.EditorSceneName;
    }

    public void OnEnable()
    {
        float pitch = Random.Range(0, 200) == 0 
            ? 0.25f
            : 1f;

        s1.SetPitch(pitch);
        s2.SetPitch(pitch);
    }

    bool e;

    public void Update()
    {
        if (EditorManager.Instance != null)
            animator.SetBool(battleString, EditorManager.Instance.cameraSelector.selectedObject != null);

        if (Preferences.GetInt("EditorMusic", 1) == 0)
        {
            s1.enabled = false;
            s2.enabled = false;
        }
        else
        {
            s1.enabled = e;
            s2.enabled = e;
        }
    }
}