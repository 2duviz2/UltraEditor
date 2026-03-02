global using static UltraEditor.Libraries.UnityExtensions;
global using UnityObject = UnityEngine.Object;

namespace UltraEditor.Libraries;

using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> a bunch of useful unity extensions. </summary>
public static class UnityExtensions
{
    extension(Vector3 vector)
    {
        /// <summary> Converts a vector3 into a color. </summary>
        public Color ToColor() =>
             new(vector.x / 255f, vector.y / 255f, vector.z / 255f);
    }

    extension(Component comp)
    {
        /// <summary> Gets the gameObject parent of this component. </summary>
        public GameObject parent =>
            comp.transform.parent?.gameObject;

        /// <summary> Adds a component to the gameObject of a component. </summary>
        public T AddComponent<T>() where T : Component =>
            comp.gameObject.AddComponent<T>();

        /// <summary> Sets the gameObject of a component to be enabled </summary>
        public void SetActive(bool value) =>
            comp.gameObject.SetActive(value);

        /// <summary> Finds a child by name <paramref name="childName"/> and returns it. </summary>
        /// <remarks> If no child with name <paramref name="childName"/> can be found, null is returned. If <paramref name="childName"/> contains a '/' character it will access the Gameobject in the hierarchy like a path name. </remarks>
        public GameObject Find(string childName) =>
            comp.transform.Find(childName)?.gameObject;

        /// <summary> Finds a child by name <paramref name="childName"/> then gets a component from it and returns the result. </summary>
        /// <remarks> If no child with name <paramref name="childName"/> can be found or the child doesn't have that component, null is returned. If <paramref name="childName"/> contains a '/' character it will access the Gameobject in the hierarchy like a path name. </remarks>
        public T Find<T>(string childName) where T : Component =>
            comp.Find(childName)?.GetComponent<T>();
    }

    extension(GameObject obj)
    {
        /// <summary> The GameObject parent of this gameObject. </summary>
        public GameObject parent
        {
            get => obj.transform.parent.gameObject;
            set => obj.transform.parent = value.transform;
        }

        /// <summary> The scale of the GameObject relative to the GameObject's parent. </summary>
        public Vector3 scale
        {
            get => obj.transform.localScale;
            set => obj.transform.localScale = value;
        }

        /// <summary> The rotation of the GameObject as Euler angles in degrees. </summary>
        public Vector3 rotation
        {
            get => obj.transform.eulerAngles;
            set => obj.transform.eulerAngles = value;
        }

        /// <summary> The world space position of the GameObject. </summary>
        public Vector3 position
        {
            get => obj.transform.position;
            set => obj.transform.position = value;
        }

        /// <summary> Gets a component from a gameObject or if it doesnt exist yet, add it. </summary>
        public T GetOrAddComponent<T>() where T : Component =>
            obj.GetComponent<T>() ?? obj.AddComponent<T>();

        /// <summary> Finds a child in a GameObject via its name/path. </summary>
        public GameObject Find(string childPath) =>
            obj.transform.Find(childPath)?.gameObject;

        /// <summary> Finds a child in a GameObject via its name/path, and gets a component from that child. </summary>
        public T Find<T>(string childPath) where T : Component =>
            obj.Find(childPath)?.GetComponent<T>();

        /// <summary> Creates a GameObject with the specified component. </summary>
        public static T Create<T>(string name) where T : Component =>
            new GameObject(name).AddComponent<T>();

        /// <summary> Creates a GameObject with the specified component and parent. </summary>
        public static T Create<T>(string name, Transform parent, bool active = true) where T : Component
        {
            GameObject gameObject = new(name);
            if (!active) gameObject.SetActive(false);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;

            return gameObject.AddComponent<T>();
        }

        /// <summary> Finds a GameObject based on name/path, doesnt matter if its enabled or not. </summary>
        public static GameObject FindObject(string Path, Scene? scene = null)
        {
            scene ??= SceneManager.GetSceneAt(0);
            string rootSearchObj = Path;
            if (Path.IndexOf('/') != -1)
            {
                rootSearchObj = Path[..Path.IndexOf('/')];
                Path = Path[(Path.IndexOf('/') + 1)..];
            }

            GameObject search = scene.Value.GetRootGameObjects().Where(g => g.name == rootSearchObj).FirstOrDefault();
            search = Path.IndexOf('/') == -1 ? search : search.transform.Find(Path)?.gameObject;
            return search;
        }
    }
}
