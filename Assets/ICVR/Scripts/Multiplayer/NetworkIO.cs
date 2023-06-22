using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace ICVR
{
    public class NetworkIO : MonoBehaviour
    {
        // singleton
        private static NetworkIO _instance;
        public static NetworkIO Instance { get { return _instance; } }

        // js functions
        [DllImport("__Internal")]
        private static extern void CreateNewConnection(string sender, string socketURL, int roomSize);

        [DllImport("__Internal")]
        private static extern void StartConnection(string roomId);

        [DllImport("__Internal")]
        private static extern void CeaseConnection();

        public delegate void NetworkUserEvent(string connectionStatus, string userId, string payload);
        public event NetworkUserEvent OnNetworkChanged;

        // interface
        public float NetworkUpdateFrequency { get; private set; }
        public bool ReadyFlag { get; set; }
        public string CurrentUserId { get; private set; }
        public bool IsConnected { get; private set; }

        // unity-assigned objects
        [SerializeField] private string SignalingServerUrl = "https://rtcmulticonnection-sockets.herokuapp.com:443/";
        
        [SerializeField] private Renderer connectionIndicator;

        // private variables
        private RtcMultiConnection myConnection;
        private static List<string> connectedUsers;
        private List<string> previousOwnIds;

        private bool readyToReceive = false;
        private bool WaitingForOthers = false;
        
        // events
        public delegate void ConnectionEvent(bool connectionState);
        public event ConnectionEvent OnConnectionChanged;

        public delegate void RoomJoinEvent(string newUserId);
        public event RoomJoinEvent OnJoinedRoom;

        // private connection variables
        private string roomPrefix = "ICVR-";
        private string roomString = "";
        private int RoomCapacity = 6;
        private int roomNumber = 0;

        private float connectionStartTick = 0;

        private string myStatus;
        private string userInfo;
        private bool networkUpdateReady = false;


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                //DontDestroyOnLoad(this.gameObject); // option to keep between scenes
            }
        }

        private void Start()
        {
            connectedUsers = new List<string>();
            previousOwnIds = new List<string>();

            connectionIndicator.material.EnableKeyword("_EMISSION");
            StartCoroutine(FadeToColour(connectionIndicator, Color.gray, 1.0f));

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

        private int IncrementRoomId()
        {
            roomNumber++;
            roomString = roomPrefix + roomNumber.ToString();
            networkUpdateReady = true;
            return roomNumber;
        }

        public void OpenJoin()
        {
            roomString = roomPrefix + roomNumber.ToString();
            StartConnection(roomString);
        }

        private void OnDisable()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                CeaseConnection();
            }
        }


        public void StartRtcConnection()
        {
            // 3-second cool down
            if ((Time.time - connectionStartTick) < 3.0f) return;
            connectionStartTick = Time.time;

            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                CreateNewConnection(gameObject.name, SignalingServerUrl, RoomCapacity);
            }
        }

        public void StopRtcConnection()
        {
            // 3-second cool down
            if ((Time.time - connectionStartTick) < 3.0f) return;
            connectionStartTick = Time.time;

            if (IsConnected || WaitingForOthers)
            {
                CloseConnection();
            }
        }

        private void RoomIsFull(string roomId)
        {
            IncrementRoomId();
            OpenJoin();
        }

        private void CloseConnection()
        {
            readyToReceive = false;
            IsConnected = false;
            WaitingForOthers = false;

            connectedUsers.Clear();
            OnConnectionChanged.Invoke(false);

            StartCoroutine(FadeToColour(connectionIndicator, Color.red, 2f));

            CeaseConnection();
            AvatarManager.Instance.ResetScene();
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
                networkUpdateReady = true;
                return;
            }

            // update UI
            StartCoroutine(FadeToColour(connectionIndicator, Color.yellow, 2f));

            myStatus = "Started";
            WaitingForOthers = true;
            networkUpdateReady = true;

            // open or join room
            roomString = roomPrefix + roomNumber.ToString();
            StartConnection(roomString);
        }



        private void OnConnectionStarted(string message)
        {
            ReadyFlag = true;
            myStatus = "Connected";
            WaitingForOthers = true;
            networkUpdateReady = true;
            OnConnectionChanged.Invoke(true);
        }

        private void OnUserOnline(string userid)
        {
            if (!connectedUsers.Contains(userid))
            {
                Debug.Log(userid + " is online.");
                ReadyFlag = true;
                //OnConnectionChanged.Invoke(true);
                OnJoinedRoom.Invoke(CurrentUserId);
            }
        }

        private void OnDestroy()
        {
            connectedUsers.Clear();
            readyToReceive = false;

            string mins = (Time.time / 60.0f).ToString();
            Debug.Log("Session destroyed after " + mins + " mins");
        }

        public void SignalReadiness()
        {
            ReadyFlag = true;
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
                Debug.Log("User showed up on network: " + cdata.Userid);
                connectedUsers.Add(cdata.Userid);
            }
            else
            {
                Debug.Log("User '" + cdata.Userid + "' already known or is self");
            }

            StartCoroutine(FadeToColour(connectionIndicator, Color.green, 2f));

            myStatus = "Connected";
            IsConnected = true;
            ReadyFlag = true;
            WaitingForOthers = false;

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
                myStatus = "Disconnected";
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
                    myStatus = "Waiting for others";
                }
            }

            Debug.Log("removing avatar:" + avatarId);
            AvatarManager.Instance.RemovePlayerAvatar(avatarId);
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