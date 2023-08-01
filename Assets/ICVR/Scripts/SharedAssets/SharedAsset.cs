/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace ICVR.SharedAssets 
{
    /// <summary>
    /// This component makes the GameObject indexable in the shared asset register. 
    /// During interaction, the presence of this component triggers p2p data transfer.
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/SharedAssets/SharedAsset.md"/>
    /// </summary>
    public class SharedAsset : MonoBehaviour 
    { 
        public bool IsBeingHandled { get; set; }

        public string Id { get; private set; }
        public Vector3 DefaultLocation { get; private set; }
        public Quaternion DefaultRotation { get; private set; }
        public Vector3 DefaultScale { get; private set; }

        private bool isNetworkAvailable = false;
        private SharedAssetManager _manager;

        private void Awake()
        {
            DefaultLocation = transform.position;
            DefaultRotation = transform.rotation;
            DefaultScale = transform.lossyScale;

            if (SharedAssetManager.Instance)
            {
                isNetworkAvailable = true;
            }
        }

        private void Start()
        {
            if (isNetworkAvailable)
            {
                _manager = SharedAssetManager.Instance;
                Id = GetGameObjectPath(gameObject);
                _manager.IncludeAssetInRegister(Id, gameObject);
            }
        }

        private void OnDestroy()
        {
            if (isNetworkAvailable)
            {
                bool removeResult = _manager.RemoveAssetFromRegister(Id);
            }
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    } 
}