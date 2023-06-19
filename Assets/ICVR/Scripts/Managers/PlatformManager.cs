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

        [DllImport("__Internal")]
        private static extern void GetBrowserInfo(string sender);

        public bool IsVRSupported { get; private set; }

        public WebXRState XrState { get; private set; }

        private bool discoveredVR = false;

        //private string[] VRBrowsers;
        //private NavigatorData BrowserData;
        //[SerializeField] private UnityEngine.UI.Text platformDetailText;

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

        private void Start()
        {
            //VRBrowsers = new string[4] { "Oculus Browser", "Firefox Reality", "Wolvic", "Pico Browser" };
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


        /* Browser Interrogation functions (currently unused)
        private void OnBrowserInfo(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.Log("Browser info was empty");
                return;
            }


            BrowserData = JsonConvert.DeserializeObject<NavigatorData>(message);

            Debug.Log("Hi " + BrowserData.Browser.Name + " user. Just checking to see if you're VR-ready  8-]");

            for (int s = 0; s < VRBrowsers.Length; s++)
            {
                if (BrowserData.Browser.Name == VRBrowsers[s])
                {
                    Debug.Log("VR check success, using " + VRBrowsers[s]);
                    //StartCoroutine(TriggerAfterDelay(2.0f));
                    break;
                }
            }
        }


        private string DisplayNavigatorSummary(NavigatorData data)
        {
            string navDataText =
                "\n\tNavigator Data" + '\n' +
                "\n\tBrowser: " + data.Browser.Name + " (" + data.Browser.Version + ")" +
                "\n\tDevice: " + data.Device.Model + " (" + data.Device.Type + ")" +
                "\n\tEngine: " + data.Engine.Name + " (" + data.Engine.Version + ")";

            return navDataText;
        }

        */
    }
}