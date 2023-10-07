using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ChainAPI : MonoBehaviour
{
    private Dictionary<int, System.Action<string>> callbacks = new Dictionary<int, System.Action<string>>();
    private int cbIndex;

    public static ChainAPI Instance;
    private void Awake()
    {
        Instance = this;
    }

    private int GetCallbackIndex(System.Action<string> cb)
    {
        cbIndex = (int)Mathf.Repeat(++cbIndex, 100);

        if (cb != null)
        {

#if UNITY_EDITOR
            cb.Invoke(string.Empty);
            return cbIndex;
#endif
            callbacks.Add(cbIndex, cb);
        }

        return cbIndex;
    }

    public void HandleCallback(string jsonData)
    {
        Debug.Log("Received JSON Data: " + jsonData);

        var response = JsonConvert.DeserializeObject<CallbackResponse>(jsonData);
        if (response == null)
        {
            Debug.LogError("Unable to parse JSON cbIndex. There is no response");
            return;
        }

        if (!string.IsNullOrEmpty(response.error))
        {
            Debug.LogError("There was an error processing callback " + response.cbIndex);
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

    public void ICLogin(System.Action<string> cb)
    {
        CanisterUtils.StartIIAuth(GetCallbackIndex(cb));
    }



}
