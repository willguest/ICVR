using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace ICVR
{
    public static class StringExt
    {
        public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "�")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }
        public static string? BookEnd(this string? value, int sideLength, string truncationSuffix = "�")
        {
            return value?.Length > (sideLength * 2)
                ? value.Substring(0, sideLength) + truncationSuffix + value.Substring(value.Length - sideLength, sideLength)
                : value;
        }
    }

    public class NetworkIO : MonoBehaviour
    {
        private static NetworkIO _instance;
        public static NetworkIO Instance { get { return _instance; } }

        [DllImport("__Internal")]
        private static extern void PrimeConnection(string sender, string socketURL, int capacity);
        
        [DllImport("__Internal")]
        private static extern void ConfigureAudio();

        [DllImport("__Internal")]
        private static extern void CeaseConnection();


        // interface
        public float NetworkUpdateFrequency { get; private set; }
        public bool ReadyFlag { get; set; }
        public string CurrentUserId { get; private set; }
        public bool IsConnected { get; private set; }

        // unity-assigned objects
        [SerializeField] private string SignalingServerUrl = "https://rtcmulticonnection-sockets.herokuapp.com:443/";
        [SerializeField] private Renderer connectionIndicator;
        [SerializeField] private bool AutoStartConnection;
        
        public delegate void NetworkUserEvent(string connectionStatus, string userId, string payload);
        public event NetworkUserEvent OnNetworkChanged;

        // private variables
        private RtcMultiConnection myConnection;
        private static List<string> connectedUsers;
        private List<string> previousOwnIds;
        private bool readyToReceive = false;

        private RoomManager roomManager;

        // events
        public delegate void ConnectionEvent(bool connectionState);
        public event ConnectionEvent OnConnectionChanged;

        public delegate void RoomJoinEvent(string newUserId);
        public event RoomJoinEvent OnJoinedRoom;

        // UI message
        private string myStatus;
        private string userInfo;

        private bool networkUpdateReady = false;
        private float connectionStartTick = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                //DontDestroyOnLoad(this.gameObject); // uncomment to keep alive between scenes
            }
        }

        private void Start()
        {
            connectedUsers = new List<string>();
            previousOwnIds = new List<string>();

            NetworkUpdateFrequency = 4;

            connectionIndicator.material.EnableKeyword("_EMISSION");
            StartCoroutine(FadeToColour(connectionIndicator, Color.gray, 1.0f));

            if (TryGetComponent(out roomManager) && AutoStartConnection &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                userInfo = "Room Manager found.";
                PrimeConnection(gameObject.name, SignalingServerUrl, roomManager.MaxPeers);
            }

            myStatus = "Awake";
            CurrentUserId = "";
            userInfo = "";
            networkUpdateReady = true;
        }

        

        private void Update()
        {
            if (ReadyFlag)
            {
                readyToReceive = true;
                ReadyFlag = false;
            }

            if (networkUpdateReady)
            {
                networkUpdateReady = false;
                OnNetworkChanged?.Invoke(myStatus, CurrentUserId, userInfo);
            }
        }

        public void StartConnection()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                CreateSession();
            }
        }

        public void StopConnection()
        {
            CloseConnection();
        }

        public void ToggleConnection()
        {
            // 3-second cool down
            if ((Time.time - connectionStartTick) < 3.0f) return;
            connectionStartTick = Time.time;

            if (IsConnected)
            {
                CloseConnection();
                Debug.Log("Connection closed");
            }
            else
            {
                if (Application.platform != RuntimePlatform.WindowsEditor)
                {
                    CreateSession();
                }
            }
        }
        

        private void CloseConnection()
        {
            readyToReceive = false;
            IsConnected = false;

            connectedUsers.Clear();
            OnConnectionChanged?.Invoke(false);
            myStatus = "Not connected";

            StartCoroutine(FadeToColour(connectionIndicator, Color.red, 2f));
            AvatarManager.Instance?.ResetScene();

            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                CeaseConnection();
            }
        }

        private void OnFinishedLoadingRTC(string message)
        {
            try
            {
                myConnection = JsonConvert.DeserializeObject<RtcMultiConnection>(message);
                CurrentUserId = myConnection.Userid;
                if (!previousOwnIds.Contains(CurrentUserId))
                {
                    previousOwnIds.Add(CurrentUserId);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Failed creating connection object:\n" + e.ToString());
                myConnection = null;
                myStatus = "Error";
                userInfo = e.ToString();
                networkUpdateReady = true;
                return;
            }

            // update UI
            StartCoroutine(FadeToColour(connectionIndicator, Color.yellow, 2f));

            myStatus = "Started";

            networkUpdateReady = true;

            Debug.Log("WebRTC connection started, Performing room check...");
            roomManager?.CheckForRooms();
        }

        public void CreateSession()
        {
            if (roomManager)
            {
                PrimeConnection(gameObject.name, SignalingServerUrl, roomManager.MaxPeers);
                roomManager?.CreateRoom();
            }
            else
            {
                PrimeConnection(gameObject.name, SignalingServerUrl, 6);
            }
            
        }

        private void RoomCheckComplete(string message)
        {
            //Debug.Log("Room check complete. " + message);
        }

        private void OnAudioConfigured(string message)
        {
            // update UI
            StartCoroutine(FadeToColour(connectionIndicator, Color.green, 2f));


            ReadyFlag = true;
            myStatus = "Ready";
            networkUpdateReady = true;

            OnConnectionChanged.Invoke(true);
        }

        private void OnUserOnline(string userid)
        {
            if (!connectedUsers.Contains(userid))
            {
                Debug.Log(userid + " is online.");
                ReadyFlag = true;
                OnConnectionChanged.Invoke(true);
                OnJoinedRoom.Invoke(CurrentUserId);
            }
        }

        private void OnDestroy()
        {
            connectedUsers.Clear();
            readyToReceive = false;

            string mins = (Time.time / 60.0f).ToString();
            Debug.Log("Session ended after " + mins + " mins");
        }

        public void SignalReadiness()
        {
            ReadyFlag = true;
        }

        public void RequestScreenUpdate()
        {
            networkUpdateReady = true;
        }

        private void RemoveAvatar(string avatarId)
        {
            if (!string.IsNullOrEmpty(avatarId))
            {
                DeleteAvatar(avatarId);
            }
        }

        private void ReceivePoseData(string message)
        {
            if (!readyToReceive)
            {
                return;
            }
            else if (String.IsNullOrEmpty(message))
            {
                Debug.Log("Data message was empty");
                return;
            }

            try
            {
                NodeInputData inputData = JsonConvert.DeserializeObject<NodeInputData>(message);
                HandlePoseData(inputData);
            }
            catch (Exception e)
            {
                Debug.Log("Error in pose deserialisation: " + e.Message);
            }
        }

        private void HandlePoseData(NodeInputData data)
        {
            if (String.IsNullOrEmpty(data.Data))
            {
                Debug.Log("null data.Data");
                return;
            }

            AvatarManager.Instance.ProcessAvatarData(data);
        }

        private void OnConnectedToNetwork(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                Debug.Log("Connection event was empty");
                return;
            }

            ConnectionData cdata = JsonConvert.DeserializeObject<ConnectionData>(message);
            if (!connectedUsers.Contains(cdata.Userid) && !previousOwnIds.Contains(cdata.Userid))
            {
                Debug.Log("user showed up on network: " + cdata.Userid);
                connectedUsers.Add(cdata.Userid);
                roomManager?.CheckForRooms();
            }
            else
            {
                Debug.Log("user '" + cdata.Userid + "' already known or is self");
            }

            StartCoroutine(FadeToColour(connectionIndicator, Color.green, 2f));

            IsConnected = true;
            ReadyFlag = true;

            OnJoinedRoom.Invoke(CurrentUserId);
            networkUpdateReady = true;
        }

        private void OnDisconnectedFromNetwork(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                Debug.Log("Disconnection event was empty");
                return;
            }

            try
            {
                ConnectionData ddata = JsonConvert.DeserializeObject<ConnectionData>(message);
                DeleteAvatar(ddata.Userid);
                networkUpdateReady = true;
            }
            catch (Exception e)
            {
                Debug.Log("error in disconnection:" + e.Message);
            }
        }

        private void DeleteAvatar(string avatarId)
        {
            if (connectedUsers.Contains(avatarId))
            {
                connectedUsers.Remove(avatarId);
                if (connectedUsers.Count < 1)
                {
                    StartCoroutine(FadeToColour(connectionIndicator, Color.yellow, 1.0f));
                    myStatus = "waiting for others";
                }
            }

            Debug.Log("Removing avatar: " + avatarId);
            AvatarManager.Instance.RemovePlayerAvatar(avatarId);
            roomManager?.CheckForRooms();
        }


        private IEnumerator FadeToColour(Renderer targetRenderer, Color endColour, float duration)
        {
            Color startColour = targetRenderer.material.color;
            float time = 0;

            while (time < duration)
            {
                Color cl = Color.Lerp(startColour, endColour, time / duration);
                targetRenderer.material.SetColor("_EmissionColor", cl);
                time += Time.deltaTime;
                yield return null;
            }
            
            targetRenderer.material.SetColor("_EmissionColor", endColour);
            
        }

    }
}