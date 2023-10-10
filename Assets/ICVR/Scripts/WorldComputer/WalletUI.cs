using Newtonsoft.Json;
using UnityEngine;

namespace ICVR
{
    public class WalletUI : MonoBehaviour
    {
        public float KIC_BALANCE { get; private set; }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.ScrollLock))
            {
                RequestCoinFromFund();
            }
        }

        public void RequestCoinFromFund()
        {
            ChainAPI.Instance.GetCoin(onGetCoin);
        }

        private void onGetCoin(string jsonData)
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

            ChainAPI.Instance.FinaliseCallback(response.cbIndex);
        }
    }
}