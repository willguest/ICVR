using Newtonsoft.Json;
using UnityEngine;

namespace ICVR
{
    [System.Serializable]
    public class UserProfile
    {
        public string principal;
        public string accountId;
        public string status;
    }

    public class IIAuth : MonoBehaviour
    {
        public UserProfile iiUserProfile { get; private set; }

        [SerializeField] private UnityEngine.UI.Text connectionResult;
        [SerializeField] private UnityEngine.UI.Image promptImage;

        private bool screenUpdateReady = false;
        private string connectionResultString = string.Empty;

        private float authTick;
        private bool isIIConnected = false;

        void Start()
        {
            authTick = Time.time;
        }

        void Update()
        {
            if (screenUpdateReady)
            {
                screenUpdateReady = false;
                connectionResult.text = connectionResultString;
            }
        }

        #region Public Interface


        public void OnGetFocus()
        {
            if (!isIIConnected)
            {
                promptImage.gameObject.SetActive(true);
            }
        }

        public void OnLoseFocus()
        {
            promptImage.gameObject.SetActive(false);
        }


        /// <summary>
        /// Begin Internet Identity authentication
        /// </summary>
        public void BeginAuth()
        {
            if (!isIIConnected)
            {
                if (Time.time - authTick > 1.0f)
                {
                    authTick = Time.time;
                    ChainAPI.Instance.ICLogin(onAuth);
                }
            }
            else
            {
                ChainAPI.Instance.currentProfile = GetUserProfile();
            }
        }

        public void EndAuth()
        {
            ChainAPI.Instance.ICLogout(OnConfirmExit);
        }

        #endregion Public Interface


        #region Private Functions

        private UserProfile GetUserProfile()
        {
            return new UserProfile
            {
                status = iiUserProfile?.status ?? "",
                principal = iiUserProfile?.principal ?? "",
                accountId = iiUserProfile?.accountId ?? ""
            };
        }

        private void onAuth(string jsonData)
        {
            iiUserProfile = ProcessAuthResponse(jsonData);
            
            string bkPrincipal = iiUserProfile.principal.BookEnd(9, "...");
            connectionResultString = "Status: " + iiUserProfile.status +
                                    "\nPrincipal: " + bkPrincipal;
            screenUpdateReady = true;
        }

        private void OnConfirmExit(string jsonData)
        {
            Debug.Log("Logout confirmed:\n" + jsonData);
            iiUserProfile = null;
            isIIConnected = false;
        }

        private UserProfile ProcessAuthResponse(string jsonData)
        {
            AuthResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<AuthResponse>(jsonData);
            }
            catch (System.Exception err)
            {
                Debug.LogError("Unable to parse AuthResponse:\n" + err);
                return null;
            }
            
            // create new user profile
            UserProfile newProfile = new UserProfile()
            {
                principal = response?.principal ?? "",
                accountId = response?.accountId ?? "",
                status = response?.result == true ? "Authenticated" : "Not Authenticated" ?? "Bugged"
            };

            // keep local state
            isIIConnected = response?.result ?? false;

            // Store principal for later use
            ChainAPI.Instance.currentProfile = newProfile;
            ChainAPI.Instance.FinaliseCallback(response?.cbIndex ?? 0);

            return newProfile;
        }

        private void OnDisconnect()
        {
            isIIConnected = false;
        }


        #endregion Private Functions
    }
}
