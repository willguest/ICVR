using Newtonsoft.Json;
using UnityEngine;

using UnityEngine.UI;

namespace ICVR
{
    public class IIAuth : MonoBehaviour
    {
        public UserProfile iiUserProfile { get; set; }

        private float authTick;
        private bool isIIConnected = false; 

        // Start is called before the first frame update
        void Start()
        {
            authTick = Time.time;
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
        /// Check current Internet Identity values
        /// </summary>
        /// <returns></returns>
        public UserProfile CheckStatus()
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
            isIIConnected = iiUserProfile.status == "Connected";

            // Request Screen Update
            // ... update relevant object with 'connected' status

        }

        private UserProfile UserAuth(string jsonData)
        {
            AuthResponse response = new AuthResponse();
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
