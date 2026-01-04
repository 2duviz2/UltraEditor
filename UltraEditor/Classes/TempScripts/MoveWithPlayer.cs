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
        public void OnCollisionStay(Collision collision)
        {
            if (!collision.collider.CompareTag("Player")) return;

            foreach (var c in collision.contacts)
            {
                if (c.normal.y > 0.5f)
                {
                    var rb = NewMovement.Instance;
                    if (rb != null)
                        rb.pushForce += delta;
                    break;
                }
            }
        }
    }
}
