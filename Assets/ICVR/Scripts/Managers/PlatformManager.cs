/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;
using WebXR;

namespace ICVR
{
    /// <summary>
    /// This class looks after mode detection and switching, interfacing with the WebXRManager. 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Managers/PlatformManager.md"/>
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        private static PlatformManager _instance;
        public static PlatformManager Instance { get { return _instance; } }

        public bool IsVRSupported { get; private set; }

        public WebXRState XrState { get; private set; }

        private bool discoveredVR = false;

        public void StartVR()
        {
            discoveredVR = true;
        }

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
            IsVRSupported = capabilities.canPresentVR;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            XrState = state;
        }

    }
}