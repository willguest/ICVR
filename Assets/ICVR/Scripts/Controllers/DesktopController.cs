// Copyright (c) Will Guest 2023
// Licensed under Mozilla Public License 2.0

using System.Collections;
using UnityEngine;
using System;
using WebXR;
using ICVR.SharedAssets;

namespace ICVR
{
    public class DesktopController : MonoBehaviour
    {
        [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
        [SerializeField] private bool rotationEnabled = true;

        [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
        [SerializeField] private bool translationEnabled = true;

        //private WebXRDisplayCapabilities capabilities;

        [Tooltip("Mouse sensitivity")]
        [SerializeField] private float mouseSensitivity = 1f;

        [Tooltip("Straffe Speed")]
        [SerializeField] private float straffeSpeed = 5f;

        [SerializeField] private float seaLevel = -4.5f;

        [Tooltip("object to move around with mouse and keyboard")]
        [SerializeField] private GameObject currentVehicle;

        [Tooltip("head object that moves around with camera")]
        [SerializeField] private GameObject headObject;

        // cursor objects
        [SerializeField] private Texture2D cursorForScene;
        [SerializeField] private Texture2D cursorForObjects;
        [SerializeField] private Texture2D cursorForInteractables;
        [SerializeField] private SimpleCrosshair crosshair;

        // public attributes
        public bool IsSwimming { get; set; }

        public GameObject CurrentObject { get; set; }

        // cursor event handling
        public delegate void CursorInteraction(AvatarHandlingData interactionData);
        public event CursorInteraction OnNetworkInteraction;

        //[SerializeField] private Canvas JoystickCanvas;
        //[SerializeField] private float joystickMultiplier;

        private VariableJoystickB variableJoystick;

        #region ----- Private Variables ------

        private WebXRState xrState = WebXRState.NORMAL;

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

        private bool isDragging = false;

        private float globalInvertMouse = 1.0f;
        private bool runOne;

        private SharedAsset currentSharedAsset;

        private float currentDistance;
        private Vector3 currentHitPoint;

        //elevation
        private float currentElevation;

        private GameObject activeMesh;
        private string prevMeshName = "";

        private Vector3 screenPoint;
        private Vector3 offset;

        static bool touching = false;
        private Touch currentTouch;
        private bool touchOne;

        private float jumpTick;

        private long touchStartTick = 0;
        private float triggerTick = 0;

        private bool isHudBusy = false;
        public bool buttonDown { get; set; }

        private bool isEditor;

        #endregion ----- Private Variables ------

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            //JoystickCanvas.gameObject.SetActive(xrState == WebXRState.NORMAL);
            SetCursorParameters();
        }

        private void Awake()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                isEditor = true;
            }
        }

        void Start()
        {
            runOne = true;
            touchOne = true;
            jumpTick = Time.time;

            SetCrosshairVisibility();
            //SetCursorParameters();

            startRotation = headObject.transform.rotation;
            currentHeading = startRotation;

            xAngle = 0.0f;
            yAngle = 0.0f;
            transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);

