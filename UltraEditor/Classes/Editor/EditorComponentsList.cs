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
                $"When touched, every enemy in <b>{EditorVariablesList.GetVariableDisplay("enemies", typeof(ActivateArena))}</b> will be activated.<br>" +
                $"If <b>{EditorVariablesList.GetVariableDisplay("onlyWave", typeof(ActivateArena))}</b> is <b>True</b> when triggered, then the music will be set to the combat track " +
                $"until an <b>ActivateNextWave</b> with <b>{EditorVariablesList.GetVariableDisplay("lastWave", typeof(ActivateNextWave))}</b> set to true is " +
                $"activated</b>"
            );
            new EditorComponent(typeof(ActivateNextWave), true);
            new EditorComponent(typeof(ActivateObject), true);
            new EditorComponent(typeof(HUDMessageObject), true);
            new EditorComponent(typeof(NewTeleportObject), true);
            new EditorComponent(typeof(LevelInfoObject), true);
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
    }
}
