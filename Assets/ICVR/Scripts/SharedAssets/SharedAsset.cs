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