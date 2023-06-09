using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ReactApi : MonoBehaviour
{
    private Dictionary<int, System.Action<string>> cbDict = new Dictionary<int, System.Action<string>>();
    private int cbIndex;

    public static ReactApi Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void HandleCallback(string jsonData)
    {
        Debug.Log("Received JSON Data: " + jsonData);

        var response = JsonConvert.DeserializeObject<CallbackResponse>(jsonData);
        if (response == null) 
        {
            Debug.Log("Unable to parse JSON cbIndex. There must be no callback");
            return;
        }
        
        if (!string.IsNullOrEmpty(response.error))
        {
            return;
        }
            
        if (!cbDict.ContainsKey(response.cbIndex))
        {
            Debug.LogError("The cbIndex=" + response.cbIndex + " does not exist in cbDict");

            Debug.Log("cdDict info:\n" +
                "there are " + cbDict.Keys.Count + " keys,\n" +
                "there are " + cbDict.Values.Count + " values,\n" +
                "dict count " + cbDict.Count);
        }
        
        cbDict[response.cbIndex]?.Invoke(jsonData);
    }

    public void FinaliseCallback(int cbIndex)
    {
        Debug.Log("removing item from cdDict key:" + cbIndex);
        cbDict.Remove(cbIndex);
    }

    public void ICLogin(System.Action<string> cb)
    {
        CanisterUtils.StartIIAuth( GetCallbackIndex(cb) );
    }

    public void GetCoin(System.Action<string> cb)
    {
        CanisterUtils.GetSomeIslandCoin(GetCallbackIndex(cb));
    }

    public void RequestPlugConnect(System.Action<string> cb)
    {
        PlugUtils.RequestConnect( GetCallbackIndex(cb) );
    }
    
    public void CheckPlugConnection(System.Action<string> cb)
    {
        PlugUtils.CheckConnection( GetCallbackIndex(cb) );
    }

    public void GetPlugNfts(System.Action<string> cb)
    {
        PlugUtils.GetPlugNfts( GetCallbackIndex(cb) );
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
            cbDict.Add(cbIndex, cb);
            Debug.Log("added cb to list at index " + cbIndex);
        }

        return cbIndex;
    }
}
