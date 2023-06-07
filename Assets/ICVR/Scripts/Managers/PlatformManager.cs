using UnityEngine;
using WebXR;
using Unity.Plastic.Newtonsoft.Json;
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

        //[SerializeField] private UnityEngine.UI.Text platformDetailText;

        public bool IsVRSupported { get; private set; }

        public WebXRState XrState { get; private set; }

        private string[] VRBrowsers;
        private NavigatorData BrowserData;
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
                //DontDestroyOnLoad(this.gameObject); // option to keep between scenes
            }
        }

        private void Start()
        {
            VRBrowsers = new string[4] { "Oculus Browser", "Firefox Reality", "Wolvic", "Pico Browser" };
        }

        private void Update()
        {
            if (discoveredVR)
            {
                discoveredVR = false;
                //platformDetailText.text = DisplayNavigatorSummary(BrowserData);
                Debug.Log("Discovered VR browser, toggling..");
                WebXRManager.Instance.ToggleVR();
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

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            XrState = state;
        }

        private void OnBrowserInfo(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.Log("Browser info was empty");
                return;
            }

            
            BrowserData = JsonConvert.DeserializeObject<NavigatorData>(message);

            Debug.Log("Nice to meet you, " + BrowserData.Browser.Name + " user. Just checking to see if you're VR ready  8-]");

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

        private IEnumerator TriggerAfterDelay(float delSec)
        {
            yield return new WaitForSeconds(delSec);
            discoveredVR = true;
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
    }
}