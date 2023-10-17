using Newtonsoft.Json;
using UnityEngine;

namespace ICVR {
    public class ChainUtils : MonoBehaviour
    {
        public static void InterrogateCanisterResponse(string jsonData)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<CanisterResponse>(jsonData);
                if (response == null)
                {
                    Debug.LogError("Unable to parse as CanisterResponse. Result was null");
                    return;
                }

                if (!string.IsNullOrEmpty(response.ErrorDetails.Canister))
                {
                    Debug.Log("Canister '" + response.ErrorDetails.Canister +
                        " was calling '" + response.ErrorDetails.Method + "'");
                }

                if (!string.IsNullOrEmpty(response.ErrorDetails.RejectMessage))
                {
                    Debug.Log("There is an error in the Canister Response:\n" +
                        response.ErrorDetails.RejectCode.ToString() + ": " + response.ErrorDetails.RejectMessage);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Interrogation failed:\n" + ex.Message);
            }
        }

    }
}