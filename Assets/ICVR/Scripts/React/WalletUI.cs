using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace ICVR {
    public class WalletUI : MonoBehaviour
    {
        public UserProfile iiUserProfile { get; set;}

        public UserProfile plugUserProfile { get; set; }


        public Button connectToPlug;
        public Button logInToII;

        public float KIC_BALANCE { get; set; }

        private float authTick;

        private bool isPlugConnected = false;
        private bool isIIConnected = false;

        void Start()
        {
            authTick = Time.time;
            KIC_BALANCE = 0f;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.G))
            {
                RequestCoinFromFund();
            }
        }


        public void BeginAuth()
        {
            if (Time.time - authTick > 1.0f)
            {
                authTick = Time.time;
                ReactApi.Instance.ICLogin(onAuth);
            }
            else
            {
                Debug.Log("auth tick stop: " + (Time.time - authTick).ToString());
            }
            
        }

        UserProfile userAuth(string jsonData)
        {
            AuthResponse response = new AuthResponse();
            UserProfile user = new UserProfile();

            try {
                response = JsonConvert.DeserializeObject<AuthResponse>(jsonData);
            }
            catch (System.Exception err) {
                Debug.LogError("Unable to parse AuthResponse:\n" + err);
                return null;
            }

            user.principal = response?.principal ?? "";
            user.accountId = response?.accountId ?? "";
            user.status = response?.result == true ? "Connected" : "Not Connected" ?? "Bugged";
            
            ReactApi.Instance.FinaliseCallback(response.cbIndex);
            return user;
        }

        void onAuth(string jsonData)
        {
            iiUserProfile = userAuth(jsonData);
            isIIConnected = iiUserProfile.status == "Connected";
            NetworkIO.Instance.RequestScreenUpdate();
        }

        void OnRequestConnection(string jsonData)
        {
            plugUserProfile = userAuth(jsonData);
            isPlugConnected = plugUserProfile.status == "Connected";
            NetworkIO.Instance.RequestScreenUpdate();
        }

        public void RequestCoinFromFund()
        {
            ReactApi.Instance.GetCoin(onGetCoin);
        }

        void onGetCoin(string jsonData)
        {
            CoinRequestResponse response = new CoinRequestResponse();
            try
            {
                response = JsonConvert.DeserializeObject<CoinRequestResponse>(jsonData);
                if (response.result)
                {
                    KIC_BALANCE += 1.00f;
                    Debug.Log("Received coin:" + jsonData);
                }
            }
            catch (System.Exception err)
            {
                Debug.LogError("Unable to parse AuthResponse:\n" + err);
            }

            ReactApi.Instance.FinaliseCallback(response.cbIndex);
        }

        public void CheckBalance()
        {
            // canister can use this too to update some UI, once every so often
            // q: best unity heartbeat approach?


        }

        void SendCoinToCanister()
        {
            // burn or transfer?
        }


        public void StartPlugConnection()
        {
            if (authTick - Time.time < -1.0f)
            {
                authTick = Time.time;
                ReactApi.Instance.CheckPlugConnection(onStartConnection);
            }
        }

        void onStartConnection(string jsonData)
        {            
            CheckPlugConnectionResponse response = new CheckPlugConnectionResponse();
            try {
                response = JsonConvert.DeserializeObject<CheckPlugConnectionResponse>(jsonData);
            }
            catch (System.Exception err) {
                Debug.LogError("Unable to parse plug response:\n" + err);
                return;
            }

            isPlugConnected = response?.result ?? false;

            if (!isPlugConnected)
            {
                ReactApi.Instance.RequestPlugConnect(OnRequestConnection);
            }
            else
            {
                NetworkIO.Instance.RequestScreenUpdate();
            }
        }


        

        void GetNfts()
        {
            ReactApi.Instance.GetPlugNfts(OnGetNfts);
        }

        void OnGetNfts(string jsonData)
        {
            var response = JsonConvert.DeserializeObject<GetDabNftsResponse>(jsonData);
            if (response == null)
            {
                Debug.LogError("Unable to parse GetDabNftsResponse -- make sure you are running the project as a WebGL build in browser");
                return;
            }

            Debug.Log("Fetched Plug NFTs with response of:\n");

            //numberOfNfts = response.collections.Count;

            foreach (var collection in response.collections)
            {
                foreach (var token in collection.tokens)
                {
                    Debug.Log(token.collection + " #" + token.index);
                }
            }

            NetworkIO.Instance.RequestScreenUpdate();
            ReactApi.Instance.FinaliseCallback(-1);
        }

        public WalletInfo CheckStatus()
        {
            return new WalletInfo
            {
                PlugStatus = plugUserProfile.status,
                PlugPrincipal = plugUserProfile.principal,
                PlugAccountId = plugUserProfile.accountId,

                IIStatus = iiUserProfile.status,
                IIPrincipal = iiUserProfile.principal,
                IIAccountId = iiUserProfile.accountId
            };
        }
    }
}