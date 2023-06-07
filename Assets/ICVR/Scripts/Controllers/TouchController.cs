using System;
using System.Collections;
using UnityEngine;


namespace ICVR
{

    public class TouchController : MonoBehaviour
    {

        // Singleton pattern
        private static TouchController _instance;

        public static TouchController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public GameObject CurrentObject { get; set; }

        [Tooltip("Mouse sensitivity")]
        //[SerializeField] private float touchSensitivity = 1f;

        // Vars
        private float minimumX = -360f;
        private float maximumX = 360f;

        private float minimumY = -90f;
        private float maximumY = 90f;

        private float rotationX = 0f;
        private float rotationY = 0f;

        private Quaternion originalRotation;

        private float globalInvertMouse = 1.0f;
        //private bool runOne;


        private Touch currentTouch;
        private bool touchOne;

        // touch-rotation vars
        private float xAngle = 0.0f;
        private float yAngle = 0.0f;

        void Start()
        {
            //runOne = true;
            touchOne = true;

            xAngle = 0.0f;
            yAngle = 0.0f;
            transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
        }


        private Quaternion GetCameraRotationFromTouch(float sensitivity, float invertMouse, bool nTouch = false)
        {
            //Touch touch = Input.GetTouch(0);
            float sensFactor = globalInvertMouse * invertMouse * sensitivity;

            if (touchOne || nTouch)
            {
                originalRotation = Camera.main.transform.rotation;
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

            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
            Quaternion newRot = originalRotation * xQuaternion * yQuaternion;

            return newRot;
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



        /*
        void OnGUI()
        {
            if (SimpleWebXR.InSession) return;

            Event e = Event.current;
            int t = Input.touchCount;

            if (t > 2 && activeMesh != null)
            {
                if (DateTime.UtcNow.Ticks - touchStartTick < MinTouchInterval / 5.0f) { return; }
                touchStartTick = DateTime.UtcNow.Ticks;

                Vector2 startTouches = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
                SimpleController.Instance.SimulateThrowForwards(startTouches, Input.GetTouch(2).position);

                return;
            }
            else if (t == 2)
            {
                if (DateTime.UtcNow.Ticks - touchStartTick < MinTouchInterval) { return; }
                touchStartTick = DateTime.UtcNow.Ticks;

                if (CurrentObject != null && CurrentObject.layer == 12 && Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    SimpleController.Instance.SendObjectTrigger(CurrentObject);
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

                    CurrentObject = SimpleController.Instance.ScreenRaycast(true);
                    if (CurrentObject != null && CurrentObject.layer == 10)
                    {
                        SimpleController.Instance.SetActiveMesh();
                    }
                    else if (CurrentObject != null && CurrentObject.layer == 12)
                    {
                        SimpleController.Instance.SendObjectTrigger(CurrentObject);
                    }
                }
                else if (currentTouch.phase == TouchPhase.Moved)
                {
                    touching = true;

                    if (SimpleController.Instance.buttonDown) return;

                    float dragMod = isDragging ? -1.0f : 1.0f;

                    secondpoint = currentTouch.position;

                    xAngle = xAngTemp + (secondpoint.x - firstpoint.x) * -180.0f * dragMod / Screen.width;
                    yAngle = yAngTemp - (secondpoint.y - firstpoint.y) * -90.0f * dragMod / Screen.height;

                    Quaternion targetRotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
                    StartCoroutine(RotateCamera(targetRotation, touchSensitivity));

                }
                else if (currentTouch.phase == TouchPhase.Ended)
                {
                    touching = false;
                    if (isDragging)
                    {
                        SimpleController.Instance.ReleaseObject();
                    }
                    else if (SimpleController.Instance.buttonDown)
                    {
                        SimpleController.Instance.ReleaseObjectTrigger(CurrentObject);
                    }
                }
                else if (currentTouch.phase == TouchPhase.Canceled)
                {
                    touching = false;
                }

            }

        }

        */
    
    
    }

}