using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class SpawnedObject : MonoBehaviour
    {
        public string ID = "";
        public string parentID = "";

        public ArenaObject arenaObject = null;
        public NextArenaObject nextArenaObject = null;
        public ActivateObject activateObject = null;
        public CheckpointObject checkpointObject = null;
        public DeathZoneObject deathZoneObject = null;
        public LightObject lightObject = null;
        public MusicObject musicObject = null;
        public HUDMessageObject hudObject = null;
        public TeleportObject teleportObject = null;
    }
}