            attachJoint = GetComponent<FixedJoint>();
            //variableJoystick = JoystickCanvas.GetComponentInChildren<VariableJoystickB>();
        }


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

        private Quaternion GetCameraRotationFromTouch(float sensitivity, float invertMouse, bool nTouch = false)
        {
            //Touch touch = Input.GetTouch(0);
            float sensFactor = globalInvertMouse * invertMouse * sensitivity;

            if (touchOne || nTouch)
            {
                rotationX = currentTouch.position.x;
                rotationY = currentTouch.position.y;
                touchOne = false;
            }
            else
            {
                rotationX = currentTouch.position.x * sensFactor;
                rotationY = currentTouch.position.y * sensFactor;
            }
            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            return RelativeQuatFromIncrements(rotationX, rotationY);
        }


        void Update()
        {
            if (xrState != WebXRState.NORMAL) { return; }

            // lateral movement
            if (translationEnabled)
            {
                MoveBodyWithKeyboard(headObject, straffeSpeed);
                //MoveBodyWithJoystick();
            }

            if (rotationEnabled)
            {
                SetCameraRotation();

                // make observation, update current object and cursor
                if (Camera.main.enabled)
                {
                    GameObject viewedObject = ScreenRaycast();
                    if (viewedObject != null)
                    {
                        CurrentObject = viewedObject;
                    }
                    else
                    {
                        CurrentObject = null;
                        prevMeshName = "";
                    }
                }
            }

            //detect swimming
            float elevation = currentVehicle.transform.position.y;
            if (elevation < seaLevel && currentElevation >= seaLevel)
            {
                IsSwimming = true;
                currentVehicle.GetComponent<Rigidbody>().mass = 2.0f;
            }
            else if (elevation > seaLevel && currentElevation <= seaLevel)
            {
                IsSwimming = false;
                currentVehicle.GetComponent<Rigidbody>().mass = 70.0f;
            }
            currentElevation = elevation;

        }

        /* // coming later
        private void MoveBodyWithJoystick()
        {
            float x = variableJoystick.Horizontal * 0.5f * Time.deltaTime * joystickMultiplier;
            float z = variableJoystick.Vertical * 0.5f * Time.deltaTime * joystickMultiplier;

            // conditions for no action
            //Camera referenceCam = Camera.main;
            if (headObject == null) return;
            if (x == 0 && z == 0) return;

            //camera forward and right vectors
            Vector3 forward = headObject.transform.forward;
            Vector3 right = headObject.transform.right;

            //project forward and right vectors on the horizontal plane (y = 0)
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            //this is the direction in the world space we want to move:
            var desiredMoveDirection = forward * z + right * x;
            currentVehicle.transform.Translate(desiredMoveDirection);
        }
        */

        private void MoveBodyWithKeyboard(GameObject referenceObject, float multiplier = 1.0f)
        {
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * straffeSpeed * runFactor;
            float z = Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed * runFactor;

            // conditions for no action
            if (referenceObject == null) return;
            if (x == 0 && z == 0) return;

            //camera forward and right vectors
            Vector3 forward = referenceObject.transform.forward;
            Vector3 right = referenceObject.transform.right;


            Vector3 personForward = currentVehicle.transform.InverseTransformDirection(headObject.transform.forward);
            Vector3 personRight = currentVehicle.transform.InverseTransformDirection(headObject.transform.right);
            personForward.y = 0;
            personRight.y = 0;
            personForward.Normalize();
            personRight.Normalize();

            //project forward and right vectors on the horizontal plane (y = 0)
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            //this is the direction in the world space we want to move:
            var desiredMoveDirection = personForward * z + personRight * x;
            currentVehicle.transform.Translate(desiredMoveDirection * multiplier);
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


        // touch vars
        private bool isMouseDown = false;

        // touch-rotation vars
        private Vector3 firstpoint;
        private Vector3 secondpoint;
        private float xAngle = 0.0f;
        private float yAngle = 0.0f;
        private float xAngTemp = 0.0f;
        private float yAngTemp = 0.0f;

        private float MinTouchInterval = 2000000f; // 10,000,000 * noSecs

        void OnGUI()
        {
            if (xrState != WebXRState.NORMAL) { return; }
            if (!isEditor)
            {
                SetCursorImage();
            }
            Event e = Event.current;
            int t = Input.touchCount;

            if (t > 2 && activeMesh != null)
            {
                if (DateTime.UtcNow.Ticks - touchStartTick < MinTouchInterval / 5.0f) { return; }
                touchStartTick = DateTime.UtcNow.Ticks;

                Vector2 startTouches = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
                SimulateThrowForwards(startTouches, Input.GetTouch(2).position);

                return;
            }
            else if (t == 2)
            {
                if (DateTime.UtcNow.Ticks - touchStartTick < MinTouchInterval) { return; }
                touchStartTick = DateTime.UtcNow.Ticks;

                if (CurrentObject != null && CurrentObject.layer == 12 && Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    ActivateObjectTrigger(CurrentObject);
                }

            }
            else if (t == 1)
            {
                currentTouch = Input.GetTouch(0);
                touching = true;

                if (currentTouch.phase == TouchPhase.Began)
                {
                    if (DateTime.UtcNow.Ticks - touchStartTick < MinTouchInterval) { return; }

                    touchStartTick = DateTime.UtcNow.Ticks;
                    firstpoint = currentTouch.position;
                    xAngTemp = xAngle;
                    yAngTemp = yAngle;

                    CurrentObject = ScreenRaycast(true);

                    if (CurrentObject != null && (CurrentObject.layer == 10 || CurrentObject.layer == 15))
                    {
                        PickUpObject(CurrentObject);
                    }
                    else if (CurrentObject != null && CurrentObject.layer == 12)
                    {
                        ActivateObjectTrigger(CurrentObject);
                    }
                }
                else if (currentTouch.phase == TouchPhase.Moved)
                {
                    touching = true;

                    if (buttonDown) return;

                    float dragMod = isDragging ? -1.0f : 1.0f;

                    secondpoint = currentTouch.position;

                    xAngle = xAngTemp + (secondpoint.x - firstpoint.x) * -180.0f * dragMod / Screen.width;
                    yAngle = yAngTemp - (secondpoint.y - firstpoint.y) * -90.0f * dragMod / Screen.height;

                    Quaternion targetRotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
                    StartCoroutine(RotateCamera(targetRotation, mouseSensitivity));

                }
                else if (currentTouch.phase == TouchPhase.Ended)
                {
                    touching = false;
                    if (isDragging)
                    {
                        ReleaseObject();
                    }
                    else if (buttonDown)
                    {
                        ReleaseObjectTrigger(CurrentObject);
                    }
                }
                else if (currentTouch.phase == TouchPhase.Canceled)
                {
                    touching = false;
                }

            }

            // next, mouse events
            else if (e.isMouse)
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
                    if (CurrentObject.layer == 9)
                    {
                        //start interation mode for furniture...
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
                                // asset is being used, do nothing
                                Debug.Log("This object is being used by someone else. Please pester them until they let you play with it.");
                            }
                        }
                        else
                        {
                            PickUpObject(CurrentObject);
                        }

                    }
                    else if (CurrentObject.layer == 12)
                    {
                        ActivateObjectTrigger(CurrentObject);
                    }
                    else if (CurrentObject.layer == 14)
                    {
                        //SendPersonTrigger(CurrentObject);
                    }
                }
                else if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    if (isDragging)
                    {
                        //activeMesh.transform.position = GetDraggingPoint();

                    }


                }
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    if (isDragging)
                    {
                        ReleaseObject();
                    }
                    else if (buttonDown && CurrentObject)
                    {
                        ReleaseObjectTrigger(CurrentObject);
                    }
                }
            }


            // finally, keyboard events
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
            }

            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.LeftShift)
                {
                    runFactor = 1.0f;
                }
            }

        }

        private void JumpSwim()
        {
            if (IsSwimming && Time.time - jumpTick > (jumpCool / 10.0f))
            {
                jumpTick = Time.time;
                Vector3 swimForce = new Vector3(0f, 150f, 0f) + (Camera.main.transform.forward * 50f);
                currentVehicle.GetComponent<Rigidbody>().AddForce(swimForce, ForceMode.Acceleration);
            }
            else if (Time.time - jumpTick > jumpCool)
            {
                jumpTick = Time.time;
                Vector3 jumpForce = new Vector3(0f, 350f, 0f) + Camera.main.transform.forward * 50f;
                currentVehicle.GetComponent<Rigidbody>().AddForce(jumpForce, ForceMode.Impulse);
            }
        }

        private void ToggleGameMode()
        {
            isGameMode = !isGameMode;
            SetCrosshairVisibility();
            SetCursorParameters();
        }

        private Vector3 SetScreenPointOffset(Vector3 activeMeshPosition)
        {
            screenPoint = Camera.main.WorldToScreenPoint(activeMesh.transform.position);

            if (float.IsNaN(screenPoint.z))
            {
                return Vector3.zero;
            }

            if (touching)
            {
                offset = activeMeshPosition - Camera.main.ScreenToWorldPoint(
                    new Vector3(currentTouch.position.x, currentTouch.position.y, screenPoint.z));
            }
            else if (isGameMode)
            {
                offset = activeMeshPosition - Camera.main.ViewportToWorldPoint(
                    new Vector3(0.5f, 0.5f, screenPoint.z));
            }
            else
            {
                offset = activeMeshPosition - Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }

            return offset;
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


        private void SetCursorImage()
        {
            // ui buttons
            if (CurrentObject != null && CurrentObject.layer == 12)
            {
                Cursor.SetCursor(cursorForInteractables, hotspot, cMode);

            }
            //interactale objects
            else if (CurrentObject != null && (CurrentObject.layer == 10 || CurrentObject.layer == 15))
            {
                Cursor.SetCursor(cursorForObjects, hotspot, cMode);
                if (isGameMode) crosshair.SetGap(18, true);
            }
            //add furniture cursor
            else if (CurrentObject != null && CurrentObject.layer == 9)
            {
                Cursor.SetCursor(cursorForInteractables, hotspot, cMode);
            }
            // default (scene) cursor
            else
            {
                Cursor.SetCursor(cursorForScene, hotspot, cMode);
                if (isGameMode) crosshair.SetGap(6, true);
            }
        }


        public GameObject PickUpObject(GameObject ooi)
        {
            if (ooi != null)
            {
                activeMesh = GetActiveMesh(ooi);
                if (activeMesh == null) return null;

                ViewObject(activeMesh);

                //SetScreenPointOffset(activeMesh.transform.position);

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

                activeRB.isKinematic = false;
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

        private AvatarController GetAvatarController(GameObject testObject)
        {
            if (testObject.GetComponent<AvatarController>())
            {
                return testObject.GetComponent<AvatarController>();
            }
            else if (testObject.GetComponentInParent<AvatarController>())
            {
                return testObject.GetComponentInParent<AvatarController>();
            }
            else if (testObject.GetComponentInChildren<AvatarController>())
            {
                return testObject.GetComponentInChildren<AvatarController>();
            }
            else
            {
                return null;
            }
        }

        private GameObject GetActiveMesh(GameObject ooi)
        {
            // priority: self, parent, child rigidbody
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


        public void ActivateObjectTrigger(GameObject currObj)
        {
            if (currObj.TryGetComponent(out UnityEngine.UI.Button button) && !button.interactable)
            {
                return;
            }

            if (currObj.TryGetComponent(out PressableButton pba) && (Time.time - triggerTick) > 0.5f)
            {
                triggerTick = Time.time;
                buttonDown = true;
                pba.ButtonPressed.Invoke();
            }

        }

        public void SendPersonTrigger(GameObject avatar)
        {
            AvatarController personController = GetAvatarController(avatar);
            if (personController && (Time.time - triggerTick) > 0.5f)
            {
                triggerTick = Time.time;
                buttonDown = true;
                //personController.OpenAudioChannel(personController.gameObject.name);
            }
        }

        public void ReleaseObjectTrigger(GameObject currObj)
        {
            if (currObj.TryGetComponent(out UnityEngine.UI.Button button) && !button.interactable)
            {
                return;
            }

            if (currObj.TryGetComponent(out PressableButton pba))
            {
                buttonDown = false;
                pba.ButtonReleased.Invoke();
            }

            /*
            if (currObj.GetComponent<AvatarController>())
            {
                buttonDown = false;
                currObj.GetComponent<AvatarController>().CloseAudioChannel(currObj.name);
            }
            */
        }

        private string ViewObject(GameObject viewObject)
        {
            if (viewObject != null)
            {
                string meshName = viewObject.name;
                //Debug.Log(DateTime.Now.ToString("u") + "_Viewed_" + meshName);

                if (prevMeshName != meshName && (viewObject.layer == 10 || viewObject.layer == 15))
                {
                    prevMeshName = meshName;
                    if (!isHudBusy)
                    {
                        //StartCoroutine(FloatUpAndOut(viewObject, meshName));
                    }
                }
                else if (prevMeshName != meshName && viewObject.layer == 9)
                {
                    prevMeshName = meshName;
                    if (!isHudBusy)
                    {
                        // report object view
                    }
                }
                return meshName;
            }
            else
            {
                return "";
            }
        }

        private Vector3 GetDraggingPoint()
        {
            if (isGameMode)
            {
                return Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, screenPoint.z)) + offset;
            }
            else
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 objPos = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                return objPos;
            }
        }

        public void SimulateThrowForwards(Vector2 screenPos, Vector2 targetTouch)
        {
            isDragging = false;

            activeMesh.GetComponent<Rigidbody>().isKinematic = false;

            Vector3 startPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, screenPoint.z));
            activeMesh.transform.position = startPos;

            Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(targetTouch.x, targetTouch.y, screenPoint.z * 3.0f));
            Ray throwRay = new Ray(startPos, targetPos - startPos);

            float forceMagnifier = activeMesh.GetComponent<Rigidbody>().mass * 5.0f;
            Vector3 bowlTraj = (targetPos - startPos) * forceMagnifier;

            activeMesh.GetComponent<Rigidbody>().AddForce(bowlTraj, ForceMode.Impulse);
            activeMesh = null;
        }





        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        IEnumerator RotateCamera(Quaternion targetRot, float speed)
        {
            float rotationTimer = 0.0f;

            while (rotationTimer < 0.8)
            {
                rotationTimer += Time.smoothDeltaTime * 1f;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationTimer * speed);
                yield return new WaitForEndOfFrame();
            }
        }



        private void DoubleClick()
        {
            if (CurrentObject != null)
            {
                CurrentObject.SendMessage("OnDoubleClick");
            }
        }


        public GameObject ScreenRaycast(bool fromTouch = false)
        {
            if (isDragging) return CurrentObject;

            Ray ray;

            if (fromTouch)
            {
                ray = Camera.main.ScreenPointToRay(currentTouch.position);
            }
            else if (isGameMode)
            {
                ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            if (Physics.Raycast(ray, out RaycastHit pointerHit, 15.0f, Physics.DefaultRaycastLayers))
            {
                currentHitPoint = pointerHit.point;
                currentDistance = pointerHit.distance;
                return pointerHit.transform.gameObject;
            }
            else
            {
                return null;
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

    }
}