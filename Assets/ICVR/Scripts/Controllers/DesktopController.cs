/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using System;
using WebXR;
using ICVR.SharedAssets;

namespace ICVR
{
    /// <summary>
    /// Handles all mouse and keyboard inputs, orients and propels the user in the space.
    /// Connects the user to the objects and tools around them and connects to other components.
    /// <para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Controllers/DesktopController.md"/>

    public class DesktopController : MonoBehaviour
    {
        // Singleton pattern
        private static DesktopController _instance;
        public static DesktopController Instance { get { return _instance; } }

        private Camera _camera;

        // Inspector Variables
        [Tooltip("Character physics container")]
        [SerializeField] private GameObject CharacterVehicle;

        [Tooltip("Mouse sensitivity")]
        [SerializeField] private float mouseSensitivity = 2f;

        [Tooltip("Straffe Speed")]
        [SerializeField] private float straffeSpeed = 1f;

        [Tooltip("(Optional) Joysick for character movement on mobile devices")]
        [SerializeField] private Canvas JoystickCanvas;

        [Tooltip("Joystick sensitivity")]
        [SerializeField] private float joystickMultiplier;


        // Cursor Objects
        [SerializeField] private Texture2D cursorForScene;
        [SerializeField] private Texture2D cursorForObjects;
        [SerializeField] private Texture2D cursorForControls;
        [SerializeField] private ICVRCrosshair crosshair;


        // Public Attributes
        public GameObject CurrentObject { get; set; }

        public float CurrentDistance { get; private set; }

        public Vector3 CurrentHitPoint { get; private set; }


        // Cursor event handling   
        public event BodyController.CursorFocus OnObjectFocus;
        public event BodyController.ObjectTrigger OnObjectTrigger;

        public delegate void CursorInteraction(AvatarHandlingData interactionData);
        public event CursorInteraction OnNetworkInteraction;


        #region ----- Private Variables ------

        private WebXRState xrState = WebXRState.NORMAL;
        private VariableJoystickB variableJoystick;

        private bool isGameMode = false;

        private FixedJoint attachJoint;

        private float runFactor = 1.0f;
        private float jumpCool = 1.0f;

        private float minimumX = -360f;
        private float maximumX = 360f;

        private float minimumY = -90f;
        private float maximumY = 90f;

        private float rotationX = 0f;
        private float rotationY = 0f;

        private Quaternion startRotation;
        private Quaternion currentHeading;

        private Vector2 hotspot = new Vector2(10, 5);
        private readonly CursorMode cMode = CursorMode.ForceSoftware;

        private bool isMouseDown = false;
        private bool isDragging = false;

        private float globalInvertMouse = 1.0f;
        private bool runOne;

        private SharedAsset currentSharedAsset;

        private GameObject activeMesh;
        private GameObject focusedObject;
        private int pLayer = 0;

        private float jumpTick;
        private float triggerTick = 0;

        public bool buttonDown { get; set; }
        private GameObject currentButton;


        #endregion ----- Private Variables ------


        #region ----- Unity Functions ------

        private void Awake()
        {
            _instance = this;
            _camera = GetComponent<Camera>();
        }

        void Start()
        {
            runOne = true;
            jumpTick = Time.time;

            SetCrosshairVisibility();

            startRotation = transform.rotation;
            currentHeading = startRotation;

            attachJoint = GetComponent<FixedJoint>();

            if (JoystickCanvas != null)
            {
                variableJoystick = JoystickCanvas.gameObject.GetComponentInChildren<VariableJoystickB>();
                UpdateJoystickVisibility();
            }
        }

        void FixedUpdate()
        {
            if (xrState != WebXRState.NORMAL) { return; }

            // set character pose
            MoveBodyWithKeyboard(straffeSpeed);
            MoveBodyWithJoystick();            
            SetCameraRotation();        

            // make observation, update current object and cursor
            GameObject viewedObject = ScreenRaycast();
            if (viewedObject != null)
            {
                CurrentObject = viewedObject;
            }
            else
            {
                CurrentObject = null;
            }

        }

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            JoystickCanvas.gameObject.SetActive(PlatformManager.Instance.IsMobile);
            SetCursorParameters();
            UpdateJoystickVisibility();
        }


        #endregion ----- Unity Functions ------


        #region ----- Input Handling -----

        [SerializeField] private bool DebugMouseInteraction;

        void OnGUI()
        {
            if (xrState != WebXRState.NORMAL) { return; }

            if (!Application.isEditor || DebugMouseInteraction)
            {
                SetCursorImage();
            }

            Event e = Event.current;

            // mouse events
            if (e.isMouse)
            {
                if (e.button == 0)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        isMouseDown = true;
                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        isMouseDown = false;
                    }
                }

