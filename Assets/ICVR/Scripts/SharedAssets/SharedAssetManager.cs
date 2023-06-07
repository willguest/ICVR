using System.Collections.Generic;
using UnityEngine;

namespace ICVR.SharedAssets
{
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

        /*
        void Start()
        {
            
            InitialiseRegister();

            if (searchForSharedAssets)
            {
                SharedAsset[] saList = FindObjectsOfType(typeof(SharedAsset)) as SharedAsset[];
                foreach (SharedAsset asset in saList)
                {
                    asset.Id = asset.gameObject.name + VersionString;
                    AddSharedAssetToRegister(asset.Id, asset.gameObject);
                }
            }
        }

        private void InitialiseRegister()
        {
            for (int t = 0; t < transform.childCount; t++)
            {
                GameObject child = transform.GetChild(t).gameObject;

                // make sure all shared assets have the sharedasset component
                SharedAsset shAs = child.GetComponent<SharedAsset>();
                if (!shAs) {
                    shAs = child.AddComponent<SharedAsset>();
                }

                shAs.Id = shAs.gameObject.name + VersionString;
                AddSharedAssetToRegister(shAs.Id, shAs.gameObject);
            }
        }

        private void AddSharedAssetToRegister(string id, GameObject asset)
        {
            // add unique values to the register
            if (!SharedAssetRegister.ContainsKey(id))
            {
                SharedAssetRegister.Add(id, asset);
            }
        }
        */

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
