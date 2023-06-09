using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using ICVR.SharedAssets;
using UnityEngine;
using WebXR;

namespace ICVR
{
    public class BodyController : MonoBehaviour
    {
        public static string CurrentUserId { get; private set; }
        public static int CurrentNoPeers { get; set; }

        [DllImport("__Internal")]
        private static extern void SendData(string msg);

        [SerializeField] private GameObject headObject;

        [SerializeField] private GameObject leftHand;
        [SerializeField] private GameObject rightHand;

        [SerializeField] private Transform leftPointer;
        [SerializeField] private Transform rightPointer;

        [SerializeField] private ChatController chatController;

        [SerializeField] private GameObject hudFollower;

        private bool IsConnectionReady = false;
        private bool hasInteractionEvent = false;

        private AvatarEventType currentEventType = AvatarEventType.None;
        private string currentEventData = "";
        private static float startTime = 0.0f;

        private void OnEnable()
        {
            Camera.main.gameObject.GetComponent<DesktopController>().OnNetworkInteraction += PackageEventData;
            leftHand.GetComponent<XRControllerInteraction>().OnHandInteraction += PackageEventData;
            rightHand.GetComponent<XRControllerInteraction>().OnHandInteraction += PackageEventData;
            WebXRManager.OnXRChange += OnXRChange;

            chatController.OnBroadcastMessage += PackageChatData;
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
            leftHand.GetComponent<XRControllerInteraction>().OnHandInteraction -= PackageEventData;
            rightHand.GetComponent<XRControllerInteraction>().OnHandInteraction -= PackageEventData;
            chatController.OnBroadcastMessage -= PackageChatData;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            headObject.transform.localRotation = Quaternion.identity;
            //bodyObject.transform.localRotation = Quaternion.identity;

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


            CurrentUserId = "Me";
            CurrentNoPeers = 0;
        }

        void Update()
        {
            if (hudFollower.activeInHierarchy)
            {
                hudFollower.transform.position = transform.position + headObject.transform.forward * 1.5f + headObject.transform.up * -0.6f;
                hudFollower.transform.rotation = Quaternion.LookRotation(headObject.transform.forward) * Quaternion.Euler(20, 0, 0);
            }

            if (!IsConnectionReady) return;

            // included 'home' for manual initialisation or debugging
            float frameTick = Time.time;
            if (Input.GetKeyUp(KeyCode.Home) || hasInteractionEvent)
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
            //Debug.Log("Dictionary changed. There are now " + numberofplayers + " players");
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

        private void PackageChatData(AvatarChatData chatData)
        {
            if (chatController != null)
            {
                chatController.UpdateChatFeed(CurrentUserId, chatData);
            }

            if (CurrentNoPeers < 1)
            {
                return;
            }

            currentEventType = AvatarEventType.Chat;
            currentEventData = JsonConvert.SerializeObject(chatData);
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