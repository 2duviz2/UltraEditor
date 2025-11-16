using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace UltraEditor.Classes.Saving
{
    internal class MusicObject : SavableObject
    {
        public string calmThemeUrl = "https://duviz.xyz/static/audio/altars.mp3";
        public string battleThemeUrl = "https://duviz.xyz/static/audio/altars.mp3";

        bool used = false;

        public static MusicObject Create(GameObject target)
        {
            MusicObject obj = target.AddComponent<MusicObject>();
            return obj;
        }

        AudioClip calmClip = null;
        AudioClip battleClip = null;

        public void Start()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }

        public void createMusic()
        {
            DownloadMusic();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (used) return;
            used = true;

            DownloadMusic();

            CheckBothReady(true);
        }

        bool isWaiting = false;
        void CheckBothReady(bool waiting = false)
        {
            if (waiting) isWaiting = waiting;

            if (calmClip != null && battleClip != null && isWaiting)
            {
                MusicManager.Instance.StartMusic();
            }
        }

        void DownloadMusic()
        {
            MusicManager musicManager = MusicManager.Instance;

            if (calmClip == null)
                StartCoroutine(GetAudio(calmThemeUrl, clip => {
                    calmClip = clip;
                    musicManager.cleanTheme.clip = clip;
                    if (EditorManager.logShit)
                        Plugin.LogInfo("Clip downloaded!");
                    CheckBothReady();
                }));

            if (battleClip == null)
                StartCoroutine(GetAudio(battleThemeUrl, clip => {
                    battleClip = clip;
                    musicManager.battleTheme.clip = clip;
                    musicManager.bossTheme.clip = clip;
                    if (EditorManager.logShit)
                        Plugin.LogInfo("Clip downloaded!");
                    CheckBothReady();
                }));
        }

        IEnumerator GetAudio(string url, System.Action<AudioClip> callback)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError("Audio download error: " + www.error);
                callback(null);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = System.IO.Path.GetFileNameWithoutExtension(url);
                callback(clip);
            }
        }
    }
}