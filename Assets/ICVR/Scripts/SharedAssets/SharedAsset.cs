using UnityEngine;

namespace ICVR.SharedAssets 
{ 
    public class SharedAsset : MonoBehaviour 
    { 
        public string Id { get; private set; }
        public bool IsBeingHandled { get; set; }
        public Vector3 DefaultLocation { get; set; }
        public Quaternion DefaultRotation { get; set; }
        public Vector3 DefaultScale { get; set; }

        private SharedAssetManager _manager;

        private void Awake()
        {
            DefaultLocation = transform.position;
            DefaultRotation = transform.rotation;
            DefaultScale = transform.lossyScale;
        }

        private void Start()
        {
            _manager = SharedAssetManager.Instance;
            Id = GetGameObjectPath(gameObject);
            bool isIncluded = _manager.IncludeAssetInRegister(Id, gameObject);
            //if (!isIncluded) { Debug.LogError("Error including " + Id + " in shared assets registry"); }
            //else { Debug.Log("added " + Id + " to shared assets"); }
        }

        private void OnDestroy()
        {
            bool removeResult = _manager.RemoveAssetFromRegister(Id);
            //if (!removeResult) { Debug.LogError("Error removing " + Id + " from shared assets registry"); }
            //else { Debug.Log("removed " + Id + " from shared asset registry"); }
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