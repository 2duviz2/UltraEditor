using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;
using NewTeleportObject = UltraEditor.Classes.IO.SaveObjects.TeleportObject;

namespace UltraEditor.Classes.Editor
{
    public static class EditorComponentsList
    {
        /// <summary> Static list of every editor component </summary>
        public static List<EditorComponent> editorComponents;

        /// <summary> Sets every editor component avaliable </summary>
        public static void SetupEditorComponents()
        {
            editorComponents = [];
            new EditorComponent(typeof(ActivateArena), true, 
                $"When touched, every <b>enemy</b> in <b>{EditorVariablesList.GetVariableDisplay("enemies", typeof(ActivateArena))}</b> will be activated.<br>" +
                $"If <b>{EditorVariablesList.GetVariableDisplay("onlyWave", typeof(ActivateArena))}</b> is <b>True</b> when triggered, then the music will be set to the combat track " +
                $"until an <b>ActivateNextWave</b> with <b>{EditorVariablesList.GetVariableDisplay("lastWave", typeof(ActivateNextWave))}</b> set to true is " +
                $"activated</b>"
            );
            new EditorComponent(typeof(ActivateNextWave), true,
                $"When the amount of <b>dead enemies</b> inside the object reaches <b>{EditorVariablesList.GetVariableDisplay("enemyCount", typeof(ActivateNextWave))}</b>, two things can happen,\n" +
                $"If <b>{EditorVariablesList.GetVariableDisplay("lastWave", typeof(ActivateNextWave))}</b> is <b>True</b>:\n" +
                $"    Every object in <b>{EditorVariablesList.GetVariableDisplay("toActivate", typeof(ActivateNextWave))}</b> will get enabled.\n" +
                $"If <b>not</b>:\n" +
                $"    Every enemy in <b>{EditorVariablesList.GetVariableDisplay("nextEnemies", typeof(ActivateNextWave))} will get enabled, usually placed in another ActivateNextWave.</b>"
            );
            new EditorComponent(typeof(ActivateObject), true,
                $"When touched by the <b>player</b>, after the time in <b>{EditorVariablesList.GetVariableDisplay("delay", typeof(ActivateObject))}</b> has passed, every object in <b>{EditorVariablesList.GetVariableDisplay("toActivate", typeof(ActivateObject))}</b> will get enabled and every object in <b>{EditorVariablesList.GetVariableDisplay("toDeactivate", typeof(ActivateObject))}</b> will get disabled." +
                $"If <b>{EditorVariablesList.GetVariableDisplay("canBeReactivated", typeof(ActivateObject))}</b> is set to <b>False</b>, the trigger will not be able to be touched again."
            );
            new EditorComponent(typeof(HUDMessageObject), true,
                $"When touched by the <b>player</b>, the message in <b>{EditorVariablesList.GetVariableDisplay("message", typeof(HUDMessageObject))}</b> will show for the player.\n" +
                $"If <b>{EditorVariablesList.GetVariableDisplay("disableAfterShowing", typeof(HUDMessageObject))}</b> is <b>True</b>, the trigger will not be able to be touched again."
            );
            new EditorComponent(typeof(NewTeleportObject), true,
                $"When touched by the <b>player</b>, the player will be <b>teleported</b> to the position in <b>{EditorVariablesList.GetVariableDisplay("NewTeleportObject", typeof(NewTeleportObject))}</b> without losing any velocity.\n" +
                $"A slowdown effect will appear when touched if <b>{EditorVariablesList.GetVariableDisplay("slowdown", typeof(NewTeleportObject))}</b> is <b>True</b>.\n" +
                $"If <b>{EditorVariablesList.GetVariableDisplay("canBeReactivated", typeof(NewTeleportObject))}</b> is <b>False</b>, the trigger will not be able to be touched again."
            );
            new EditorComponent(typeof(LevelInfoObject), true,
                $"There should only be <b>one</b> LevelInfoObject in the level, every variable does something in specific:" +
                $"{VariableDescription("tipOfTheDay", typeof(LevelInfoObject), "Changes terminal's Tip of the Day, only works when playing the level via the main menu.")}\n" +
                $"{VariableDescription("levelLayer", typeof(LevelInfoObject), "Changes the hud title that shows the level layer, only works when playing the level via the main menu.")}\n" +
                $"{VariableDescription("levelName", typeof(LevelInfoObject), "Changed the hud title that shows the level name, only works when playing the level via the main menu.")}\n" +
                $"{VariableDescription("playMusicOnDoorOpen", typeof(LevelInfoObject), "Makes the music play when the first door is opened, only works when playing the level via the main menu.")}\n" +
                $"{VariableDescription("changeLighting", typeof(LevelInfoObject), "If enabled, every lighting variable will  change the light.")}\n" +
                $"{VariableDescription("ambientColor", typeof(LevelInfoObject), "Changes the ambient color of the level.")}\n" +
                $"{VariableDescription("intensityMultiplier", typeof(LevelInfoObject), "Changes default light strenght.")}\n" +
                $"{VariableDescription("skybox", typeof(LevelInfoObject), "Changes the skybox type.")}\n"
            );
            new EditorComponent(typeof(DeathZone), true);
            new EditorComponent(typeof(Light), true);
            new EditorComponent(typeof(MusicObject), true);
            new EditorComponent(typeof(SFXObject), true);
            new EditorComponent(typeof(CubeTilingAnimator), true);
            new EditorComponent(typeof(MovingPlatformAnimator), true);
            new EditorComponent(typeof(SkullActivatorObject), true);
        }

