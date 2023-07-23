using System.IO;
using UnityEngine;


public class StreamingAsset : MonoBehaviour
{
    [SerializeField] private Object streamingAssetFolder;
    [SerializeField] private string extension;
    [SerializeField] private string[] filePaths;

    private string[] dataStore;
    public int currDataIndex = -1;

    public string[] BuildDataStore()
    {
        dataStore = new string[filePaths.Length];
        for (int s = 0; s < filePaths.Length; s++)
        {
            dataStore[s] = Application.streamingAssetsPath + filePaths[s];
        }
        
        //Debug.Log("data store created with " + dataStore.Length + " elements: " + string.Join(",", dataStore));
        return dataStore;
    }

    public string GetCurrentFileUrl()
    {
        return Path.GetFileNameWithoutExtension(dataStore[currDataIndex]);
    }

    public string GetFirstFileUrl()
    {
        if (dataStore.Length < 1)
        {
            return "";
        }

        currDataIndex = 0;
        return dataStore[0];
    }

    public string GetRandomFileUrl()
    {
        if (dataStore.Length < 1)
        {
            return "";
        }

        int randomTrackNo = Random.Range(0, dataStore.Length);
        currDataIndex = randomTrackNo;
        return dataStore[randomTrackNo];
    }

    public string GetNextFileUrl()
    {
        if (dataStore.Length < 1)
        {
            return "";
        }

        if (currDataIndex == -1)
        {
            currDataIndex = 0;
        }
        else if ((currDataIndex + 1) < dataStore.Length)
        {
            currDataIndex++;
        }
        else
        {
            currDataIndex = 0;
        }

        return dataStore[currDataIndex];
    }

    public string GetPrevFileUrl()
    {
        if (dataStore.Length < 1)
        {
            return "";
        }

        if (currDataIndex == -1)
        {
            currDataIndex = 0;
        }
        else if (dataStore.Length > 1 && currDataIndex == 0)
        {
            currDataIndex = dataStore.Length - 1;
        }
        else if (dataStore.Length > 1 && currDataIndex > 0)
        {
            currDataIndex--;
        }
        else
        {
            currDataIndex = 0;
        }
        return dataStore[currDataIndex];
    }

}

