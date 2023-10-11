/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using WebXR;
using ICVR.SharedAssets;

#if UNITY_EDITOR || !UNITY_WEBGL
using UnityEngine.XR;
#endif

namespace ICVR
{
    /// <summary>
    /// Interaction handler for XR controllers on the web. Uses the thumbstick, grip and trigger buttons 
    /// to control the character, including jumping and swimming. Connect interaction events to the 
    /// virtual world's collision and data layers. Warning: modifying this without proper knowledge can 
    /// really mess things up.
    /// <para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Controllers/XRController.md"/>
    /// </summary>
    public class XRController : MonoBehaviour
    {
        [SerializeField] private bool debugHand;
        [SerializeField] private GameObject CharacterRoot;
        [SerializeField] private BodyController BodyController;

        [SerializeField] private float MaxInteractionDistance = 15.0f;
        [SerializeField] private Renderer[] GameObjectRenderers;

        [SerializeField] private ControllerHand hand = ControllerHand.NONE;
        [SerializeField] LayerMask PointerLayerMask;


        #region Public Variables, Functions and Attributes


        public enum ButtonTypes
        {
            Trigger = 0,
            Grip = 1,
            Thumbstick = 2,
            Touchpad = 3,
            ButtonA = 4,
            ButtonB = 5
        }
        public enum AxisTypes
        {
            Trigger,
            Grip
        }
        public enum Axis2DTypes
        {
            Thumbstick, // primary2DAxis
            Touchpad  // secondary2DAxis
        }

        public Action<bool> OnControllerActive;
        public Action<bool> OnHandActive;
        public Action<WebXRHandData> OnHandUpdate;

        
        // flag used by ObjectInterface - used when the hand is bound to an object
        public bool IsUsingInterface { private get; set; }

        // flag set by ControlDynamics - used when the controller is articulating equipment
        public bool IsControllingObject { get; set; }

        public void SetGripPose(string gripPose)
        {
            animTrigger = gripPose;
            anim.SetTrigger(animTrigger);
        }

        public void ModifyJoint(int jointIndex, Rigidbody connectedBody = null)
        {
            attachJoint[jointIndex].connectedBody = connectedBody;
        }

        // events, use as hooks for controller button functions
        public delegate void ButtonPressed(float buttonValue);
        public event ButtonPressed AButtonEvent;
        public event ButtonPressed BButtonEvent;

        public delegate void HandInteraction(AvatarHandlingData interactionData);
        public event HandInteraction OnHandInteraction;


        #endregion Public Variables, Functions and Attributes


        #region Private Variables

        private float trigger;
        private float squeeze;
        private float thumbstick;
        private float thumbstickX;
        private float thumbstickY;
        private float touchpad;
        private float touchpadX;
        private float touchpadY;
        private float buttonA;
        private float buttonB;

        private Dictionary<ButtonTypes, WebXRControllerButton> buttonStates = new Dictionary<ButtonTypes, WebXRControllerButton>();

        private bool controllerActive = false;
        private bool handActive = false;

        private WebXRState xrState = WebXRState.NORMAL;

        private FixedJoint[] attachJoint;

        private Rigidbody currentNearRigidBody = null;
        private Rigidbody currentFarRigidBody = null;
        private List<Rigidbody> nearcontactRigidBodies = new List<Rigidbody>();
        private List<Rigidbody> farcontactRigidBodies = new List<Rigidbody>();

        [SerializeField] private Animator anim;

        private SharedAsset currentNearSharedAsset;
        private SharedAsset currentFarSharedAsset;

        private GameObject currentObject;
        private GameObject myPointer;

        private string prevMeshName = "";

        float trigThresUp = 0.90f;
        float trigThresDn = 0.10f;
        float gripThresUp = 0.90f;
        float gripThresDn = 0.10f;

        float rightThumbstickThreshold = 0.9f;
        float thAT = 0.25f;     // thumbstick absolute threshold

        private float prevRightThX = 0f;
        private float prevTrig = 0f;
        private float prevGrip = 0f;

        private bool touchingButton;
        private bool pointingAtButton;

        private int playersPresent = 0;

        private GameObject currentButton;

        // Object Handling
        private bool distanceManip = false;
        private bool nearManip = false;

        private string animTrigger = "";

