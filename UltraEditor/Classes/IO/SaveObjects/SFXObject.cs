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
    public class SFXObject : SavableObject
    {
        public string url = "https://duviz.xyz/static/audio/altars.mp3";
        public bool disableAfterPlaying = false;
        public bool playOnAwake = false;
        public bool loop = false;
        public float range = -1;
        public float volume = 1;

        AudioSource source = null;

        bool used = false;
        bool started = false;

        public static SFXObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            SFXObject obj = target.AddComponent<SFXObject>();
            if (spawnedObject != null) spawnedObject.sfxObject = obj;
            return obj;
        }

        public AudioClip clip = null;

        public static List<(string, AudioClip)> cachedClips = [];

        public void Start()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }

        public override void Create()
        {
            started = false;
            DownloadMusic();
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialize = range <= 0 ? false : true;
            source.maxDistance = range;
            source.minDistance = 0;
            source.volume = volume;
            source.loop = loop;
        }

        public override void Tick()
        {
            if (EditorManager.canOpenEditor) // update while in editor
            {
                if (source == null) Create();
                source.spatialize = range <= 0 ? false : true;
                source.maxDistance = range;
                source.minDistance = 0;
                source.volume = volume;
                source.loop = loop;
            }
            if (!started && playOnAwake && NewMovement.Instance.activated && Time.timeScale != 0)
            {
                CheckReady(true);
            }
        }

        public void OnDisable()
        {
            started = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Player") return;
            if (used) return;
            if (disableAfterPlaying)
                used = true;

            DownloadMusic();

            CheckReady(true);
        }

        bool isWaiting = false;
        void CheckReady(bool waiting = false)
        {
            if (waiting) isWaiting = waiting;

            if (clip != null && isWaiting)
            {
                started = true;
                source.clip = clip;
                source.Play();
            }
        }

        void DownloadMusic(bool force = false)
        {
            if (clip == null || force)
                StartCoroutine(GetAudio(url, clip =>
                {
                    this.clip = clip;
                    if (EditorManager.logShit)
                        Plugin.LogInfo("Clip downloaded!");
                    CheckReady();
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