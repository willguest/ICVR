using UnityEngine;
using WebXR;

namespace ICVR
{
    /// <summary>
    /// The RigController acts as an adjustable pivot between the head and hands.
    /// It is useful for body-oriented events and functions, and connects the HUD.
    /// </summary>
    public class RigController : MonoBehaviour
    {
        [SerializeField] private Transform cameraReference;
        [SerializeField] private GameObject HUDObjectRoot;

        [Range(1f, 10f)]
        [SerializeField] private float HUDSnappiness = 3;

        private Vector3 BodyOffset;
        private Vector3 UiOffset;
        private Quaternion UiStartRot;
        private WebXRState xrState = WebXRState.NORMAL;

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
        }

        public void Start()
        {
            UiStartRot = HUDObjectRoot.transform.localRotation;
            UiOffset = HUDObjectRoot.transform.position - transform.position;
            BodyOffset = cameraReference.position - transform.position;
        }

        private void FixedUpdate()
        {
            transform.localPosition = cameraReference.localPosition - BodyOffset;

            transform.localRotation = Quaternion.Euler(0f, cameraReference.localRotation.eulerAngles.y, 0f);

            if (xrState == WebXRState.NORMAL)
            {
                // set position of following UI
                if (HUDObjectRoot.activeInHierarchy)
                {
                    Vector3 hudTarget = transform.position + (transform.forward * UiOffset.z) + (transform.up * UiOffset.y) + transform.right * UiOffset.x;
                    HUDObjectRoot.transform.position = Vector3.Lerp(HUDObjectRoot.transform.position, hudTarget, Time.deltaTime * HUDSnappiness);

                    HUDObjectRoot.transform.rotation = Quaternion.LookRotation(transform.forward) * UiStartRot;
                }
            }
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            transform.localRotation = Quaternion.identity;
            HUDObjectRoot.SetActive(xrState == WebXRState.NORMAL);
        }
    }
}
