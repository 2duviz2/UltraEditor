using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class MusicObject : SavableObject
    {
        public string calmThemePath = "https://duviz.xyz/static/audio/altars.mp3";
        public string battleThemePath = "https://duviz.xyz/static/audio/altars.mp3";

        bool used = false;

        public static MusicObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            MusicObject musicObject = target.AddComponent<MusicObject>();
            if (spawnedObject != null) spawnedObject.musicObject = musicObject;
            return musicObject;
        }

        public AudioClip calmClip = null;
        public AudioClip battleClip = null;

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

        void DownloadMusic(bool force = false)
        {
            MusicManager musicManager = MusicManager.Instance;

            if (calmClip == null || force)
                StartCoroutine(GetAudio(calmThemePath, clip =>
                {
                    calmClip = clip;
                    musicManager.cleanTheme.clip = clip;
                    if (EditorManager.logShit)
                        Plugin.LogInfo("Clip downloaded!");
                    CheckBothReady();
                }));

            if (battleClip == null || force)
                StartCoroutine(GetAudio(battleThemePath, clip =>
                {
                    battleClip = clip;
                    musicManager.battleTheme.clip = clip;
                    musicManager.bossTheme.clip = clip;
                    if (EditorManager.logShit)
                        Plugin.LogInfo("Clip downloaded!");
                    CheckBothReady();
                }));
        }


        IEnumerator GetAudio(string url, Action<AudioClip> callback)
        {
            AudioType audType = url[(url.LastIndexOf('.') + 1)..].ToLower() switch
            {
                "wav" => AudioType.WAV,
                "ogg" => AudioType.OGGVORBIS,
                "mp3" => AudioType.MPEG,
                "mp4" => AudioType.MPEG,
                _ => AudioType.MPEG
            };

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audType);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError("Audio download error: " + www.error);
                callback(null);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileNameWithoutExtension(url);
                callback(clip);
            }
        }
    }
}