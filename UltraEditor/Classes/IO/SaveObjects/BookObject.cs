using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltraEditor.Classes.Editor;
using Unity.AI.Navigation;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class BookObject : SavableObject
    {
        public string content = "Hi!";

        public static BookObject Create(GameObject target, SpawnedObject spawnedObject = null)
        {
            BookObject obj = target.AddComponent<BookObject>();
            if (spawnedObject != null) spawnedObject.bookObject = obj;
            return obj;
        }

        public override void Create()
        {
            NavMeshModifier mod = gameObject.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            gameObject.GetComponent<Collider>().isTrigger = true;
        }

        bool spawned = false;
        public override void Tick()
        {
            if (Time.timeScale == 0) return;

            if (!spawned)
            {
                spawned = true;
                GameObject book = EditorManager.Instance.SpawnAsset("Assets/Prefabs/Items/Book.prefab", true);
                book.transform.position = transform.position;
                book.transform.rotation = transform.rotation;
                book.transform.localScale = transform.lossyScale;
                Readable r = book.GetComponent<Readable>();
                AccessTools.Field(r.GetType(), "content").SetValue(r, content);
            }
        }
    }
}
