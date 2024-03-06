/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ICVR
{
    [System.Serializable]
    public class GameObjectFloatEvent : UnityEvent<float> { }

    [System.Serializable]
    public class GameObjectBoolEvent : UnityEvent<bool> { }

    /// <summary>
    /// A simplified version of 'Grabbable', for single handed interaction only. For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/ObjectInterface.md"/>
    /// </summary>
    public class ObjectInterface : MonoBehaviour
    {
        public bool IsBeingUsed;
        public bool IsBeingHeld;

        [SerializeField] private Transform controlPoseLeft;
        [SerializeField] private Transform controlPoseRight;
        [SerializeField] private string gripPose;

        [SerializeField] private UnityEvent OnGetFocusEvent;
        [SerializeField] private UnityEvent OnLoseFocusEvent;

        [SerializeField] private UnityEvent<bool> OnGripEvent;
        [SerializeField] private UnityEvent<float> OnTriggerEvent;

        private Transform previousParent;
        private GameObject currentManipulator;

        private float triggerEnterTick = 0f;
        private float triggerExitTick = 0f;

        public void ToggleActivation(GameObject manipulator, bool state)
        {
            if (manipulator == gameObject)
            {
                manipulator = null;
            }

            if (state)
            {
                if (ReceiveControl(manipulator))
                {
                    IsBeingUsed = state;
                    OnGetFocusEvent?.Invoke();
                }
            }
            else
            {
                if (LoseControl())
                {
                    OnLoseFocusEvent?.Invoke();
                    IsBeingUsed = state;
                }
            }
        }

        public void SetTrigger(float triggerValue)
        {
            if (IsBeingUsed || IsBeingHeld)
            {
                OnTriggerEvent?.Invoke(triggerValue);
            }
        }

        public void SetGrip(XRController xrc, bool state)
        {
            IsBeingHeld = state;
            OnGripEvent?.Invoke(state);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.realtimeSinceStartup - triggerEnterTick < 0.1f) return;
            if (!IsBeingUsed && other.gameObject.TryGetComponent(out XRController xrctrl))
            {
                triggerEnterTick = Time.realtimeSinceStartup;
                ToggleActivation(other.gameObject, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (Time.realtimeSinceStartup - triggerExitTick < 0.1f) return;
            if (!IsBeingUsed && other.gameObject.TryGetComponent(out XRController xrctrl))
            {
                triggerExitTick = Time.realtimeSinceStartup;
                ToggleActivation(null, false);
            }
        }

        private bool ReceiveControl(GameObject manipulator)
        {
            if (manipulator == null) return false;

            // hand-based control
            if (manipulator && manipulator.TryGetComponent(out XRController xrctrl))
            {
                // compatibility checks
                if (xrctrl.IsUsingInterface) return false;
                if (xrctrl.IsControllingObject) return false;

                Transform activeControlPose;
                if (xrctrl.hand == ControllerHand.LEFT && controlPoseLeft != null)
                {
                    activeControlPose = controlPoseLeft;
                }
                else if (xrctrl.hand == ControllerHand.RIGHT && controlPoseRight != null)
                {
                    activeControlPose = controlPoseRight;
                }
                else
                {
                    return false;
                }

                previousParent = manipulator.transform;

                // send grip update, if it exists
                if (!string.IsNullOrEmpty(gripPose))
                {
                    xrctrl.SetGripPose(gripPose);
                }

                xrctrl.IsUsingInterface = true;
                currentManipulator = manipulator.transform.Find("model")?.gameObject;

                // disable hand colliders
                foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
                {
                    cc.enabled = false;
                }

                StartCoroutine(LerpToControlPose(currentManipulator, transform,
                    activeControlPose.localPosition, activeControlPose.localRotation, 0.2f));
            }
            else
            {
                currentManipulator = manipulator;
            }
            return true;
        }

        private bool LoseControl()
        {
            if (currentManipulator == null) return false;

            if (IsBeingHeld) return false;

            if (gameObject.TryGetComponent(out ControlDynamics cd))
            {
                cd.ResetPose();
            }

            if (previousParent.TryGetComponent(out XRController xrc))
            {
                xrc.SetGripPose("relax");
                xrc.IsUsingInterface = false;

                StartCoroutine(LerpToControlPose(currentManipulator, previousParent,
                    Vector3.zero, Quaternion.identity, 0.2f));

                // re-enable hand colliders
                foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
                {
                    cc.enabled = true;
                }
            }
            currentManipulator = null;
            return true;
        }

        private IEnumerator LerpToControlPose(GameObject objToLerp, Transform newParent, Vector3 endPosition, Quaternion endRotation, float duration)
        {
            // switch parent and wait a frame
            currentManipulator.transform.parent = newParent;
            if (endPosition == Vector3.zero)
            {
                objToLerp.transform.localScale = Vector3.one;
            }

            yield return new WaitForEndOfFrame();

            Transform t = objToLerp.transform;
            float time = 0;
            while (time < duration)
            {
                objToLerp.transform.localPosition = Vector3.Lerp(t.localPosition, endPosition, time / duration);
                objToLerp.transform.localRotation = Quaternion.Slerp(t.localRotation, endRotation, time / duration);

                time += Time.smoothDeltaTime;
                yield return null;
            }

            objToLerp.transform.localPosition = endPosition;
            objToLerp.transform.localRotation = endRotation;
        }
    }
}