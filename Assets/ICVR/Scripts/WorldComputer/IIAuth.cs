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

        private float authTick;
        private bool isIIConnected = false;

        void Start()
        {
            authTick = Time.time;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Pause))
            {
                BeginAuth();
            }
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
        }

        /// <summary>
        /// Check current II user values
        /// </summary>
        /// <returns></returns>
        public UserProfile GetUserProfile()
        {
            return new UserProfile
            {
                status = iiUserProfile.status,
                principal = iiUserProfile.principal,
                accountId = iiUserProfile.accountId
            };
        }

        private void onAuth(string jsonData)
        {
            iiUserProfile = UserAuth(jsonData);
            isIIConnected = (iiUserProfile.status == "Connected");

            Debug.Log("II authentication finished with status: " + iiUserProfile.status);

            // Request Screen Update
            // ... update relevant object with 'connected' status

        }
        private UserProfile UserAuth(string jsonData)
        {
            AuthResponse response;
            UserProfile user = new UserProfile();

            try
            {
                response = JsonConvert.DeserializeObject<AuthResponse>(jsonData);
            }
            catch (System.Exception err)
            {
                Debug.LogError("Unable to parse AuthResponse:\n" + err);
                return null;
            }

            user.principal = response?.principal ?? "";
            user.accountId = response?.accountId ?? "";
            user.status = response?.result == true ? "Connected" : "Not Connected" ?? "Bugged";

            ChainAPI.Instance.FinaliseCallback(response.cbIndex);
            return user;
        }
        private void OnDisconnect()
        {
            isIIConnected = false;
        }

    }
}
