using UnityEngine;
using WebXR;

namespace ICVR
{
    /// <summary>
    /// The RigController acts as an adjustable pivot between the head and hands.
    /// This is useful for body-oriented events and functions. Currently it serves three functions:
    /// - It aligns the HUD to the body, allowing them to switch to at any time.
    /// - It communicates with the body avatar controller (if present) to switch arm rigging.
    /// - It contains a 'footplate', which gives stability to the body's main RigidBody, `CharacterRoot`.
    /// </summary>
    public class RigController : MonoBehaviour
    {
        [SerializeField] private Transform cameraFollowerPose;
        [SerializeField] private GameObject HUDAssetRoot;

        [Tooltip("(Optional) The root level component on either Pamaman or Powoman. \nSee ICVR ➥ Avatar ➥ Prefabs.")]
        [SerializeField] private ICVRAnimatorController animatorController;

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
            UiStartRot = HUDAssetRoot.transform.localRotation;
            UiOffset = HUDAssetRoot.transform.position - transform.position;
            BodyOffset = cameraFollowerPose.position - transform.position;
        }

        private void FixedUpdate()
        {
            transform.localPosition = cameraFollowerPose.localPosition - BodyOffset;

            transform.localRotation = Quaternion.Euler(0f, cameraFollowerPose.localRotation.eulerAngles.y, 0f);

            if (xrState == WebXRState.NORMAL)
            {
                // set position of following UI
                if (HUDAssetRoot.activeInHierarchy)
                {
                    HUDAssetRoot.transform.position = transform.position + (transform.forward * UiOffset.z) + (transform.up * UiOffset.y) + transform.right * UiOffset.x;

                    HUDAssetRoot.transform.rotation = Quaternion.LookRotation(transform.forward) * UiStartRot;
                }
            }
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            transform.localRotation = Quaternion.identity;
            HUDAssetRoot.SetActive(xrState == WebXRState.NORMAL);

            if (animatorController != null)
            {
                if (xrState == WebXRState.NORMAL)
                {
                    animatorController.RelaxArmRig();
                }
                else
                {
                    animatorController.PrepareArmRig();
                }
            }
        }
    }
}