        private float actionTick = 0f;
        private float triggerEnterTick = 0f;
        private float triggerExitTick = 0f;

        private bool IsSwimming;
        private float currentElevation;
        private float jumpTick;
        private float seaLevel = -4.5f;

        private WebXRCamera xrCameras;
        private Camera vrGuideCam;


        #endregion Private Variables


        #region Unity Functions

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnControllerUpdate += OnControllerUpdate;
            WebXRManager.OnHandUpdate += OnHandUpdateInternal;

            SetControllerActive(false);
            SetHandActive(false);
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnControllerUpdate -= OnControllerUpdate;
            WebXRManager.OnHandUpdate -= OnHandUpdateInternal;

            SetControllerActive(false);
            SetHandActive(false);
        }


        void Start()
        {
            attachJoint = new FixedJoint[] { GetComponents<FixedJoint>()[0], GetComponents<FixedJoint>()[1] };
            myPointer = transform.Find("pointer").gameObject;

            xrCameras = CharacterRoot.GetComponent<WebXRCamera>();
            vrGuideCam = xrCameras.GetCamera(WebXRCamera.CameraID.LeftVR);

            if (!debugHand)
            {
                ToggleRenderers(xrState == WebXRState.VR);
            }

            jumpTick = Time.time;
        }

        void FixedUpdate()
        {
            // only do this in a webxr session
            if (xrState != WebXRState.VR && debugHand == false) { return; }

            DetectSwimming();

            // left stick controls movement
            if (hand == ControllerHand.LEFT)
            {
                if (Math.Abs(thumbstickX) > thAT || Math.Abs(thumbstickY) > thAT)
                {
                    MoveBodyWithJoystick(thumbstickX, thumbstickY, 2.0f);
                }
            }
            // right stick turns in 60 degree steps and forwards-backwards
            else if (hand == ControllerHand.RIGHT)
            {
                if (Mathf.Abs(thumbstickX) > rightThumbstickThreshold && prevRightThX <= rightThumbstickThreshold)
                {
                    RotateWithJoystick(thumbstickX);
                }
                prevRightThX = Mathf.Abs(thumbstickX);

                if (Math.Abs(thumbstickY) > thAT)
                {
                    MoveBodyWithJoystick(0.0f, thumbstickY, 2.0f);
                }
            }

            // trigger effects, either for distance interaction or events using ObjectInterface
            float trigVal = GetAxis(AxisTypes.Trigger);

            if (trigVal > trigThresUp && prevTrig <= trigThresUp)
            {
                if (IsUsingInterface && currentNearRigidBody)
                {
                    if (currentNearRigidBody.TryGetComponent(out ObjectInterface objInt))
                    {
                        objInt.OnTrigger();
                    }
                }
                else
                {
                    PickupFar();
                }
                
            }
            else if (trigVal < trigThresDn && prevTrig >= trigThresDn)
            {
                if (distanceManip)
                {
                    DropFar();
                }
                else if (currentButton != null)
                {
                    DropFar();
                }
            }

            // grip for near interaction only
            float gripVal = GetAxis(AxisTypes.Grip);
            if (gripVal > gripThresUp && prevGrip <= gripThresUp)
            {
                PickupNear();
            }
            else if (gripVal < gripThresDn && prevGrip >= gripThresDn)
            {
                DropNear();
            }

            if (thumbstick == 1)
            {
                JumpSwim();
            }

            // isusinginterface interface..
            if (IsUsingInterface)
            {
                if (GetButtonDown(ButtonTypes.ButtonA))
                {
                    AButtonEvent.Invoke(1.0f);
                }
                else if (GetButtonUp(ButtonTypes.ButtonA))
                {
                    AButtonEvent.Invoke(0.0f);
                }

                if (GetButtonDown(ButtonTypes.ButtonB))
                {
                    BButtonEvent.Invoke(1.0f);
                }
                else if (GetButtonUp(ButtonTypes.ButtonB))
                {
                    BButtonEvent.Invoke(0.0f);
                }
            }

            prevTrig = trigVal;
            prevGrip = gripVal;

            // set pointers
            PlacePointer();
            SetActiveFarMesh();
        }

#if UNITY_EDITOR || !UNITY_WEBGL
        private InputDevice? inputDevice;
        private int buttonsFrameUpdate = -1;

