/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

//using UnityEngine.Localization.Settings;
using System.Collections;

namespace ICVR {
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private Text roomName;
        [SerializeField] private ControlDynamics roomSelector;
        [SerializeField] private Transform roomParent;
        [SerializeField] private Transform roomTemplate;

        [SerializeField] private Button CreateButton;
        [SerializeField] private Button JoinButton;

        private RoomObject[] rooms;
        private string currentRoom;

        // text assets for name generation
        private TextAsset fourLW;
        private TextAsset fiveLW;

        public int MaxPeers
        {
            get { return maxPeers; }
            set { maxPeers = value; }
        }

        private int maxPeers = 6;
        private bool publicRoom = true;

        private System.Random randomSeed;
        private string roomString = "";
        private bool newRoomFound = false;


        [DllImport("__Internal")]
        private static extern void RoomCheck(string sender);

        [DllImport("__Internal")]
        private static extern void OpenRoom(string sender, string roomId, int roomSize, bool isPublic);

        [DllImport("__Internal")]
        private static extern void SetRoomSize(string sender, string roomId, int roomSize);

        [DllImport("__Internal")]
        private static extern void JoinRoom(string sender, string roomId);

        void Start()
        {
            randomSeed = new System.Random();
            fourLW = Resources.Load("4LetterWords") as TextAsset;
            fiveLW = Resources.Load("5LetterWords") as TextAsset;
            roomName.text = GetNewRoomName();
        }



        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                RoomCheck(gameObject.name);
            }
            else if (Input.GetKeyUp(KeyCode.Insert))
            {
                var ro = new RoomObject
                {
                    MaxParticipantsAllowed = 2,
                    Sessionid = "sally walk",
                    Participants = new string[0]
                };

                ReceiveRoomInfo(JsonConvert.SerializeObject(ro));

            }

            if (newRoomFound)
            {
                newRoomFound = false;
                MakeRoomListItem(rooms[rooms.Length - 1]);
            }
        }

        private void RoomIsFull()
        {
            // to do
        }

        public void CheckForRooms()
        {
            RoomCheck(gameObject.name);
        }

        public void UpdateRoomName()
        {
            roomName.text = GetNewRoomName();
        }

        private string GetNewRoomName()
        {
            string[] words = new string[2] { GetRandomWord(fiveLW.bytes, 5), GetRandomWord(fourLW.bytes, 4) };
            roomString = string.Join(" ", words);
            return roomString;
        }

        private void SetRoomName(string newName)
        {
            roomString = newName;
            roomName.text = roomString;
        }

        private string GetRandomWord(byte[] source, int wordLength)
        {
            string foundWord = "";
            char[] subset = new char[10];

            Array.Copy(source, randomSeed.Next(1, source.Length - 10), subset, 0, 10);
            string[] midStr = new string(subset).Split(',');

            foreach (string s in midStr)
            {
                if (s.Length == wordLength)
                {
                    foundWord = s;
                    break;
                }
            }
            return foundWord;
        }

        public void SetRoomSize(int newCapacity)
        {
            maxPeers = newCapacity;
        }

        public void SetRoomMode(bool isPublic)
        {
            publicRoom = isPublic;
        }

        public void CreateRoom()
        {
            OpenRoom(gameObject.name, roomString, maxPeers, publicRoom);
        }

        private void RoomCreated(string message)
        {
            //currentRoom = JsonConvert.DeserializeObject<string>(message);
            Debug.Log("Room '" + message + "' created");
            currentRoom = message;

            CheckForRooms();

            // prevent further room creation or joining
            CreateButton.interactable = false;
            JoinButton.interactable = false;
        }

        public void JoinRoom()
        {
            if (!string.IsNullOrEmpty(roomSelector.State))
            {
                roomString = roomSelector.State;
                if (roomString != currentRoom)
                {
                    JoinRoom(gameObject.name, roomString);
                }
            }
            else
            {
                var tcl = roomTemplate.GetComponent<Text>();

                if (tcl.color == Color.yellow){
                    tcl.color = Color.white;
                }
                else {
                    tcl.color = Color.yellow;
                }
            }
        }

        private void RoomJoined(string message)
        {
            CheckForRooms();
            currentRoom = message;

            // prevent new room creation or joining (until disconnect)
            CreateButton.interactable = false;
            JoinButton.interactable = false;
        }


        private void ReceiveRoomInfo(string message)
        {
            RoomObject newRoom = JsonConvert.DeserializeObject<RoomObject>(message);
            bool roomExists = false;
            if (rooms == null)
            {
                rooms = new RoomObject[0];
            }

            for (int r = 0; r < rooms.Length; r++)
            {
                RoomObject room = rooms[r];

                if (room.Sessionid == newRoom.Sessionid)
                {
                    Debug.Log("Room already exists. Updating..");

                    roomExists = true;
                    room.Owner = newRoom.Owner;
                    room.Participants = newRoom.Participants;
                    room.MaxParticipantsAllowed = newRoom.MaxParticipantsAllowed;
                    room.Session = newRoom.Session;

                    string roomDetails = room.Sessionid + " (" + room.Participants.Length + "/" + room.MaxParticipantsAllowed.ToString() + ")";
                    roomParent.GetChild(r + 1).gameObject.GetComponent<TMPro.TextMeshPro>().text = roomDetails;
                }
            }

            if (!roomExists)
            {
                Array.Resize(ref rooms, rooms.Length + 1);
                rooms[rooms.Length - 1] = newRoom;
                newRoomFound = true;
            }

            JoinButton.interactable = (rooms.Length > 0 && newRoom.Sessionid != currentRoom);
        }

        private void MakeRoomListItem(RoomObject room)
        {
            string roomDetails = room.Sessionid + " (" + room.Participants.Length + "/" + room.MaxParticipantsAllowed.ToString() + ")";

            
            // create new room label from prefab
            GameObject roomObject = Instantiate(roomTemplate.gameObject);
            roomObject.name = room.Sessionid;
            roomObject.GetComponent<TMPro.TextMeshPro>().text = roomDetails;
            roomObject.transform.SetParent(roomParent, false);

            // make new control event
            ControlEvent newCE = new ControlEvent()
            {
                state = room.Sessionid,
                sensor = roomObject.GetComponent<Collider>(),
                controlEffect =  null
            };

            int noRooms = roomSelector.AddControlEvent(newCE);
            float objectheight = -11.0f * noRooms + 6.0f;
            Vector3 objectPos = new Vector3(0f, objectheight, 0f);

            roomObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            roomObject.GetComponent<RectTransform>().anchoredPosition = objectPos;
            roomObject.transform.rotation = roomTemplate.rotation;
        }
    }
}

