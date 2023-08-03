/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using ICVR.SharedAssets;
using UnityEngine;

namespace ICVR
{
    /// <summary>
    /// Handles interaction events for the avatar's virtual hand. For more information see 
    /// <para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Multiplayer/AvatarHand.md"/>
    /// </summary>
    public class AvatarHand : MonoBehaviour
    {
        private FixedJoint[] attachJoint;
        private Rigidbody currentNearRigidBody = null;
        private Rigidbody currentFarRigidBody = null;
        private string prevLayer = "";

        void Start()
        {
            attachJoint = new FixedJoint[] { GetComponents<FixedJoint>()[0], GetComponents<FixedJoint>()[1] };
        }


        public void ReceiveInstruction(AvatarHandlingData instruction)
        {
            // nullable target of avatar interaction
            GameObject target;

            if (SharedAssetManager.Instance.SharedAssetRegister.TryGetValue(instruction.TargetId, out target))
            {
                if (!target)
                {
                    return;
                }

                if (instruction.EventType == AvatarInteractionEventType.AcquireData)
                {
                    if (instruction.Distance == ManipulationDistance.Near)
                    {
                        PickupNear(target, instruction.AcquisitionEvent);
                    }
                    else if (instruction.Distance == ManipulationDistance.Far)
                    {
                        PickupFar(target, instruction.AcquisitionEvent);
                    }
                }
                else if (instruction.EventType == AvatarInteractionEventType.ReleaseData)
                {
                    if (instruction.Distance == ManipulationDistance.Near)
                    {
                        DropNear(target, instruction.ReleaseEvent);
                    }
                    else if (instruction.Distance == ManipulationDistance.Far)
                    {
                        DropFar(target, instruction.ReleaseEvent);
                    }
                }
            }
        }

        public void PickupNear(GameObject target, AcquireData acquisition)
        {
            // access rigidbody of the remotely controlled object...
            currentNearRigidBody = target.GetComponent<Rigidbody>();

            if (!currentNearRigidBody) return;
            
            // move object to start pose 
            target.transform.position = acquisition.ObjectPosition;
            target.transform.rotation = acquisition.ObjectRotation;

            attachJoint[0].connectedBody = currentNearRigidBody;

            // rememeber layer and set as tool
            prevLayer = LayerMask.LayerToName(currentNearRigidBody.gameObject.layer);
            SetLayerRecursively(currentNearRigidBody.gameObject, LayerMask.NameToLayer("Tools"));
        }

        private void DropNear(GameObject target, ReleaseData release)
        {
            if (!currentNearRigidBody)
                return;

            // set starting pose
            target.transform.position = release.ReleasePosition;
            target.transform.rotation = release.ReleaseRotation;

            // release connections
            attachJoint[0].connectedBody = null;

            // apply forces
            currentNearRigidBody.AddForce(release.ForceData.LinearForce, ForceMode.Impulse);
            currentNearRigidBody.AddTorque(release.ForceData.AngularForce, ForceMode.Impulse);

            // reset and forget
            SetLayerRecursively(currentNearRigidBody.gameObject, LayerMask.NameToLayer(prevLayer));
            prevLayer = "";
            currentNearRigidBody = null;
        }

        public void PickupFar(GameObject target, AcquireData acquisition)
        {
            currentFarRigidBody = target.GetComponent<Rigidbody>();

            if (!currentFarRigidBody) return;

            target.transform.position = acquisition.ObjectPosition;
            target.transform.rotation = acquisition.ObjectRotation;

            attachJoint[1].connectedBody = currentFarRigidBody;
        }

        private void DropFar(GameObject target, ReleaseData release)
        {
            if (!currentFarRigidBody) return;

            target.transform.position = release.ReleasePosition;
            target.transform.rotation = release.ReleaseRotation;

            attachJoint[1].connectedBody = null;

            currentFarRigidBody.AddForce(release.ForceData.LinearForce, ForceMode.Impulse);
            currentFarRigidBody.AddTorque(release.ForceData.AngularForce, ForceMode.Impulse);

            currentFarRigidBody = null;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

    }
}
