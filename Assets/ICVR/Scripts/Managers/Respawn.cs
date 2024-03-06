/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using UnityEngine;
using ICVR.SharedAssets;

namespace ICVR
{
    /// <summary>
    /// Replaces object when they fall out of the scene and hit the 'Planes of Destruction'. 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Managers/Respawn.md"/>
    /// </summary>
    public class Respawn : MonoBehaviour
    {
        [SerializeField] private Transform DefaultRespawnPose;
        [SerializeField] private GameObject characterRoot;
        [SerializeField] private List<GameObject> characterColliders;

        void Start()
        {

        }

        void OnTriggerEnter(Collider col)
        {
            GameObject respawnObject = col.gameObject;
            ManageRespawn(respawnObject);
        }

        private void ManageRespawn(GameObject respawnObject)
        {
            if (characterColliders.Contains(respawnObject))
            {
                ReplaceCharacter(respawnObject);
            }
            else
            {
                ReplaceObject(respawnObject);
            }
        }

        private void ReplaceObject(GameObject obj)
        {
            string name = obj.name;
            Vector3 scale = obj.transform.localScale;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            SharedAsset sa = obj.GetComponent<SharedAsset>();

            // put shared assets back where they started
            if (sa)
            {
                obj.transform.position = sa.DefaultLocation;
                obj.transform.rotation = sa.DefaultRotation;
                obj.transform.localScale = sa.DefaultScale;
            }
            else
            {
                obj.transform.position = DefaultRespawnPose.position;
                obj.transform.rotation = DefaultRespawnPose.rotation;
                obj.transform.localScale = scale;
            }

            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private void ReplaceCharacter(GameObject specialObject)
        {
            characterRoot.transform.position = Vector3.zero + Vector3.up * 1.0f;
            characterRoot.transform.rotation = Quaternion.identity;
            characterRoot.GetComponent<Rigidbody>().velocity = Vector3.zero;
            characterRoot.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}