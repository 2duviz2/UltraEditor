using System;
using System.Collections.Generic;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;

namespace UltraEditor.Classes.Editor
{
    public static class EditorVariablesList
    {
        /// <summary> Static list of every editor variable </summary>
        public static List<EditorVariable> editorVariables;

        /// <summary> Sets every editor variable avaliable </summary>
        public static void SetupEditorVariables()
        {
            editorVariables = [];
            NewInspectorVariable("localPosition", typeof(Transform), "Position");
            NewInspectorVariable("localEulerAngles", typeof(Transform), "Rotation");
            NewInspectorVariable("localScale", typeof(Transform), "Scale");

            NewInspectorVariable("matType", typeof(CubeObject), "Material type");
            NewInspectorVariable("matTiling", typeof(CubeObject), "Material scale");
            NewInspectorVariable("shape", typeof(CubeObject), "Object shape");
            NewInspectorVariable("isTrigger", typeof(CubeObject), "Ignore collision");
            NewInspectorVariable("fixMaterialTiling", typeof(CubeObject), "Center texture");
            NewInspectorVariable("customTextureUrl", typeof(CubeObject), "Custom texture URL");

            NewInspectorVariable("enemies", typeof(ActivateArena), "Enemies");
            NewInspectorVariable("onlyWave", typeof(ActivateArena), "Is only wave");

            NewInspectorVariable("nextEnemies", typeof(ActivateNextWave), "Next enemies");
            NewInspectorVariable("toActivate", typeof(ActivateNextWave), "To activate");
            NewInspectorVariable("enemyCount", typeof(ActivateNextWave), "Enemy count");
            NewInspectorVariable("lastWave", typeof(ActivateNextWave), "Last wave");

            NewInspectorVariable("toActivate", typeof(ActivateObject), "To activate");
            NewInspectorVariable("toDeactivate", typeof(ActivateObject), "To deactivate");
            NewInspectorVariable("canBeReactivated", typeof(ActivateObject), "Can be reactivated");
            NewInspectorVariable("delay", typeof(ActivateObject), "Trigger delay");

            NewInspectorVariable("intensity", typeof(Light), "Intensity");
            NewInspectorVariable("range", typeof(Light), "Range");
            NewInspectorVariable("type", typeof(Light), "Type");
            NewInspectorVariable("color", typeof(Light), "Color");

            NewInspectorVariable("rooms", typeof(CheckPoint), "Rooms");
            NewInspectorVariable("checkpointRooms", typeof(CheckpointObject), "Rooms");

            NewInspectorVariable("notInstakill", typeof(DeathZone), "Not instakill");
            NewInspectorVariable("damage", typeof(DeathZone), "Damage");
            NewInspectorVariable("affected", typeof(DeathZone), "Affected entities");

            NewInspectorVariable("calmThemePath", typeof(MusicObject), "Calm theme URL");
            NewInspectorVariable("battleThemePath", typeof(MusicObject), "Battle theme URL");

            NewInspectorVariable("url", typeof(SFXObject), "Sound URL");
            NewInspectorVariable("disableAfterPlaying", typeof(SFXObject), "Disable after playing");
            NewInspectorVariable("playOnAwake", typeof(SFXObject), "Play on awake");
            NewInspectorVariable("loop", typeof(SFXObject), "Loop");
            NewInspectorVariable("range", typeof(SFXObject), "Range");
            NewInspectorVariable("volume", typeof(SFXObject), "Volume");


            NewInspectorVariable("message", typeof(HUDMessageObject), "Message");
            NewInspectorVariable("disableAfterShowing", typeof(HUDMessageObject), "Disable after showing");

            NewInspectorVariable("teleportPosition", typeof(IO.SaveObjects.TeleportObject), "Teleport position");
            NewInspectorVariable("canBeReactivated", typeof(IO.SaveObjects.TeleportObject), "Can be reactivated");
            NewInspectorVariable("slowdown", typeof(IO.SaveObjects.TeleportObject), "Slowdown effect");

            NewInspectorVariable("tipOfTheDay", typeof(LevelInfoObject), "Tip of the Day");
            NewInspectorVariable("levelLayer", typeof(LevelInfoObject), "Level layer");
            NewInspectorVariable("levelName", typeof(LevelInfoObject), "Level name");
            NewInspectorVariable("playMusicOnDoorOpen", typeof(LevelInfoObject), "Play music on start");
            NewInspectorVariable("changeLighting", typeof(LevelInfoObject), "Change lighting");
            NewInspectorVariable("ambientColor", typeof(LevelInfoObject), "Ambient color");
            NewInspectorVariable("intensityMultiplier", typeof(LevelInfoObject), "Lighting intensity");
            NewInspectorVariable("skybox", typeof(LevelInfoObject), "Skybox");
            NewInspectorVariable("customSkyboxUrl", typeof(LevelInfoObject), "Custom skybox URL");

            NewInspectorVariable("scrolling", typeof(CubeTilingAnimator), "Scrolling");
            NewInspectorVariable("affectedCubes", typeof(CubeTilingAnimator), "Affected objects");

            NewInspectorVariable("affectedCubes", typeof(MovingPlatformAnimator), "Affected objects");
            NewInspectorVariable("points", typeof(MovingPlatformAnimator), "Target points");
            NewInspectorVariable("speed", typeof(MovingPlatformAnimator), "Speed");
            //NewInspectorVariable("movesWithThePlayer", typeof(MovingPlatformAnimator));
            NewInspectorVariable("mode", typeof(MovingPlatformAnimator), "Mode");

            //NewInspectorVariable("acceptedItemType", typeof(SkullActivatorObject));
            NewInspectorVariable("triggerAltars", typeof(SkullActivatorObject), "Trigger altars");
            NewInspectorVariable("toActivate", typeof(SkullActivatorObject), "To activate");
            NewInspectorVariable("toDeactivate", typeof(SkullActivatorObject), "To deactivate");

            NewInspectorVariable("content", typeof(BookObject), "Content");
        }

        /// <summary> Creates a new EditorVariable </summary>
        static void NewInspectorVariable(string varName, Type parentComponent, string varDisplay)
        {
            new EditorVariable(varName, varDisplay, parentComponent);
        }

        /// <summary> Gets the display name for that specific variable </summary>
        public static string GetVariableDisplay(string varName, Type parentComponent) =>
            editorVariables.Find(v => v.varName == varName && v.parentComponent == parentComponent)?.varDisplay ?? varName;
    }
}
