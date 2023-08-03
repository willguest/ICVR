/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using ICVR.SharedAssets;
using UnityEngine;
using WebXR;

namespace ICVR
{
    /// <summary>
    /// The BodyController is the meeting point for all data and where they get packaged and sent across the network. 
    /// <para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Controllers/BodyController.md"/>
    /// </summary>
    public class BodyController : MonoBehaviour
    {
        public static string CurrentUserId { get; private set; }
        public static int CurrentNoPeers { get; set; }
        public float BodyMass { get; private set; }

        [DllImport("__Internal")]
        private static extern void SendData(string msg);

        [SerializeField] private GameObject headObject;

        [SerializeField] private GameObject leftHand;
        [SerializeField] private GameObject rightHand;

        [SerializeField] private Transform leftPointer;
        [SerializeField] private Transform rightPointer;

        [SerializeField] private GameObject hudFollower;



        private bool IsConnectionReady = false;
        private bool hasInteractionEvent = false;

        private Pose UiStartPos;

        private AvatarEventType currentEventType = AvatarEventType.None;
        private string currentEventData = "";
        private static float startTime = 0.0f;

        private void OnEnable()
        {
            Camera.main.gameObject.GetComponent<DesktopController>().OnNetworkInteraction += PackageEventData;
            leftHand.GetComponent<XRController>().OnHandInteraction += PackageEventData;
            rightHand.GetComponent<XRController>().OnHandInteraction += PackageEventData;
            WebXRManager.OnXRChange += OnXRChange;

        }

        private void OnDisable()
        {
            if (AvatarManager.Instance)
            {
                AvatarManager.Instance.OnDictionaryChanged -= playersChanged;
            }
            if (NetworkIO.Instance)
            {
                NetworkIO.Instance.OnConnectionChanged -= SetConnectionReady;
                NetworkIO.Instance.OnJoinedRoom -= InitialiseDataChannel;
            }
            WebXRManager.OnXRChange -= OnXRChange;

            IsConnectionReady = false;

            if (Camera.main)
            {
                Camera.main.gameObject.GetComponent<DesktopController>().OnNetworkInteraction -= PackageEventData;
            }
            leftHand.GetComponent<XRController>().OnHandInteraction -= PackageEventData;
            rightHand.GetComponent<XRController>().OnHandInteraction -= PackageEventData;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            headObject.transform.localRotation = Quaternion.identity;
            //bodyObject.transform.localRotation = Quaternion.identity;

            // Turn off the following UI when in VR.
            hudFollower.SetActive(state == WebXRState.NORMAL);
        }

        void Start()
        {
            if (AvatarManager.Instance)
            {
                AvatarManager.Instance.OnDictionaryChanged += playersChanged;
            }
            if (NetworkIO.Instance)
            {
                NetworkIO.Instance.OnConnectionChanged += SetConnectionReady;
                NetworkIO.Instance.OnJoinedRoom += InitialiseDataChannel;
            }

            UiStartPos.position = hudFollower.transform.position;
            UiStartPos.rotation = hudFollower.transform.rotation;

            CurrentUserId = "Me";
            CurrentNoPeers = 0;
        }

        void Update()
        {
            // set position of following UI
            if (hudFollower.activeInHierarchy)
            {
                hudFollower.transform.position = transform.position + headObject.transform.forward * UiStartPos.position.z + headObject.transform.up * UiStartPos.position.y + headObject.transform.right * UiStartPos.position.x; 
                hudFollower.transform.rotation = Quaternion.LookRotation(headObject.transform.forward) * UiStartPos.rotation;
            }

            if (!IsConnectionReady) return;


            float frameTick = Time.time;
            if (hasInteractionEvent)
            {
                startTime = frameTick + 0.25f; 
                SendData(JsonConvert.SerializeObject(BuildDataFrame()));
                
                hasInteractionEvent = false;
                currentEventData = "";
            }
            else if ((frameTick - startTime) > 0.25f)
            {
                startTime = frameTick;
                if (CurrentNoPeers > 0)
                {
                    SendData(JsonConvert.SerializeObject(BuildDataFrame()));
                }
            }
        }

        private bool notifyingNetwork = false;

        public void InitialiseDataChannel(string userid = "")
        {
            if (!notifyingNetwork)
            {
                StartCoroutine(StartAfterDelay(2.0f));
            }
        }

        private IEnumerator StartAfterDelay(float delay)
        {
            notifyingNetwork = true;
            yield return new WaitForSeconds(delay);

            SetConnectionReady(true);
            notifyingNetwork = false;
        }

        private void playersChanged(int numberofplayers)
        {
            //Debug.Log("There are now " + numberofplayers + " peers");
            CurrentNoPeers = numberofplayers;

            // send a packet to start communication
            if (CurrentNoPeers > 0)
            {
                hasInteractionEvent = true;
            }
        }

        private void SetConnectionReady(bool newState)
        {
            CurrentUserId = NetworkIO.Instance.CurrentUserId;

            // force send message on next frame
            IsConnectionReady = newState;
            hasInteractionEvent = newState;
        }

        private void PackageEventData(AvatarHandlingData ahdFrame)
        {
            if (CurrentNoPeers < 1)
            {
                return;
            }

            currentEventType = AvatarEventType.Interaction;
            currentEventData = JsonConvert.SerializeObject(ahdFrame);
            hasInteractionEvent = true;
        }

        private NodeDataFrame BuildDataFrame()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                NodeDataFrame dataToSend = new NodeDataFrame
                {
                    Id = CurrentUserId,
                    HeadPosition = headObject.transform.position,
                    HeadRotation = headObject.transform.rotation,
                    LeftHandPosition = leftHand.transform.position,
                    RightHandPosition = rightHand.transform.position,
                    LeftHandRotation = leftHand.transform.rotation,
                    RightHandRotation = rightHand.transform.rotation,

                    LeftHandPointer = leftPointer.position,
                    RightHandPointer = rightPointer.position,

                    EventType = currentEventType,
                    EventData = currentEventData
                };

                return dataToSend;
            }
            else
            {
                return null;
            }
        }
    }
}