        private void LateUpdate()
        {
            TryUpdateButtons();
        }
#endif


        #endregion Unity Functions


        #region State Functions

        private void ToggleRenderers(bool onOrOff)
        {
            foreach (Renderer r in GameObjectRenderers)
            {
                r.enabled = onOrOff;
            }
        }

        private void OnHandUpdateInternal(WebXRHandData handData)
        {
            if (handData.hand == (int)hand)
            {
                if (!handData.enabled)
                {
                    SetHandActive(false);
                    return;
                }
                SetControllerActive(false);
                SetHandActive(true);

                transform.localPosition = handData.joints[0].position;
                transform.localRotation = handData.joints[0].rotation;

                trigger = handData.trigger;
                squeeze = handData.squeeze;

                OnHandUpdate?.Invoke(handData);
            }
        }

        private void SetControllerActive(bool active)
        {
            if (controllerActive != active)
            {
                controllerActive = active;
                OnControllerActive?.Invoke(controllerActive);
            }
        }

        private void SetHandActive(bool active)
        {
            if (handActive == active)
            {
                return;
            }
            handActive = active;
            OnHandActive?.Invoke(handActive);
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            ToggleRenderers(xrState == WebXRState.VR);
        }

        private void DetectSwimming()
        {
            float elevation = CharacterRoot.transform.position.y;

            if (elevation < seaLevel && currentElevation >= seaLevel)
            {
                IsSwimming = true;
                CharacterRoot.GetComponent<Rigidbody>().mass = 2.0f;
            }
            else if (elevation > seaLevel && currentElevation <= seaLevel)
            {
                IsSwimming = false;
                CharacterRoot.GetComponent<Rigidbody>().mass = 70.0f;
            }
            currentElevation = elevation;
        }

        private void JumpSwim()
        {
            if (IsSwimming && Time.time - jumpTick > 0.2f)
            {
                jumpTick = Time.time;
                Vector3 swimForce = new Vector3(0f, 150f, 0f) + (CharacterRoot.transform.forward * 50f);
                Debug.Log("stroke");
                CharacterRoot.GetComponent<Rigidbody>().AddForce(swimForce, ForceMode.Acceleration);
            }
            else if (Time.time - jumpTick > 2.0f)
            {
                jumpTick = Time.time;
                Vector3 jumpForce = new Vector3(0f, 350f, 0f) + (CharacterRoot.transform.forward * 50f);
                Debug.Log("hop");
                CharacterRoot.GetComponent<Rigidbody>().AddForce(jumpForce, ForceMode.Impulse);
            }
        }



        #endregion State Functions


        #region Button Functionality


        private void TryUpdateButtons()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            if (buttonsFrameUpdate == Time.frameCount)
            {
                return;
            }
            buttonsFrameUpdate = Time.frameCount;
            if (!WebXRManager.Instance.isSubsystemAvailable && inputDevice != null)
            {
                inputDevice.Value.TryGetFeatureValue(CommonUsages.trigger, out trigger);
                inputDevice.Value.TryGetFeatureValue(CommonUsages.grip, out squeeze);
                if (trigger <= 0.02)
                {
                    trigger = 0;
                }
                else if (trigger >= 0.98)
                {
                    trigger = 1;
                }

                if (squeeze <= 0.02)
                {
                    squeeze = 0;
                }
                else if (squeeze >= 0.98)
                {
                    squeeze = 1;
                }

                Vector2 axis2D;
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D))
                {
                    thumbstickX = axis2D.x;
                    thumbstickY = axis2D.y;
                }
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.secondary2DAxis, out axis2D))
                {
                    touchpadX = axis2D.x;
                    touchpadY = axis2D.y;
                }
                bool buttonPressed;
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out buttonPressed))
                {
                    thumbstick = buttonPressed ? 1 : 0;
                }
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out buttonPressed))
                {
                    touchpad = buttonPressed ? 1 : 0;
                }
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed))
                {
                    buttonA = buttonPressed ? 1 : 0;
                }
                if (inputDevice.Value.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonPressed))
                {
                    buttonB = buttonPressed ? 1 : 0;
                }
            }
