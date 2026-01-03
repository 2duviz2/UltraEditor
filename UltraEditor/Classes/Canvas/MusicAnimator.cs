using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.Canvas
{
    public class MusicAnimator : MonoBehaviour
    {
        public Animator animator;
        public AudioSource s1, s2;
        public string battleString = "battle";

        public void Start()
        {
            s1.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
            s2.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        }

        public void Update()
        {
            if (EditorManager.Instance == null) return;
            animator.SetBool(battleString, EditorManager.Instance.cameraSelector.selectedObject != null);
        }
    }
}