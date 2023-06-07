using UnityEngine;
using WebXR;

namespace ICVR
{
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