#endif
        }

        private void OnControllerUpdate(WebXRControllerData controllerData)
        {
            if (controllerData.hand == (int)hand)
            {
                if (!controllerData.enabled)
                {
                    SetControllerActive(false);
                    return;
                }

                transform.localRotation = controllerData.rotation;
                transform.localPosition = controllerData.position;

                trigger = controllerData.trigger;
                squeeze = controllerData.squeeze;
                thumbstick = controllerData.thumbstick;
                thumbstickX = controllerData.thumbstickX;
                thumbstickY = controllerData.thumbstickY;
                touchpad = controllerData.touchpad;
                touchpadX = controllerData.touchpadX;
                touchpadY = controllerData.touchpadY;
                buttonA = controllerData.buttonA;
                buttonB = controllerData.buttonB;

                SetControllerActive(true);
            }
        }

        private float GetAxis(AxisTypes action)
        {
            TryUpdateButtons();
            switch (action)
            {
                case AxisTypes.Grip:
                    return squeeze;
                case AxisTypes.Trigger:
                    return trigger;
            }
            return 0;
        }

        private bool GetButtonDown(ButtonTypes action)
        {
            TryUpdateButtons();
            if (!buttonStates.ContainsKey(action))
            {
                return false;
            }
            return buttonStates[action].down;
        }

        private bool GetButtonUp(ButtonTypes action)
        {
            TryUpdateButtons();
            if (!buttonStates.ContainsKey(action))
            {
                return false;
            }
            return buttonStates[action].up;
        }


        #endregion Controller Functionality


        #region Character Movement

        private void MoveBodyWithJoystick(float xax, float yax, float multiplier = 1.0f)
        {
            float x = xax * Time.deltaTime;
            float z = yax * Time.deltaTime;
            Camera referenceCam = vrGuideCam;

            // get camera-aligned directions
            Vector3 worldForward = CharacterRoot.transform.InverseTransformDirection(referenceCam.transform.forward);
            Vector3 worldRight = CharacterRoot.transform.InverseTransformDirection(referenceCam.transform.right);

            // flatten and normalise
            worldForward.y = 0;
            worldRight.y = 0;
            worldForward.Normalize();
            worldRight.Normalize();

            //direction in world space we want to move
            Vector3 desiredMoveDirection = worldForward * z + worldRight * x;
            CharacterRoot.transform.Translate(desiredMoveDirection * multiplier);
        }

        private void RotateWithJoystick(float value)
        {
            if (value == 0) { return; }

            if (value > 0)
            {
                CharacterRoot.transform.RotateAround(CharacterRoot.transform.position, Vector3.up, 60);
            }
            else
            {
                CharacterRoot.transform.RotateAround(CharacterRoot.transform.position, Vector3.up, -60);
            }
        }


        #endregion Character Movement


        #region Interaction Functions

        public void PickupFar()
        {
            // button actions take priority
            if (touchingButton || pointingAtButton)
            {
                if (currentButton.TryGetComponent(out PressableButton pba) && (Time.time - actionTick) > 0.5f)
                {
                    actionTick = Time.time;
                    pba.ButtonPressed.Invoke();
                }
                return;
            }        

            // stop here if not handling a moveable object
            currentFarRigidBody = GetDistantRigidBody();
            if (!currentFarRigidBody) return;

            GameObject currentFarObject = currentFarRigidBody.gameObject;

            // skip near-interaction-only objects
            if (currentFarObject.GetComponent<ControlDynamics>()) { return; }

            // pick up
            currentFarRigidBody.MovePosition(transform.position);
            attachJoint[1].connectedBody = currentFarRigidBody;
            distanceManip = true;

            // identify shared asset and assign currentTargetId
            SharedAsset sharedAsset = currentFarObject.GetComponent<SharedAsset>();
            if (sharedAsset != null)
            {
                if (!sharedAsset.IsBeingHandled)
                {
                    currentFarSharedAsset = sharedAsset;
                    currentFarSharedAsset.IsBeingHandled = true;

                    InvokeAcquisitionEvent(currentFarSharedAsset.Id, currentFarRigidBody.gameObject.transform, ManipulationDistance.Far);
                }
                else
                {
                    // asset is being used
                    Debug.Log("This object is being used by someone else. Please pester them until they let you play with it.");
                }
            }

            anim.SetTrigger("pointAtIt");
        }


        public void DropFar()
        {
            if (touchingButton || pointingAtButton)
            {
                if (currentButton.TryGetComponent(out PressableButton pba))
                {
                    pba.ButtonReleased.Invoke();
                }
                return;
            }

            // if no object sensed, stop 
            if (!currentFarRigidBody) return;

            ThrowData newThrow;
            attachJoint[1].connectedBody = null;
            farcontactRigidBodies.Clear();
            distanceManip = false;

            if (currentFarRigidBody.gameObject.TryGetComponent(out RigidDynamics rd))
            {
                currentFarRigidBody.useGravity = rd.UsesGravity;
                newThrow = rd.Throw;
            }
            else
            {
                newThrow = new ThrowData()
                {
                    LinearForce = currentFarRigidBody.velocity,
                    AngularForce = currentFarRigidBody.angularVelocity
                };
            }

            currentFarRigidBody.AddForce(newThrow.LinearForce, ForceMode.Impulse);
            currentFarRigidBody.AddTorque(newThrow.AngularForce, ForceMode.Impulse);

            anim.SetTrigger("relax");

            // network release
            if (currentFarSharedAsset != null)
            {
                currentFarSharedAsset.IsBeingHandled = false;
                InvokeReleaseEvent(currentFarSharedAsset.Id, currentFarRigidBody.gameObject, ManipulationDistance.Far, newThrow);
                currentFarSharedAsset = null;
            }

            currentFarRigidBody = null;
        }

        private void BeginAttractFar(Rigidbody targetRB)
        {
            if (targetRB.gameObject.TryGetComponent(out Grabbable grabber))
            {
                currentFarRigidBody.useGravity = false;
                attachJoint[1].connectedBody = null;

                grabber.BeginAttraction(hand, transform, AttractFar);

            }
        }

        public void AttractFar(Grabbable sender)
        {
            if (currentFarRigidBody && prevGrip > 0.8f)
            {
                gameObject.GetComponentInChildren<MeshCollider>().enabled = false;

                // local release far
                farcontactRigidBodies.Clear();
                currentFarRigidBody = null;
                distanceManip = false;

                // network release far
                if (currentFarSharedAsset)
                {
                    currentFarSharedAsset.IsBeingHandled = false;
                    InvokeReleaseEvent(currentFarSharedAsset.Id, sender.gameObject, ManipulationDistance.Far, new ThrowData());
                }

                // use the force
                animTrigger = sender.AttractObject(hand, transform, PickupNear);
            }
            else
            {
                DropFar();
            }
        }


        public void PickupNear()
        {
            currentNearRigidBody = GetNearRigidBody();

            if (!currentNearRigidBody)
            {
                if (currentFarRigidBody != null)
                {
                    BeginAttractFar(currentFarRigidBody);
                    return;
                }
                else
                {
                    return;
                }
            }

            GameObject currentNearObject = currentNearRigidBody.gameObject;

            if (currentNearObject.TryGetComponent(out Grabbable g))
            {
                if (g.BeGrabbed(hand, transform))
                {
                    return;
                }
            }

            currentNearRigidBody.MovePosition(transform.position);
            attachJoint[0].connectedBody = currentNearRigidBody;
            currentNearRigidBody.isKinematic = false;

            // determine if controlling a fixed object
            if (currentNearObject.GetComponent<ControlDynamics>())
            {
                IsControllingObject = true;
            }

            if (currentNearObject.TryGetComponent(out SharedAsset sharedAsset))
            {
                if (!sharedAsset.IsBeingHandled)
                {
                    currentNearSharedAsset = sharedAsset;
                    currentNearSharedAsset.IsBeingHandled = true;
                    InvokeAcquisitionEvent(currentNearSharedAsset.Id, currentNearObject.transform, ManipulationDistance.Near);
                }
                else
                {
                    // asset is being used by someone else
                    Debug.Log("This object is being used by someone else");
                }
            }

            nearManip = true;

            if (!string.IsNullOrEmpty(animTrigger))
            {
                anim.SetTrigger(animTrigger);
            }
            else
            {
                anim.SetTrigger("holdIt");
            }

            nearManip = true;
        }

        public void DropNear()
        {
            nearcontactRigidBodies.Clear();

            if (!currentNearRigidBody) return;

            if (currentNearRigidBody.gameObject.TryGetComponent(out Grabbable g))
            {
                if (g.Disengage(hand, transform))
                {
                    return;
                }
            }

            ThrowData newThrow;
            attachJoint[0].connectedBody = null;
            nearManip = false;

            // local throw
            if (currentNearRigidBody.gameObject.TryGetComponent(out RigidDynamics rd))
            {
                currentNearRigidBody.useGravity = rd.UsesGravity;
                newThrow = rd.Throw;
            }
            else
            {
                newThrow = new ThrowData() { 
                    LinearForce = currentNearRigidBody.velocity, 
                    AngularForce = currentNearRigidBody.angularVelocity 
                };
            }

            currentNearRigidBody.AddForce(newThrow.LinearForce, ForceMode.Impulse);
            currentNearRigidBody.AddTorque(newThrow.AngularForce, ForceMode.Impulse);

            if (IsControllingObject)
            {
                currentNearRigidBody.gameObject.GetComponent<ControlDynamics>().FinishInteraction();
                IsControllingObject = false;
                IsUsingInterface = false;
            }

            // reset and forget
            animTrigger = "";
            anim.SetTrigger("relax");

            // network throw
            if (currentNearSharedAsset != null)
            {
                InvokeReleaseEvent(currentNearSharedAsset.Id, currentNearRigidBody.gameObject, ManipulationDistance.Near, newThrow);
                currentNearSharedAsset.IsBeingHandled = false;
                currentNearSharedAsset = null;
            }

            currentNearRigidBody = null;
        }



        private bool InvokeAcquisitionEvent(string target, Transform interactionTransform, ManipulationDistance distance)
        {
            playersPresent = BodyController.CurrentNoPeers;

            // check for shared asset id
            if (!string.IsNullOrEmpty(target) && playersPresent > 0)
            {
                AcquireData newAcquisition = new AcquireData
                {
                    AcqTime = DateTime.UtcNow.Ticks,
                    ObjectPosition = interactionTransform.position,
                    ObjectRotation = interactionTransform.rotation
                };

                AvatarHandlingData interactionEvent = BuildEventFrame(target, distance, AvatarInteractionEventType.AcquireData, newAcquisition, null);
                OnHandInteraction?.Invoke(interactionEvent);
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool InvokeReleaseEvent(string target, GameObject interactionObject, ManipulationDistance distance, ThrowData throwData)
        {
            playersPresent = BodyController.CurrentNoPeers;

            // check for shared asset id
            if (!string.IsNullOrEmpty(target) && playersPresent > 0)
            {
                ReleaseData newRelease = new ReleaseData
                {
                    ReleaseTime = DateTime.UtcNow.Ticks,
                    ReleasePosition = interactionObject.transform.position,
                    ReleaseRotation = interactionObject.transform.rotation,
                    ForceData = throwData
                };

                AvatarHandlingData interactionEvent = BuildEventFrame(target, distance, AvatarInteractionEventType.ReleaseData, null, newRelease);
                OnHandInteraction.Invoke(interactionEvent);
                return true;
            }
            else
            {
                return false;
            }
        }

        private AvatarHandlingData BuildEventFrame(string targetId, ManipulationDistance distance,
            AvatarInteractionEventType eventType, AcquireData acqDataFrame = null, ReleaseData relDataFrame = null)
        {
            AvatarHandlingData eventFrame = new AvatarHandlingData
            {
                Hand = hand,
                TargetId = targetId,
                Distance = distance,
                EventType = eventType,
                AcquisitionEvent = acqDataFrame,
                ReleaseEvent = relDataFrame
            };
            return eventFrame;
        }


        #endregion Interaction Functions


        #region Low-Level Interaction Methods

        private void SetActiveFarMesh()
        {
            // only active when there is a focused object
            if (currentObject != null)
            {
                string meshName = currentObject.name;
                int layer = currentObject.layer;

                // layer-dependent actions
                if (layer == 10 || layer == 15)         //  interactable objects or tools
                {
                    pointingAtButton = false;
                    currentButton = null;

                    // must be solid and not near-only interaction
                    if (currentObject.GetComponent<Rigidbody>() && !GetComponent<ControlDynamics>())
                    {
                        if (meshName != prevMeshName)
                        {
                            farcontactRigidBodies.Clear();
                            farcontactRigidBodies.Add(currentObject.GetComponent<Rigidbody>());
                        }

                        // set pointer appearance for heavy objects
                        // ...
                    }
                }
                else if (layer == 12)       // buttons
                {
                    pointingAtButton = true;
                    currentButton = currentObject;

                    // set pointer appearance for buttons
                    // ...
                }
                else
                {
                    if (!distanceManip)
                    {
                        farcontactRigidBodies.Clear();
                        prevMeshName = "";
                    }
                }

                prevMeshName = meshName;
            }
            else
            {
                if (!distanceManip)
                {
                    farcontactRigidBodies.Clear();
                    prevMeshName = "";
                }
                pointingAtButton = false;
                currentButton = null;

                // set default pointer appearance 
                // ...
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag != "Interactable" || nearManip)
            { 
                return;
            }

            int objectLayer = other.gameObject.layer;

            if (objectLayer == 12) // buttons
            {
                touchingButton = true;
                currentButton = other.gameObject;

                if (other.gameObject.TryGetComponent(out PressableButton pb) &&
                    (Time.time - triggerEnterTick) > 0.2f)
                {
                    triggerEnterTick = Time.time;
                    pb.ButtonPressed?.Invoke();
                }

            }
            else if (objectLayer == 9 || objectLayer == 10 || objectLayer == 15) // furniture, objects or tools
            {
                if (other.gameObject.TryGetComponent(out Rigidbody rb) &&
                    (Time.time - triggerEnterTick) > 0.2f)
                {
                    triggerEnterTick = Time.time;
                    if (!nearcontactRigidBodies.Contains(rb))
                    {
                        nearcontactRigidBodies.Add(rb);
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag != "Interactable")
                return;

            int objectLayer = other.gameObject.layer;

            if (objectLayer == 12) // buttons
            {
                currentButton = null;
                touchingButton = false;

                if (other.gameObject.TryGetComponent(out PressableButton pb) &&
                    (Time.time - triggerEnterTick) > 0.2f)
                {
                    triggerEnterTick = Time.time;
                    pb.ButtonReleased?.Invoke();
                }

            }
            else if (objectLayer == 9 || objectLayer == 10 || objectLayer == 15) // furniture, objects or tools
            {
                if (other.gameObject.TryGetComponent(out Rigidbody rb)
                    && (Time.time - triggerExitTick) > 0.2f)
                {
                    triggerExitTick = Time.time;

                    if (nearcontactRigidBodies.Contains(rb))
                    {
                        nearcontactRigidBodies.Remove(rb);
                    }
                }
            }
        }


        private Rigidbody GetDistantRigidBody()
        {
            Rigidbody nearestRigidBody = null;
            float minDistance = MaxInteractionDistance;
            float distance = 0.0f;

            foreach (Rigidbody contactBody in farcontactRigidBodies)
            {
                distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

                if (distance < (minDistance * minDistance))
                {
                    minDistance = distance;
                    nearestRigidBody = contactBody;
                }
            }

            return nearestRigidBody;
        }


        private Rigidbody GetNearRigidBody()
        {
            Rigidbody nearestRigidBody = null;

            foreach (Rigidbody contactBody in nearcontactRigidBodies)
            {
                nearestRigidBody = contactBody;
            }

            return nearestRigidBody;
        }


        private void PlacePointer()
        {
            Vector3 pointerPos = CastControllerRay();
            myPointer.transform.position = pointerPos;
            myPointer.transform.rotation = transform.rotation;
            myPointer.transform.GetChild(0).transform.rotation = transform.rotation;
        }

        private Vector3 CastControllerRay()
        {
            Vector3 rayStart = gameObject.transform.position + (gameObject.transform.forward * 0.3f);
            Ray handRay = new Ray(rayStart, gameObject.transform.forward);
            RaycastHit newHit = new RaycastHit();

            Vector3 currentHitPoint = gameObject.transform.position + (gameObject.transform.forward * -0.125f);

            if (Physics.Raycast(handRay, out newHit, MaxInteractionDistance, PointerLayerMask))
            {
                currentObject = newHit.collider.gameObject;
                currentHitPoint = newHit.point + (handRay.direction.normalized * -0.02f);
            }
            else
            {
                currentObject = null;
            }
            return currentHitPoint;
        }

        #endregion Low-Level Interaction Methods
    
    
    }

}