                if (e.clickCount == 2)
                {
                    DoubleClick();
                    isMouseDown = false;
                }

                if (e.type == EventType.MouseDown && e.button == 0 && CurrentObject != null)
                {
                    if (CurrentObject.layer == 9 || CurrentObject.layer == 14)
                    {
                        //set interation options for controllables...
                    }
                    else if (CurrentObject.layer == 10 || CurrentObject.layer == 15)
                    {
                        // identify shared asset and assign currentTargetId
                        SharedAsset sharedAsset = CurrentObject.GetComponent<SharedAsset>();

                        if (sharedAsset)
                        {
                            if (!sharedAsset.IsBeingHandled)
                            {
                                currentSharedAsset = sharedAsset;
                                PickUpObject(CurrentObject);
                            }
                            else
                            {
                                Debug.Log("This object is being used by someone else. \n" +
                                    "Please pester them until they let you play with it.");
                            }
                        }
                        else if (!CurrentObject.GetComponent<XRController>())
                        {
                            PickUpObject(CurrentObject);
                        }
                    }
                    else if (CurrentObject.layer == 12) // buttons
                    {
                        ActivateObjectTrigger(CurrentObject);
                    }
                }
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    if (isDragging)
                    {
                        ReleaseObject();
                    }
                    else if (buttonDown && currentButton != null)
                    {
                        ReleaseObjectTrigger(currentButton);
                    }
                }
            }


            // keyboard events
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.I)
                {
                    globalInvertMouse *= -1.0f;
                }

                if (e.keyCode == KeyCode.M)
                {
                    ToggleGameMode();
                }

                if (e.keyCode == KeyCode.Space)
                {
                    JumpSwim();
                }

                if (e.keyCode == KeyCode.LeftShift)
                {
                    runFactor = 2.0f;
                }

                if (e.keyCode == KeyCode.CapsLock)
                {
                    if (runFactor == 1.0f)
                        runFactor = 2.0f;
                    else
                        runFactor = 1.0f;
                }

            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.LeftShift)
                {
                    runFactor = 1.0f;
                }
            }
        }

        #endregion ----- Input Handling -----


        #region ----- Character Movement ------



        private Quaternion GetCameraRotationFromMouse(float sensitivity, float invertMouse)
        {
            if (runOne)
            {
                rotationX = Input.GetAxis("Mouse X");
                rotationY = Input.GetAxis("Mouse Y");
                runOne = false;
            }
            else
            {
                rotationX += (Input.GetAxis("Mouse X") * invertMouse) * sensitivity;
                rotationY += (Input.GetAxis("Mouse Y") * invertMouse) * sensitivity;
            }
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            return RelativeQuatFromIncrements(rotationX, rotationY);
        }

        private Quaternion RelativeQuatFromIncrements(float rotX, float rotY)
        {
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
            currentHeading = startRotation * xQuaternion * yQuaternion;
            return currentHeading;
        }

        private void MoveBodyWithJoystick()
        {
            if (variableJoystick == null) { return; }

            float x = variableJoystick.Horizontal * 0.5f * Time.deltaTime * joystickMultiplier;
            float z = variableJoystick.Vertical * 0.5f * Time.deltaTime * joystickMultiplier;

            // conditions for no action
            if (x == 0 && z == 0) return;

            //camera forward and right vectors
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            //project forward and right vectors on the horizontal plane (y = 0)
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            //this is the direction in the world space we want to move:
            var desiredMoveDirection = forward * z + right * x;
            CharacterVehicle.transform.Translate(desiredMoveDirection);
        }

        private void MoveBodyWithKeyboard(float multiplier)
        {
            float x = Input.GetAxis("Horizontal") * Time.smoothDeltaTime * runFactor;
            float z = Input.GetAxis("Vertical") * Time.smoothDeltaTime * runFactor;

            // conditions for no action
            if (CharacterVehicle == null) return;
            if (x == 0 && z == 0) return;

            //project forward and right vectors on the horizontal plane (y = 0)
            Vector3 personForward = CharacterVehicle.transform.InverseTransformDirection(transform.forward);
            Vector3 personRight = CharacterVehicle.transform.InverseTransformDirection(transform.right);
            personForward.y = 0;
            personRight.y = 0;
            personForward.Normalize();
            personRight.Normalize();

            //this is the direction in the world space we want to move:
            var desiredMoveDirection = personForward * z + personRight * x;
            CharacterVehicle.transform.Translate(desiredMoveDirection * multiplier);
        }

        private void SetCameraRotation()
        {
            //float dragMod = 1.0f;
            float dragMod = isDragging ? -1.5f * globalInvertMouse : 1.0f;

            if (isGameMode)
            {
                Quaternion camQuat = GetCameraRotationFromMouse(mouseSensitivity, 1.0f * globalInvertMouse);
                StartCoroutine(RotateCamera(camQuat, mouseSensitivity));
            }
            else
            {
                if (isMouseDown)
                {
                    Quaternion camQuat = GetCameraRotationFromMouse(mouseSensitivity, -1.0f * dragMod * globalInvertMouse);
                    StartCoroutine(RotateCamera(camQuat, mouseSensitivity));
                }
            }
        }

        private void JumpSwim()
        {
            if (CharacterVehicle == null) { return; }

            bool isSwimming = CharacterVehicle.transform.position.y < -10f;

            if (isSwimming && Time.time - jumpTick > (jumpCool / 10.0f))
            {
                jumpTick = Time.time;
                Vector3 swimForce = new Vector3(0f, 150f, 0f) + (transform.forward * 10f);
                CharacterVehicle.GetComponent<Rigidbody>().AddForce(swimForce, ForceMode.Impulse);
            }
            else if (Time.time - jumpTick > jumpCool)
            {
                jumpTick = Time.time;
                Vector3 jumpForce = new Vector3(0f, 350f, 0f) + transform.forward * 50f;
                CharacterVehicle.GetComponent<Rigidbody>().AddForce(jumpForce, ForceMode.Impulse);
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        private System.Collections.IEnumerator RotateCamera(Quaternion targetRot, float speed)
        {

            float rotationTimer = 0.0f;

            while (rotationTimer < 0.8)
            {
                rotationTimer += Time.smoothDeltaTime * 1f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationTimer * speed);
                yield return new WaitForEndOfFrame();
            }
        }


        #endregion ----- Character Movement ------


        private void ToggleGameMode()
        {
            isGameMode = !isGameMode;
            SetCrosshairVisibility();
            SetCursorParameters();
        }

        private void SetCrosshairVisibility()
        {
            // toggle crosshair visibility
            int isGamey = (isGameMode ? 1 : 0) * 255;
            crosshair.SetColor(CrosshairColorChannel.ALPHA, isGamey, true);
        }

        private void SetCursorParameters()
        {
            if (xrState != WebXRState.NORMAL)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = isGameMode ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isGameMode;
            }
        }

        private void SetDefaultCursor()
        {
            if (focusedObject)
            {
                InvokeFocusEvent(focusedObject, false);
                focusedObject = null;
            }
            pLayer = 0;
            Cursor.SetCursor(cursorForScene, hotspot, cMode);
            if (isGameMode) crosshair.SetGap(6, true);
        }

        private void SetCursorImage()
        {
            if (CurrentObject == null)
            {
                SetDefaultCursor();
                return;
            }

            if (CurrentObject.layer == pLayer)
            {
                return;
            }

            pLayer = CurrentObject.layer;

            // ui buttons
            if (CurrentObject.layer == 12)
            {
                Cursor.SetCursor(cursorForControls, hotspot, cMode);
            }
            // interactable objects
            else if (CurrentObject.layer == 10 || CurrentObject.layer == 15)
            {
                focusedObject = CurrentObject;
                InvokeFocusEvent(CurrentObject, true);
                Cursor.SetCursor(cursorForObjects, hotspot, cMode);
                if (isGameMode) crosshair.SetGap(18, true);
            }
            // controllable cursor
            else if (CurrentObject.layer == 9 || CurrentObject.layer == 14)
            {
                focusedObject = CurrentObject;
                InvokeFocusEvent(CurrentObject, true);
                Cursor.SetCursor(cursorForControls, hotspot, cMode);
            }
            // default (scene) cursor
            else
            {
                InvokeFocusEvent(null, false);
                SetDefaultCursor();
            }
        }

        private void UpdateJoystickVisibility()
        {
            JoystickCanvas.gameObject.SetActive(PlatformManager.Instance.IsMobile);
        }


        #region ----- Object Interaction ------

        bool wasKinematic = false;

        private GameObject PickUpObject(GameObject ooi)
        {
            if (ooi != null)
            {
                activeMesh = GetActiveMesh(ooi);
                if (activeMesh == null) return null;

                Rigidbody actRB = activeMesh.GetComponent<Rigidbody>();
                actRB.isKinematic = false;

                attachJoint.connectedBody = actRB;

                if (currentSharedAsset)
                {
                    currentSharedAsset.IsBeingHandled = true;
                    InvokeAcquisitionEvent(currentSharedAsset.Id, activeMesh.transform);
                }

                // flag caught by the fixed update
                isDragging = true;
                return activeMesh;
            }
            else
            {
                activeMesh = null;
                return null;
            }
        }

        private void ReleaseObject()
        {
            isDragging = false;

            if (activeMesh.TryGetComponent(out RigidDynamics dynamics))
            {
                Rigidbody activeRB = activeMesh.GetComponent<Rigidbody>();

                attachJoint.connectedBody = null;
                activeRB.useGravity = dynamics.UsesGravity;

                ThrowData td = dynamics.Throw;

                activeRB.isKinematic = wasKinematic;
                activeRB.AddForce(td.LinearForce, ForceMode.Impulse);
                activeRB.AddTorque(td.AngularForce, ForceMode.Impulse);

                // network release
                if (currentSharedAsset)
                {
                    InvokeReleaseEvent(currentSharedAsset.Id, activeMesh, td);
                    currentSharedAsset.IsBeingHandled = false;
                    currentSharedAsset = null;
                }
            }

            if (activeMesh.TryGetComponent(out ControlDynamics cd))
            {
                cd.FinishInteraction();
            }

            activeMesh = null;
        }

        private void InvokeFocusEvent(GameObject focalObject, bool state)
        {
            OnObjectFocus?.Invoke(focalObject, state);
        }

        private void InvokeAcquisitionEvent(string target, Transform interactionTransform)
        {
            AcquireData newAcquisition = new AcquireData
            {
                AcqTime = DateTime.UtcNow.Ticks,
                ObjectPosition = interactionTransform.position,
                ObjectRotation = interactionTransform.rotation
            };

            var interactionEvent = BuildEventFrame(target, AvatarInteractionEventType.AcquireData, newAcquisition, null);
            OnNetworkInteraction.Invoke(interactionEvent);
        }

        private void InvokeReleaseEvent(string target, GameObject interactionObject, ThrowData throwData)
        {
            ReleaseData newRelease = new ReleaseData
            {
                ReleaseTime = DateTime.UtcNow.Ticks,
                ReleasePosition = interactionObject.transform.position,
                ReleaseRotation = interactionObject.transform.rotation,
                ForceData = throwData
            };

            var interactionEvent = BuildEventFrame(target, AvatarInteractionEventType.ReleaseData, null, newRelease);
            OnNetworkInteraction.Invoke(interactionEvent);
        }

        private AvatarHandlingData BuildEventFrame(string targetId, AvatarInteractionEventType eventType, AcquireData acqDataFrame = null, ReleaseData relDataFrame = null)
        {
            AvatarHandlingData eventFrame = new AvatarHandlingData
            {
                Hand = ControllerHand.NONE,
                TargetId = targetId,
                Distance = ManipulationDistance.None,
                EventType = eventType,
                AcquisitionEvent = acqDataFrame,
                ReleaseEvent = relDataFrame
            };
            return eventFrame;
        }

        private GameObject GetActiveMesh(GameObject ooi)
        {
            // priority: self, parent, child
            if (ooi.GetComponent<Rigidbody>())
            {
                return ooi;
            }
            else if (ooi.GetComponentInParent<Rigidbody>())
            {
                return ooi.GetComponentInParent<Rigidbody>().gameObject;
            }
            else if (ooi.GetComponentInChildren<Rigidbody>())
            {
                return ooi.GetComponentInChildren<Rigidbody>().gameObject;
            }
            else
            {
                return null;
            }
        }

        private void ActivateObjectTrigger(GameObject currObj)
        {
            if (currObj.TryGetComponent(out PressableButton pba) && (Time.time - triggerTick) > 0.5f)
            {
                triggerTick = Time.time;
                buttonDown = true;
                currentButton = currObj;
                pba.ButtonPressed.Invoke();
            }
        }

        private void ReleaseObjectTrigger(GameObject currObj)
        {
            if (currObj.TryGetComponent(out PressableButton pba))
            {
                buttonDown = false;
                currentButton = null;
                pba.ButtonReleased.Invoke();
            }
        }

        private void DoubleClick()
        {
            if (CurrentObject != null)
            {
                if (CurrentObject.TryGetComponent(out ObjectInterface objInt))
                {
                    OnObjectTrigger?.Invoke(objInt.gameObject, 0.75f);
                }
            }
        }

        private GameObject ScreenRaycast(bool fromTouch = false)
        {
            if (!_camera.isActiveAndEnabled) return null;

            if (isDragging) return CurrentObject;

            Ray ray;

            if (isGameMode)
            {
                ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                ray = _camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            }

            if (Physics.Raycast(ray, out RaycastHit pointerHit, 60.0f, Physics.DefaultRaycastLayers))
            {
                CurrentHitPoint = pointerHit.point;
                CurrentDistance = pointerHit.distance;
                return pointerHit.transform.gameObject;
            }
            else
            {
                return null;
            }
        }

        #endregion ----- Object Interaction ------


    }
}