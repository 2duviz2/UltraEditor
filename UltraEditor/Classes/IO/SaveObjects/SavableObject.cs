using System;
using UnityEngine;

namespace UltraEditor.Classes.IO.SaveObjects
{
    public class SavableObject : MonoBehaviour
    {
        public string Name;
        public Vector3 Position;
        public Vector3 EulerAngles;
        public Vector3 Scale;

        public virtual void Update()
        {
            Name = gameObject.name;
            Position = transform.position;
            EulerAngles = transform.eulerAngles;
            Scale = transform.lossyScale;

            GetComponent<CubeObject>()?.Tick();
        }
    }
}