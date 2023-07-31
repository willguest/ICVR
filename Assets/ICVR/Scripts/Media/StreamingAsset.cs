/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.IO;
using UnityEngine;

namespace ICVR
{
    /// <summary>
    /// Identifies the Object as containing media in the `StreamingAssets` folder which is accessed on-
    /// demand by the MediaController. This class uses the StreamingAssetHandler in Unity to populate 
    /// the file paths that will be loaded at runtime.
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Media/StreamingAsset.md"/>
    /// </summary>
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
}

