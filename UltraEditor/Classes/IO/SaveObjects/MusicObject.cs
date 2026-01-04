using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static List<(string, AudioClip)> cachedClips = [];

        public void Start()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }

        public void createMusic()
        {
            DownloadMusic();
            if (EditorManager.canOpenEditor) { MusicManager.Instance.ForceStopMusic(); MusicManager.Instance.forcedOff = false; }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Player") return;
            if (used) return;
            if (FindObjectsOfType<MusicObject>().Length > 1) { MusicManager.Instance.ForceStopMusic(); MusicManager.Instance.forcedOff = false; }
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
                MusicManager musicManager = MusicManager.Instance;
                musicManager.cleanTheme.clip = calmClip;
                musicManager.battleTheme.clip = battleClip;
                musicManager.bossTheme.clip = battleClip;
                musicManager.forcedOff = false;
                musicManager.StartMusic();
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
            (string, AudioClip) cached = cachedClips.FirstOrDefault(x => x.Item1 == url);

            if (cached.Item2 != null)
            {
                callback(cached.Item2);
                yield break;
            }

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
                cachedClips.Add((url, clip));
                callback(clip);
            }
        }
    }
}