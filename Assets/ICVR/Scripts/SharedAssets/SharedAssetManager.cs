/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using UnityEngine;

namespace ICVR.SharedAssets
{
    /// <summary>
    /// This singleton component manages the shared asset register, which keeps track of all the objects whose 
    /// movement and state is shared across the p2p network. For more information 
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/SharedAssets/SharedAssetManager.md"/>
    /// </summary>
    public class SharedAssetManager : MonoBehaviour
    {
        public static SharedAssetManager Instance { get { return _instance; } }
        private static SharedAssetManager _instance;

        public Dictionary<string, GameObject> SharedAssetRegister { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            SharedAssetRegister = new Dictionary<string, GameObject>();
        }

        public GameObject RetrieveAssetFromRegister(string id)
        {
            return SharedAssetRegister[id];
        }

        public bool IncludeAssetInRegister(string Id, GameObject asset)
        {
            if (!SharedAssetRegister.ContainsKey(Id))
            {
                SharedAssetRegister.Add(Id, asset);
                return true;
            }
            return false;
        }

        public bool RemoveAssetFromRegister(string Id)
        {
            if (SharedAssetRegister.ContainsKey(Id))
            {
                SharedAssetRegister.Remove(Id);
                return true;
            }
            return false;
        }

    }
}
