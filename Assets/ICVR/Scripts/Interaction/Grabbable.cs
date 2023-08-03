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
    /// This component allows object that are grabbed by the user to modify the hand pose. 
    /// e.g. bowling balls, baseball bats or pool cues (2-handed). For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Interaction/Grabbable.md"/>
    /// </summary>
    public class Grabbable : MonoBehaviour
    {
        [Tooltip("The pose (position and rotation) of the left hand, when held.")]
        [SerializeField] private Transform controlPoseLeft;
        [Tooltip("The pose (position and rotation) of the right hand, when held.")]
        [SerializeField] private Transform controlPoseRight;

        [Tooltip("Name of the condition (in the animator) that identifies the hand transition")]
        [SerializeField] private string primaryHandPose;
        [Tooltip("Not yet active. To be added soon")]
        [SerializeField] private string secondHandPose;

        public delegate void SecondHandGrab(ControllerHand hand, Transform thisHand, Transform otherHand);
        public event SecondHandGrab OnSecondHand;

        public delegate void SecomdHandDrop(bool isTopHand);
        public event SecomdHandDrop OffSecondHand;

        public ControllerHand WieldState { get; private set; }

        private Transform HandTracePrimary;
        private Transform HandTraceSecondary;

        // layer management
        private string myLayer;

        private void OnEnable()
        {
            myLayer = LayerMask.LayerToName(gameObject.layer);
        }
        public bool BeGrabbed(ControllerHand hand, Transform handTransform)
        {
            switch (WieldState)
            {
                case ControllerHand.NONE:
                    {
                        WieldState = hand;
                        HandTracePrimary = handTransform;
                        SetLayerRecursively(handTransform.gameObject, LayerMask.NameToLayer("Body"));
                        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Tools"));
                        return false;
                    }
                case ControllerHand.LEFT:
                case ControllerHand.RIGHT:
                    {
                        if (hand != WieldState)
                        {
                            Transform cPose = (WieldState == ControllerHand.RIGHT) ? controlPoseRight : controlPoseLeft;

                            HandTraceSecondary = handTransform;
                            WieldState = ControllerHand.BOTH;
                            OnSecondHand?.Invoke(hand, handTransform, HandTracePrimary);
                            SetLayerRecursively(handTransform.gameObject, LayerMask.NameToLayer("Body"));
                            HandTracePrimary.GetComponent<XRController>().ModifyJoint(0);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public bool Disengage(ControllerHand hand, Transform handTransform)
        {
            ControllerHand otherHand = (hand == ControllerHand.RIGHT) ? ControllerHand.LEFT : ControllerHand.RIGHT;
            Transform cPose = (hand == ControllerHand.RIGHT) ? controlPoseRight : controlPoseLeft;

            StartCoroutine(SetLayerAfterDelay(0.1f, handTransform.gameObject, LayerMask.NameToLayer("Tools")));

            switch (WieldState)
            {
                case ControllerHand.LEFT:
                case ControllerHand.RIGHT:
                    {
                        if (hand == WieldState)
                        {
                            WieldState = ControllerHand.NONE;
                            StartCoroutine(SetLayerAfterDelay(0.1f, gameObject, LayerMask.NameToLayer(myLayer)));
                            return false;
                        }
                        else
                        {
                            WieldState = otherHand;
                            return false;
                        }
                    }
                case ControllerHand.BOTH:
                    {
                        if (handTransform != HandTracePrimary) // secondary hand lift-off
                        {
                            WieldState = otherHand;
                            OffSecondHand?.Invoke(true);
                            return true;
                        }
                        else                            // primary hand drop
                        {
                            WieldState = ControllerHand.NONE;
                            StartCoroutine(SetLayerAfterDelay(0.1f, HandTraceSecondary.gameObject, LayerMask.NameToLayer("Tools")));

                            StartCoroutine(SetLayerAfterDelay(0.1f, gameObject, LayerMask.NameToLayer(myLayer)));
                            OffSecondHand?.Invoke(false);
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public void BeginAttraction(ControllerHand hand, Transform handTransform, System.Action<Grabbable> callback)
        {
            Transform cPose = (hand == ControllerHand.RIGHT) ? controlPoseRight : controlPoseLeft;
            Quaternion targetRot = handTransform.rotation * cPose.localRotation;
            StartCoroutine(OrientToHand(targetRot, 1.0f, callback));
        }

        public string AttractObject(ControllerHand hand, Transform handTransform, System.Action callback)
        {
            Transform cPose = (hand == ControllerHand.RIGHT) ? controlPoseRight : controlPoseLeft;
            StartCoroutine(LerpToGrabPose(handTransform, cPose, 0.5f, callback));
            return primaryHandPose;
        }

        private IEnumerator OrientToHand(Quaternion targetRotation, float duration, System.Action<Grabbable> callback)
        {
            yield return new WaitForEndOfFrame();

            Quaternion startQuat = transform.rotation;
            float elapsedTime = 0;
            float ratio = elapsedTime / duration;

            while (ratio < 1.0f)
            {
                elapsedTime += Time.deltaTime;
                ratio = elapsedTime / duration;

                transform.rotation = Quaternion.Slerp(startQuat, targetRotation, ratio);
                yield return null;
            }

            transform.rotation = targetRotation;
            callback(this);
        }

        private IEnumerator SetLayerAfterDelay(float delay, GameObject obj, int newLayer)
        {
            yield return new WaitForSeconds(delay);
            SetLayerRecursively(obj, newLayer);
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

        private IEnumerator LerpToGrabPose(Transform handTransform, Transform controlPose, float duration, System.Action callback)
        {
            yield return new WaitForEndOfFrame();

            Vector3 startPos = transform.position;
            Quaternion startQuat = transform.rotation;

            Vector3 poseOffset = controlPose.localPosition;
            Quaternion poseRot = controlPose.localRotation;

            float time = 0f;
            float ratio = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                ratio = time / duration;

                Quaternion targetRot = handTransform.rotation * poseRot;
                Vector3 targetPos = handTransform.position +
                (handTransform.forward * poseOffset.z) +
                (handTransform.right * poseOffset.x) +
                (handTransform.up * poseOffset.y);

                transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0.0f, 1.0f, ratio));
                transform.rotation = Quaternion.Slerp(startQuat, targetRot, ratio);

                yield return null;
            }

            callback();
        }
    }
}