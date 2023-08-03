/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace ICVR
{
    /// <summary>
    /// Handles the visualisation of the avatars, and receives messages about network events.
    /// <para /><see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Multiplayer/AvatarManager.md"/>
    /// </summary>
    public class AvatarManager : MonoBehaviour
    {
        private static AvatarManager _instance;
        public static AvatarManager Instance { get { return _instance; } }

        [SerializeField] private GameObject avatarTemplate;

        public bool AudioChannelOpen { get; set; }

        // events
        public delegate void DictionaryChanged(int noPlayersNow);
        public event DictionaryChanged OnDictionaryChanged;

        private Dictionary<string, GameObject> otherPlayers;
        private Dictionary<string, AvatarController> avatarControllers;

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
                CreateNewPlayerAvatar(currentDataFrame);
                readyToCreateAvatar = false;
                currentDataFrame = null;
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


        public void ProcessAvatarData(NodeInputData nodeFrame)
        {
            NodeDataFrame nodeData = JsonConvert.DeserializeObject<NodeDataFrame>(nodeFrame.Data);

            if (avatarControllers.ContainsKey(nodeData.Id))
            {
                avatarControllers[nodeData.Id].UpdateAvatar(nodeFrame.Latency, nodeData);
            }
            else
            {
                currentDataFrame = nodeData;
                readyToCreateAvatar = true;
            }
        }


        public void CreateNewPlayerAvatar(NodeDataFrame nodeFrame)
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