using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEditor.Classes.IO.SaveObjects;
using UnityEngine;

namespace UltraEditor.Classes.Editor
{
    public static class LoadingHelper
    {
        /// <summary>
        /// Returns a list of every found object in the list of ids
        /// </summary>
        /// <param name="ids">A list of ids to search for</param>
        /// <returns>List of objects retrieved from the ids</returns>
        public static List<GameObject> GetObjectsWithIdsList(List<string> ids)
        {
            return [.. GetObjectsWithIds(ids)];
        }

        /// <summary>
        /// Returns an array of every found object in the list of ids
        /// </summary>
        /// <param name="ids">A list of ids to search for</param>
        /// <returns>Array of objects retrieved from the ids</returns>
        public static GameObject[] GetObjectsWithIds(List<string> ids)
        {
            List<GameObject> foundObjects = [];
            foreach (var e in ids)
            {
                bool found = false;
                foreach (var obj in GameObject.FindObjectsOfType<SavableObject>(true))
                {
                    if (e == EditorManager.GetIdOfObj(obj.gameObject))
                    {
                        foundObjects.Add(obj.gameObject);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    foreach (var obj in GameObject.FindObjectsOfType<Transform>(true))
                    {
                        if (e == EditorManager.GetIdOfObj(obj.gameObject))
                        {
                            foundObjects.Add(obj.gameObject);
                            found = true;
                            break;
                        }
                    }
            }

            return [.. foundObjects];
        }
    }
}
