using UnityEngine;
using UnityEngine.UI;

namespace ICVR.SharedAssets
{
    public class ChatController : MonoBehaviour
    {
        public bool HasFocus { get; private set; }

        [SerializeField] private InputField _input;
        [SerializeField] private Text _output;

        private string currentDraft = "";
        private string messageBuffer = "";
        private bool newChatMessageReady = false;

        public delegate void ChatBroadcast(AvatarChatData chatData);
        public event ChatBroadcast OnBroadcastMessage;


        void Update()
        {
            if (newChatMessageReady)
            {
                newChatMessageReady = false;
                _output.text += messageBuffer;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (HasFocus)
                {
                    BroadcastChatMessage();
                }
                else
                {
                    Debug.Log("input not focused");
                }
            }
        }

        public void GetFocus()
        {
            HasFocus = true;
            _input.ActivateInputField();
        }

        public void LoseFocus()
        {
            HasFocus = false;
            _input.DeactivateInputField();
        }

        public void BroadcastChatMessage()
        {
            if (currentDraft.Length == 0) return;

            AvatarChatData chatMessage = new AvatarChatData
            {
                Scope = "broadcast",
                Message = currentDraft
            };

            OnBroadcastMessage.Invoke(chatMessage);

            currentDraft = "";
            _input.SetTextWithoutNotify("");
            _input.ActivateInputField();
        }

        public void UpdateChatFeed(string id, AvatarChatData acd)
        {
            if (acd.Scope == "broadcast")
            {
                messageBuffer = id + ":\n" + acd.Message + "\n";
                newChatMessageReady = true;
            }
        }

        public void UpdateChatMessage(string message)
        {
            if (message.Length < 1) return;

            string lastChar = message.Substring(message.Length - 1);
            if (lastChar.ToString().Equals("`"))
            {
                _input.text = message.Substring(0, message.Length - 1);
            }
            else
            {
                currentDraft = message;
            }
        }


    }
}