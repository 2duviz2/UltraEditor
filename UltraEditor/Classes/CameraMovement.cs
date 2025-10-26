﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltraEditor.Classes
{
    public class CameraMovement : MonoBehaviour
    {
        public float movementSpeed = 50f;
        public float mouseSensitivity = 3f;
        public float shiftMultiplier = 3f;
        
        public void Update()
        {
            if (!Plugin.canMove()) return;
            if (EditorManager.Instance.blocker.activeSelf) return;

            float speed = movementSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.unscaledDeltaTime;
            float horizontal = Input.GetAxisRaw("Horizontal") * speed;
            float vertical = Input.GetAxisRaw("Vertical") * speed;
            float ascend = (Input.GetKey(KeyCode.E) ? 1 : 0 - (Input.GetKey(KeyCode.Q) ? 1 : 0)) * speed;
            transform.Translate(new Vector3(horizontal, ascend, vertical));
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
                transform.Rotate(Vector3.up, mouseX, Space.World);
                transform.Rotate(Vector3.right, -mouseY, Space.Self);
            }
        }
    }
}
