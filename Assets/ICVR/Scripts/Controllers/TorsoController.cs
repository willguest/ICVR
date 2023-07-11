using UnityEngine;
using WebXR;

namespace ICVR
{
    /// <summary>
    /// The TorsoController acts as an adjustable pivot between the head and hands.
    /// This is useful for body-oriented events and functions. It is mostly a 
    /// placeholder for future development, but does work to align the feet and
    /// reset the rotation when switching in and out of VR.
    /// </summary>
    public class TorsoController : MonoBehaviour
    {
        [SerializeField] private Transform cameraFollowerPose;

        private WebXRState xrState = WebXRState.NORMAL;

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
        }


        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
        }


        public void Initialise()
        {

        }

        void FixedUpdate()
        {
            if (xrState == WebXRState.NORMAL)
            {
                transform.localRotation = Quaternion.Euler(0f, cameraFollowerPose.localRotation.eulerAngles.y, 0f);
            }
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            transform.localRotation = Quaternion.identity;
        }

    }
}
