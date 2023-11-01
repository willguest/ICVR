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
    /// <summary>
    /// A simplified version of 'Grabbable', for single handed interaction only. For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/ObjectInterface.md"/>
    /// </summary>
    public class ObjectInterface : MonoBehaviour
    {
        public bool IsBeingUsed { get; set; }

        [SerializeField] private Transform controlPoseLeft;
        [SerializeField] private Transform controlPoseRight;
        [SerializeField] private string gripPose;

        [SerializeField] private UnityEvent OnGetFocusEvent;
        [SerializeField] private UnityEvent OnLoseFocusEvent;
        [SerializeField] private UnityEvent OnTriggerEvent;

        private Transform previousParent;
        private GameObject currentManipulator;

        public void ToggleActivation(GameObject manipulator, bool state)
        {
            if (manipulator == this.gameObject)
            {
                manipulator = DesktopController.Instance.gameObject;
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
            
            //IsBeingUsed = state;
        }

        public void OnTrigger()
        {
            if (IsBeingUsed)
            {
                OnTriggerEvent?.Invoke();
            }
        }



        private void OnTriggerEnter(Collider other)
        {
            if (IsBeingUsed) { return; }

            if (other.gameObject.layer == 15) // tools
            {
                ToggleActivation(other.gameObject, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsBeingUsed) { return; }

            if (other.gameObject.layer == 15)
            {
                ToggleActivation(null, false);
            }
        }

        private bool ReceiveControl(GameObject manipulator)
        {
            previousParent = manipulator.transform;

            // hand-based control
            if (manipulator.TryGetComponent(out XRController xrctrl))
            {
                // compatibility checks
                if (xrctrl.IsUsingInterface) return false;
                
                if (xrctrl.IsControllingObject) return false;

                Transform activeControlPose = controlPoseRight;
                if (manipulator.name.ToLower().Contains("left") || controlPoseRight == null)
                {
                    activeControlPose = (controlPoseLeft ? controlPoseLeft : null);
                }

                if (activeControlPose == null) return false; 

                // send grip update, if it exists
                if (!string.IsNullOrEmpty(gripPose))
                {
                    xrctrl.SetGripPose(gripPose);
                }

                xrctrl.IsUsingInterface = true;
                currentManipulator = manipulator.transform.Find("model")?.gameObject;
                
                // disable hand colliders, to prevent interference with object colliders and rigidbodies
                foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
                {
                    cc.enabled = false;
                }

                currentManipulator.transform.parent = gameObject.transform;
                StartCoroutine(LerpToControlPose(currentManipulator, activeControlPose.localPosition, activeControlPose.localRotation, 0.4f));
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

            if (gameObject.TryGetComponent(out ControlDynamics cd))
            {
                cd.ResetPose();
            }

            if (previousParent.TryGetComponent(out XRController xrc))
            {
                if (!xrc.IsUsingInterface) return false;

                currentManipulator.transform.parent = previousParent.transform;
                xrc.SetGripPose("relax");
                xrc.IsUsingInterface = false;

                StartCoroutine(LerpToControlPose(currentManipulator, Vector3.zero, Quaternion.identity, 0.4f));

                // re-enable hand colliders
                foreach (CapsuleCollider cc in currentManipulator.GetComponentsInChildren<CapsuleCollider>())
                {
                    cc.enabled = true;
                }
            }

            if (currentManipulator.TryGetComponent(out DesktopController dtc))
            {
                // desktop-specific, object-agnostic 'lose focus' actions
            }
            
            currentManipulator = null;
            return true;
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