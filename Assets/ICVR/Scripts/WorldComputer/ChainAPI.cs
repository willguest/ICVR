using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ICVR
{
    /// <summary>
    /// A helper class, extending Strings to allow for truncated and book-end (AB...YZ) styles
    /// </summary>
    public static class StringExt
    {
#nullable enable
        public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }
        public static string? BookEnd(this string? value, int sideLength, string truncationAffix = "…")
        {
            return value?.Length > (sideLength * 2)
                ? value.Substring(0, sideLength) + truncationAffix + value.Substring(value.Length - sideLength, sideLength)
                : value;
        }
#nullable disable
    }


    public sealed class ChainAPI : MonoBehaviour
    {
        // Singleton pattern
        private static ChainAPI _instance;
        public static ChainAPI Instance { get { return _instance; } }

        // Referece to current user profile
        public UserProfile currentProfile { get; set; }

        // Callback tracking system
        private static Dictionary<int, System.Action<string>> callbacks = new Dictionary<int, System.Action<string>>();
        private int cbIndex;


        private void Awake()
        {
            _instance = this;
        }

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
            try
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
            catch (System.Exception e)
            {
                Debug.LogError("Error reading callbackresponse. Interrogating...\n" +
                    e.Message);

                ChainUtils.InterrogateCanisterResponse(jsonData);
            }
        }


        public void FinaliseCallback(int cbIndex)
        {
            if (callbacks.ContainsKey(cbIndex))
            {
                callbacks.Remove(cbIndex);
            }
        }


        // Connect new canister functions here

        public void ICLogin(System.Action<string> cb)
        {
            CanisterUtils.StartIIAuth(GetCallbackIndex(cb));
        }

        public void ICLogout(System.Action<string> cb)
        {
            CanisterUtils.EndIISession(GetCallbackIndex(cb));
        }

        public void RequestToken(System.Action<string> cb)
        {
            TokenUtils.RequestTokenFromFund(GetCallbackIndex(cb));
        }

    }
}