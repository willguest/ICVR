using Newtonsoft.Json;
using ICVR.SharedAssets;
using System.Collections.Generic;
using UnityEngine;

namespace ICVR
{
    public class AvatarManager : MonoBehaviour
    {
        private static AvatarManager _instance;
        public static AvatarManager Instance { get { return _instance; } }

        [SerializeField] private GameObject avatarTemplate;
        [SerializeField] private UnityEngine.UI.Text chatText;

        private Dictionary<string, GameObject> otherPlayers;
        private Dictionary<string, AvatarController> avatarControllers;

        public bool AudioChannelOpen { get; set; }

        // events
        public delegate void DictionaryChanged(int noPlayersNow);
        public event DictionaryChanged OnDictionaryChanged;

        private string messageBuffer = "";
        private bool newChatMessageReady = false;

        private bool readyToCreateAvatar = false;
        private NodeDataFrame currentDataFrame;



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
        }

        private void OnDestroy()
        {
            foreach (string user in otherPlayers.Keys)
            {
                RemovePlayerAvatar(user);
            }
            otherPlayers.Clear();
            otherPlayers = null;
        }

        public void ResetScene()
        {
            //remove all entries from lists
            foreach (string user in otherPlayers.Keys)
            {
                RemovePlayerAvatar(user);
            }

            // make sure all stray children are removed
            for (int t = 0; t < transform.childCount; t++)
            {
                Destroy(transform.GetChild(t).gameObject);
            }

            // make new dictionaries
            otherPlayers.Clear();
            avatarControllers.Clear();
            otherPlayers = new Dictionary<string, GameObject>();
            avatarControllers = new Dictionary<string, AvatarController>();
            AudioChannelOpen = false;
        }

        void Start()
        {
            otherPlayers = new Dictionary<string, GameObject>();
            avatarControllers = new Dictionary<string, AvatarController>();
            AudioChannelOpen = false;
        }

        private void Update()
        {
            if (readyToCreateAvatar && currentDataFrame != null)
            {
                createNewPlayerAvatar(currentDataFrame);
                readyToCreateAvatar = false;
                currentDataFrame = null;
            }

            if (newChatMessageReady)
            {
                newChatMessageReady = false;
                chatText.text += messageBuffer;
            }
        }

        public void ProcessAvatarData(NodeInputData nodeFrame)
        {
            NodeDataFrame nodeData = JsonConvert.DeserializeObject<NodeDataFrame>(nodeFrame.Data);

            if (avatarControllers.ContainsKey(nodeData.Id))
            {
                avatarControllers[nodeData.Id].UpdateAvatar(nodeFrame.Latency, nodeData);

                if (nodeData.EventType == AvatarEventType.Chat)
                {
                    PostChatMessage(nodeData.Id, nodeData.EventData);
                }
            }
            else
            {
                currentDataFrame = nodeData;
                readyToCreateAvatar = true;
            }
        }

        private void PostChatMessage(string id, string chatData)
        {
            AvatarChatData acd = JsonConvert.DeserializeObject<AvatarChatData>(chatData);

            if (acd.Scope == "broadcast")
            {
                messageBuffer = id + ":\n" + acd.Message + "\n";
                newChatMessageReady = true;
            }
        }


        public void createNewPlayerAvatar(NodeDataFrame nodeFrame)
        {
            GameObject newPlayerObject = Instantiate(avatarTemplate, nodeFrame.HeadPosition, nodeFrame.HeadRotation, this.transform);
            newPlayerObject.name = nodeFrame.Id;

            if (!otherPlayers.ContainsKey(nodeFrame.Id))
            {
                otherPlayers.Add(nodeFrame.Id, newPlayerObject);
            }

            if (!avatarControllers.ContainsKey(newPlayerObject.name))
            {
                AvatarController avCon = newPlayerObject.GetComponent<AvatarController>();
                avatarControllers.Add(nodeFrame.Id, avCon);
                avCon.Initialise();
            }

            OnDictionaryChanged?.Invoke(otherPlayers.Count);
        }

        public void RemovePlayerAvatar(string userId)
        {
            if (otherPlayers.TryGetValue(userId, out GameObject playerObject))
            {
                playerObject.GetComponent<AvatarController>().EndSession();
                avatarControllers.Remove(userId);
                otherPlayers.Remove(userId);
                Destroy(playerObject);

                if (otherPlayers.Count == 0)
                {
                    AudioChannelOpen = false;
                }

                OnDictionaryChanged?.Invoke(otherPlayers.Count);
            }
        }
    }
}