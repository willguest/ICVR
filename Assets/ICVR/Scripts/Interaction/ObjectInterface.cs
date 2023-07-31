/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using UnityEngine;

namespace ICVR
{
    /// <summary>
    /// A simplified version of 'Grabbable', for single handed interaction only. For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/ObjectInterface.md"/>
    /// </summary>
    public class ObjectInterface : MonoBehaviour
    {
        [SerializeField] private Transform controlPoseLeft;
        [SerializeField] private Transform controlPoseRight;

        private Transform previousParent;
        private GameObject currentManipulator;

        private bool IsBeingUsed;

        public void ToggleActivation()
        {
            IsBeingUsed = !IsBeingUsed;

            if (currentManipulator != null && !IsBeingUsed)
            {
                LoseControl();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsBeingUsed) { return; }

            if (other.gameObject.layer == 15)
            {
                if (other.gameObject.TryGetComponent(out XRControllerInteraction xrctrl))
                {
                    if (!xrctrl.IsControllingObject)
                    {
                        previousParent = other.gameObject.transform;
                        ReceiveControl(other.gameObject);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsBeingUsed) { return; }

            if (other.gameObject.layer == 15)
            {
                if (!other.gameObject.GetComponent<XRControllerInteraction>().IsControllingObject)
                {
                    LoseControl();
                }
            }
        }

        private void ReceiveControl(GameObject manipulator)
        {
            currentManipulator = manipulator.transform.Find("model").gameObject;

            // disable hand colliders, to prevent interference with object colliders and rigidbodies
            foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
            {
                cc.enabled = false;
            }

            currentManipulator.transform.parent = gameObject.transform;
            Transform activeControlPose = controlPoseRight;

            if (manipulator.name.ToLower().Contains("left"))
            {
                activeControlPose = controlPoseLeft;
            }

            StartCoroutine(LerpToControlPose(currentManipulator, activeControlPose.localPosition, activeControlPose.localRotation, 0.4f));
            IsBeingUsed = true;
        }


        private void LoseControl()
        {
            if (currentManipulator == null) return;

            currentManipulator.transform.parent = previousParent.transform;

            if (gameObject.GetComponent<ControlDynamics>())
            {
                gameObject.GetComponent<ControlDynamics>().ResetPose();
            }

            StartCoroutine(LerpToControlPose(currentManipulator, Vector3.zero, Quaternion.identity, 0.4f));

            // re-enable hand colliders
            foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
            {
                cc.enabled = true;
            }

            currentManipulator = null;
            IsBeingUsed = false;
        }


        private IEnumerator LerpToControlPose(GameObject objToLerp, Vector3 endPosition, Quaternion endRotation, float duration)
        {
            yield return new WaitForEndOfFrame();

            Transform t = objToLerp.transform;
            float time = 0;
            while (time < duration)
            {
                objToLerp.transform.localPosition = Vector3.Lerp(t.localPosition, endPosition, time / duration);
                objToLerp.transform.localRotation = Quaternion.Slerp(t.localRotation, endRotation, time / duration);

                time += Time.deltaTime;
                yield return null;
            }

            objToLerp.transform.localPosition = endPosition;
            objToLerp.transform.localRotation = endRotation;
        }
    }
}