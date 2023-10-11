using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ICVR
{
    public sealed class ChainAPI : MonoBehaviour
    {
        // Singleton pattern
        private static ChainAPI _instance;
        public static ChainAPI Instance { get { return _instance; } }

        private static Dictionary<int, System.Action<string>> callbacks = new Dictionary<int, System.Action<string>>();
        private int cbIndex;

        private int GetCallbackIndex(System.Action<string> cb)
        {
            cbIndex = (int)Mathf.Repeat(++cbIndex, 100);
            if (cb != null)
            {
#if UNITY_EDITOR
                cb.Invoke(string.Empty);
                return cbIndex;
#else
                callbacks.Add(cbIndex, cb);
#endif
            }
            return cbIndex;
        }

        public void HandleCallback(string jsonData)
        {
            var response = JsonConvert.DeserializeObject<CallbackResponse>(jsonData);
            if (response == null)
            {
                Debug.LogError("Unable to parse JSON cbIndex. There is no response");
                return;
            }

            if (!string.IsNullOrEmpty(response.error))
            {
                Debug.LogError("There was an error processing callback " + 
                    response.cbIndex + "\n" + response.error);
                return;
            }

            if (!callbacks.ContainsKey(response.cbIndex))
            {
                Debug.LogError("The cbIndex=" + response.cbIndex + " does not exist in callbacks");
                return;
            }

            callbacks[response.cbIndex]?.Invoke(jsonData);
        }

        public void FinaliseCallback(int cbIndex)
        {
            callbacks.Remove(cbIndex);
        }

        // Add new canister functions here


        public void ICLogin(System.Action<string> cb)
        {
            CanisterUtils.StartIIAuth(GetCallbackIndex(cb));
        }

        public void GetCoin(System.Action<string> cb)
        {
            TokenUtils.GetSomeIslandCoin(GetCallbackIndex(cb));
        }

    }
}