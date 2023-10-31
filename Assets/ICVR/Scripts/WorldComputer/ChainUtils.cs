using Newtonsoft.Json;
using UnityEngine;

namespace ICVR {
    public class ChainUtils : MonoBehaviour
    {
        public static void InterrogateCanisterResponse(string jsonData)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<CanisterResponseError>(jsonData);
                if (response == null)
                {
                    Debug.LogError("Unable to parse CanisterResponse. Result was null");
                    return;
                }

                if (!string.IsNullOrEmpty(response.ErrorCode))
                {
                    Debug.Log("Error code: " + response.ErrorCode);
                    return;
                }

                if (!string.IsNullOrEmpty(response.RejectMessage))
                {
                    Debug.Log("Reject Info :\n" + response.RejectCode.ToString() + 
                        ": " + response.RejectMessage);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Interrogation result:\n" + ex.Message);
            }
        }

    }
}