        /// <summary> Returns a list of every type in editorComponents without any other property </summary>
        /// <returns> List of Types the editor has </returns>
        public static List<Type> GetTypes() =>
            [.. editorComponents.Select(editorComp => editorComp.componentType)];

        /// <summary> Returns a list of every type in editorComponents that is a trigger </summary>
        /// <returns> List of Types that are a trigger </returns>
        public static List<Type> GetTriggerTypes() =>
            [.. from c in editorComponents where c.isTrigger select c.componentType];

        /// <summary> Returns if the Component is a trigger </summary>
        /// <returns> True if the component is a trigger </returns>
        public static bool IsTrigger(Component c)
        {
            return GetTriggerTypes().Contains(c.GetType());
        }

        /// <summary> Returns the component's description </summary>
        /// <returns> Component's description </returns>
        public static string GetDescription(Component c)
        {
            return editorComponents.FirstOrDefault(t => t.componentType == c.GetType())?.description;
        }

        /// <summary> Returns a list of MonoBehaviour types avaliable for the editor </summary>
        /// <param name="forceNormalOnes"> Forces to return EditorComponents as if AdvancedInspector was false </param>
        /// <returns> List of MonoBehaviour types avaliable for the editor </returns>
        public static List<Type> GetMonoBehaviourTypes(bool forceNormalOnes = false)
        {
            // Grab components from EditorComponentsList
            if ((EditorManager.Instance != null && !EditorManager.advancedInspector) || forceNormalOnes)
                return GetTypes();

            // Get Unity's default components (unused unless AdvancedInspector is true and forceNormalOnes is false)
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var engineAssemblyNames = new[]
            {
                "UnityEngine.CoreModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.UIModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.AIModule",
                "UnityEngine.AudioModule",
                "UnityEngine.ParticleSystemModule"
            };

            foreach (var name in engineAssemblyNames)
            {
                var a = assemblies.FirstOrDefault(x => x.GetName().Name == name);
                if (a == null)
                {
                    try { assemblies.Add(Assembly.Load(name)); }
                    catch { }
                }
            }

            var types = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
                })
                .Where(t => (t.IsSubclassOf(typeof(MonoBehaviour)) || typeof(Component).IsAssignableFrom(t)) && !t.IsAbstract)
                .ToList();

            return types;
        }

        /// <summary> Will return a variable preset string for the description text, it will highlight the variable name. </summary>
        static string VariableDescription(string varName, Type type, string varDescription)
        {
            return $"<b>{EditorVariablesList.GetVariableDisplay(varName, type)}</b> {varDescription}";
        }
    }
}