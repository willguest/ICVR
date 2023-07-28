using UnityEngine;
using WebXR;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Collections;

namespace ICVR
{
    public class PlatformManager : MonoBehaviour
    {
        private static PlatformManager _instance;
        public static PlatformManager Instance { get { return _instance; } }

        public bool IsVRSupported { get; private set; }

        public WebXRState XrState { get; private set; }

        private bool discoveredVR = false;


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


        private void Update()
        {
            if (discoveredVR)
            {
                Debug.Log("Initiating VR...");

                WebXRManager.Instance.ToggleVR();
                discoveredVR = false;
            }
        }

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnXRCapabilitiesUpdate += CheckCapabilties;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnXRCapabilitiesUpdate -= CheckCapabilties;
        }

        private void CheckCapabilties(WebXRDisplayCapabilities capabilities)
        {
            Debug.Log("haz VR? " + capabilities.canPresentVR.ToString());
            IsVRSupported = capabilities.canPresentVR;
        }

        public void StartVR()
        {
            discoveredVR = true;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            XrState = state;
        }

    }
}