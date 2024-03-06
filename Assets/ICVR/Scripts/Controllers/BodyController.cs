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
        // Singleton pattern
        private static BodyController _instance;
        public static BodyController Instance { get { return _instance; } }

        public static string CurrentUserId { get; private set; }
        public static int CurrentNoPeers { get; set; }
        public float BodyMass { get; private set; }

        public delegate void CursorFocus(GameObject focalObject, bool state);
        public delegate void ObjectTrigger(GameObject focalObject, float value);
        public delegate void ObjectGrip(ControllerHand hand, GameObject focalObject, bool state);

        [DllImport("__Internal")]
        private static extern void SendData(string msg);

        [SerializeField] private GameObject headObject;

        [SerializeField] private XRController leftController;
        [SerializeField] private XRController rightController;

        [SerializeField] private Transform leftPointer;
        [SerializeField] private Transform rightPointer;

        private bool IsVR;

        private bool IsConnectionReady = false;
        private bool hasInteractionEvent = false;

        private AvatarEventType currentEventType = AvatarEventType.None;
        private string currentEventData = "";
        private static float startTime = 0.0f;

        private bool notifyingNetwork = false;

        private void OnEnable()
        {
            IsVR = (WebXRManager.Instance.XRState != WebXRState.NORMAL);
            WebXRManager.OnXRChange += OnXRChange;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            MapEvents(false);
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            //headObject.transform.localRotation = Quaternion.identity;

            // Turn off the following UI when in VR
            IsVR = (state == WebXRState.VR);

            // link controller events in VR
            MapControllerEvents(IsVR);
        }

        private void MapControllerEvents(bool isOn)
        {
            if (isOn)
            {
                rightController.OnHandFocus += HandleObjectFocus;
                leftController.OnHandFocus += HandleObjectFocus;

                rightController.OnObjectGrip += HandleObjectGrip;
                leftController.OnObjectGrip += HandleObjectGrip;

                rightController.OnObjectTrigger += HandleObjectTrigger;
                leftController.OnObjectTrigger += HandleObjectTrigger;

                leftController.OnHandInteraction += PackageEventData;
                rightController.OnHandInteraction += PackageEventData;
            }
            else
            {
                rightController.OnHandFocus -= HandleObjectFocus;
                leftController.OnHandFocus -= HandleObjectFocus;

                rightController.OnObjectGrip -= HandleObjectGrip;
                leftController.OnObjectGrip -= HandleObjectGrip;

                rightController.OnObjectTrigger -= HandleObjectTrigger;
                leftController.OnObjectTrigger -= HandleObjectTrigger;

                leftController.OnHandInteraction -= PackageEventData;
                rightController.OnHandInteraction -= PackageEventData;
            }
        }

        void MapEvents(bool isOn)
        {
            if (isOn)
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

                DesktopController.Instance.OnObjectFocus += HandleObjectFocus;
                DesktopController.Instance.OnObjectTrigger += HandleObjectTrigger;
                DesktopController.Instance.OnNetworkInteraction += PackageEventData;

            }
            else
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


                IsConnectionReady = false;

                DesktopController.Instance.OnObjectFocus -= HandleObjectFocus;
                DesktopController.Instance.OnNetworkInteraction -= PackageEventData;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            MapEvents(true);

#if UNITY_EDITOR
            // debugging option
            MapControllerEvents(true);
#endif

            CurrentUserId = "Me";
            CurrentNoPeers = 0;
        }

        void Update()
        {
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

        private void HandleObjectGrip(ControllerHand hand, GameObject controllable, bool state)
        {
            if (controllable != null && controllable.TryGetComponent(out ObjectInterface objInt))
            {
                objInt.SetGrip(GetHandController(hand), state);
            }
        }

        private void HandleObjectTrigger(GameObject controllable, float value)
        {
            if (controllable != null && controllable.TryGetComponent(out ObjectInterface objInt))
            {
                objInt.SetTrigger(value);
            }
        }

        private void HandleObjectFocus(GameObject go, bool state)
        {
            if (go != null && go.TryGetComponent(out ObjectInterface objInt))
            {
                objInt.ToggleActivation(go, state);
            }
        }

        private XRController GetHandController(ControllerHand hand)
        {
            return (hand == ControllerHand.RIGHT) ? rightController :
                (hand == ControllerHand.LEFT) ? leftController : null;
        }

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
                    LeftHandPosition = leftController.transform.position,
                    RightHandPosition = rightController.transform.position,
                    LeftHandRotation = leftController.transform.rotation,
                    RightHandRotation = rightController.transform.rotation,

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