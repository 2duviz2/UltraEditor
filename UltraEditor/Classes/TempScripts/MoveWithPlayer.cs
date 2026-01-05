using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace UltraEditor.Classes.TempScripts
{
    public class MoveWithPlayer : MonoBehaviour
    {
        public Vector3 delta;
        public GameObject ghost = null;
        public void Start()
        {
            ghost = new GameObject("Ghost");
            ghost.transform.localScale = Vector3.one;
        }

        public void FixedUpdate()
        {
            ghost.transform.position = transform.position;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Player")) return;

            PlayerMovementParenting.Instance.AttachPlayer(ghost.transform);
        }

        public void OnCollisionExit(Collision collision)
        {
            if (!collision.collider.CompareTag("Player")) return;

            PlayerMovementParenting.Instance.DetachPlayer(ghost.transform);
        }
    }
}
