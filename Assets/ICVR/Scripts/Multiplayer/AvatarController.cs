using Newtonsoft.Json;
using ICVR.SharedAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace ICVR
{
    public class AvatarController : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void StartAudioStream(string userId);

        [DllImport("__Internal")]
        private static extern void StopAudioStream(string userId);

        public Color DefaultColour { get; private set; }

        [SerializeField] private List<Renderer> AffectedRenderers;
        
        [SerializeField] private GameObject head;
        [SerializeField] private GameObject body;

        [SerializeField] private AvatarHand leftHand;
        [SerializeField] private AvatarHand rightHand;

        [SerializeField] private GameObject leftPointer;
        [SerializeField] private GameObject rightPointer;

        [SerializeField] private TextMesh latencyText;
        [SerializeField] private Renderer connectionIndicator;


        private AudioSource Voice;

        private FixedJoint fixedJoint;
        private Rigidbody currentRigidbody;
        private string prevLayer = "";

        private string currentAudioURL = "";
        private string currentAudioId = "";

        private bool ReadyToPlay = false;
        private float updateFrequency = 4.0f;
        private float avatarLerpTime;

        private float triggerTick = 0;


        #region --- Start and Update ---

        void Start()
        {
            updateFrequency = NetworkIO.Instance.NetworkUpdateFrequency;
            fixedJoint = head.GetComponent<FixedJoint>();
        }

        void Update()
        {
            if (ReadyToPlay)
            {
                //Debug.Log("Playing track: " + currentAudioId + " (" + currentAudioURL + ")");
                ReadyToPlay = false;
                Play(currentAudioURL, currentAudioId);
            }

            
        }

        #endregion --- Start and Update ---


        #region --- Interface ---

        public void Initialise()
        {
            DefaultColour = ChangeColour();
            avatarLerpTime = 0.8f;
            Voice = head.GetComponent<AudioSource>();
        }

        public void EndSession()
        {
            ReadyToPlay = false;
            UnloadAudioResources();
        }

        public void OpenAudioChannel(string userId)
        {
            StartAudioStream(userId);
        }
        public void CloseAudioChannel(string userId)
        {
            StopAudioStream(userId);
        }

        public void AddAudioStream(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                Debug.Log("Add audio event was empty");
                return;
            }

            AudioStream audioData = JsonConvert.DeserializeObject<AudioStream>(message);

            if (string.IsNullOrEmpty(audioData.Userid))
            {
                Debug.Log("no user identified");
                return;
            }

            ConnectAudioSource(audioData.Userid, audioData.MediaElement.Url);
        }
        private bool audioIsActive()
        {
            return AvatarManager.Instance.AudioChannelOpen;
        }

        public void UpdateAvatar(long latency, NodeDataFrame ndf)
        {
            //int eventDataLength = ndf.EventData.Length;
            //avatarLerpTime = avatarLerpTime / (1 + eventDataLength);

            latencyText.text = latency.ToString();

            // set avatar body position
            StartCoroutine(LerpToDataFrame(ndf, avatarLerpTime));

            if (!string.IsNullOrEmpty(ndf.EventData))
            {
                // only process interaction events here
                if (ndf.EventType == AvatarEventType.Interaction)
                {
                    UpdateHandInteractions(ndf.EventData);
                }
            }
        }
   

        public void ConnectAudioSource(string userid, string audioUrl)
        {
            currentAudioURL = audioUrl;
            currentAudioId = userid;
            ReadyToPlay = true;
        }

        public void SetDefaultColor(Color col)
        {
            ChangeColour();
        }


        #endregion --- Interface ---

        #region --- Avatar Interaction ---

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "torso" && other.GetType() == typeof(CapsuleCollider) && (Time.time - triggerTick) > 0.5f)
            {
                if (!audioIsActive())
                {
                    Debug.Log("starting audio stream");
                    triggerTick = Time.time;
                    //AvatarManager.Instance.AudioChannelOpen = true;
                    //StartAudioStream(gameObject.name);
                    
                }
                else
                {
                    Debug.Log("audio open elsewhere");
                }
            }
            
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "torso" && other.GetType() == typeof(CapsuleCollider) && (Time.time - triggerTick) > 0.5f )
            {
                if (audioIsActive())
                {
                    Debug.Log("stopping audio stream");
                    triggerTick = Time.time;
                    //AvatarManager.Instance.AudioChannelOpen = false;
                    //StopAudioStream(gameObject.name);
                }

            }
        }

        private void UpdateHandInteractions(string avatarHandlingEvent)
        {
            if (string.IsNullOrEmpty(avatarHandlingEvent))
            {
                Debug.Log("null or empty event string");
                return;
            }

            try
            {
                AvatarHandlingData ahdFrame = JsonConvert.DeserializeObject<AvatarHandlingData>(avatarHandlingEvent);

                if (ahdFrame.Hand == ControllerHand.LEFT)
                {
                    leftHand.ReceiveInstruction(ahdFrame);
                }
                else if (ahdFrame.Hand == ControllerHand.RIGHT)
                {
                    rightHand.ReceiveInstruction(ahdFrame);
                }
                else // ControllerHand.NONE == desktopcontroller input
                {
                    ReceiveInstruction(ahdFrame);
                }
            }
            catch (Exception e)
            {
                Debug.Log("error talking to hands:" + e.Message);
            }
        }

        private void ReceiveInstruction(AvatarHandlingData instruction)
        {
            GameObject target;

            if (SharedAssetManager.Instance.SharedAssetRegister.TryGetValue(instruction.TargetId, out target))
            {
                if (instruction.EventType == AvatarInteractionEventType.AcquireData)
                {
                    PickUpObject(target, instruction.AcquisitionEvent);
                }
                else if (instruction.EventType == AvatarInteractionEventType.ReleaseData)
                {
                    DropObject(target, instruction.ReleaseEvent);
                }
            }
        }

        private void PickUpObject(GameObject target, AcquireData acquisition)
        {
            currentRigidbody = target.GetComponent<Rigidbody>();

            if (!currentRigidbody) return;

            // move object to start pose 
            target.transform.position = acquisition.ObjectPosition;
            target.transform.rotation = acquisition.ObjectRotation;

            fixedJoint.connectedBody = currentRigidbody;
            target.GetComponent<SharedAsset>().IsBeingHandled = true;

            // rememeber layer and set as tool
            prevLayer = LayerMask.LayerToName(target.layer);
            SetLayerRecursively(target, LayerMask.NameToLayer("Tools"));
        }


        private void DropObject(GameObject target, ReleaseData release)
        {
            if (!currentRigidbody) return;

            target.transform.position = release.ReleasePosition;
            target.transform.rotation = release.ReleaseRotation;

            currentRigidbody.AddForce(release.ForceData.LinearForce, ForceMode.Impulse);
            currentRigidbody.AddTorque(release.ForceData.AngularForce, ForceMode.Impulse);

            // reset and forget
            SetLayerRecursively(currentRigidbody.gameObject, LayerMask.NameToLayer(prevLayer));
            prevLayer = "";

            target.GetComponent<SharedAsset>().IsBeingHandled = false;

            fixedJoint.connectedBody = null;
            currentRigidbody = null;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
            {
                return;
            }

            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        #endregion --- Avatar Interaction ---

        #region --- Avatar Colour Setting ---

        private Color GetRandomColour()
        {
            float upperlimit = 30f;
            return new Color(
                (float)UnityEngine.Random.Range(0, upperlimit),
                (float)UnityEngine.Random.Range(0, upperlimit),
                (float)UnityEngine.Random.Range(0, upperlimit),
                (float)UnityEngine.Random.Range(0, upperlimit)
                );
        }

        private Color ChangeColour()
        {
            Color c = GetRandomColour();

            foreach (MeshRenderer r in AffectedRenderers)
            {
                r.material.SetColor("_EmissionColor", c * 0.02f);
            }
            return c;
        }


        #endregion --- Avatar Colour Setting ---


        #region --- Audio Functions ---


        private void Play(string fileName, string trackName)
        {
            // unload any existing audio
            UnloadAudioResources();

            // Load Clip then assign to audio source and play
            LoadClip(fileName, (clip) =>
            {
            // create new AudioSource (so old one can be destroyed)
            AudioSource freshAudioSource = CreateAudioSource(clip, trackName);

            // play new track
            freshAudioSource.Play();
            });
        }

        private AudioSource CreateAudioSource(AudioClip clip, string trackId)
        {
            // create and configure audio source
            AudioSource newAS = gameObject.AddComponent<AudioSource>();
            newAS.volume = 1.0f;
            newAS.spatialBlend = 1.0f;
            newAS.rolloffMode = AudioRolloffMode.Logarithmic;
            newAS.minDistance = 1.0f;
            newAS.maxDistance = 20.0f;

            // set and configure clip
            newAS.clip = clip;
            newAS.clip.name = trackId;

            // assign global variable
            Voice = newAS;
            return newAS;
        }

        private void UnloadAudioResources()
        {
            foreach (AudioSource audio in gameObject.GetComponents<AudioSource>())
            {
                audio.Stop();
                if (audio.clip != null)
                {
                    audio.clip.UnloadAudioData();
                    audio.clip = null;
                    DestroyImmediate(audio.clip, false);
                }
                Destroy(audio);
            }
        }

        private void LoadClip(string url, Action<AudioClip> onLoadingCompleted)
        {
            StartCoroutine(LoadStreamFromUri(url, onLoadingCompleted));
        }


        IEnumerator LoadStreamFromUri(string _url, Action<AudioClip> onLoadingCompleted)
        {
            using (var webRequest = UnityWebRequestMultimedia.GetAudioClip(_url, AudioType.AUDIOQUEUE))
            {
                ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = true;

                webRequest.SendWebRequest();

                // while InProgress(0) or Successful(1)
                while ((int)webRequest.result < 2 && webRequest.downloadedBytes < 1024)
                    yield return null;

                // check for ConnectionError(2), ProtocolError(3), DataProcessingError(4)
                if ((int)webRequest.result >= 2)
                {
                    Debug.Log("webrequest error: " + webRequest.error);
                    yield break;
                }

                AudioClip ac = ((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip;
                if (ac != null)
                {
                    onLoadingCompleted(ac);
                }
                else
                {
                    Debug.Log("Clip from audio stream was null");
                }
            }
        }

        #endregion --- Audio Functions ---


        #region --- Avatar Lerping ---

        private IEnumerator LerpToState(GameObject avatar, Transform initialState, Vector3 targetPos, Quaternion targetRot, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                avatar.transform.position = Vector3.Lerp(avatar.transform.position, targetPos, time / duration);
                avatar.transform.rotation = Quaternion.Lerp(avatar.transform.rotation, targetRot, time / duration);

                time += Time.deltaTime;
                yield return null;
            }
            avatar.transform.position = targetPos;
            avatar.transform.rotation = targetRot;
        }

        private IEnumerator LerpToDataFrame(NodeDataFrame dataFrame, float duration)
        {
            float time = 0;
            Transform initLeftHand = leftHand.transform;
            Transform initRightHand = rightHand.transform;
            Transform initLeftPoint = leftPointer.transform;
            Transform initRightPoint = rightPointer.transform;

            while (time < duration)
            {
                float step = time / duration;
                transform.position = Vector3.Lerp(transform.position, dataFrame.HeadPosition, step);
                head.transform.rotation = Quaternion.Lerp(head.transform.rotation, dataFrame.HeadRotation, step);
                body.transform.rotation = Quaternion.Euler(0f, head.transform.rotation.eulerAngles.y, 0f);

                leftHand.transform.position = Vector3.Lerp(initLeftHand.position, dataFrame.LeftHandPosition, step);
                leftHand.transform.rotation = Quaternion.Lerp(initLeftHand.rotation, dataFrame.LeftHandRotation, step);

                rightHand.transform.position = Vector3.Lerp(initRightHand.position, dataFrame.RightHandPosition, step);
                rightHand.transform.rotation = Quaternion.Lerp(initRightHand.rotation, dataFrame.RightHandRotation, step);

                leftPointer.transform.position = Vector3.Lerp(initLeftPoint.position, dataFrame.LeftHandPointer, step);
                rightPointer.transform.position = Vector3.Lerp(initRightPoint.position, dataFrame.RightHandPointer, step);

                time += Time.deltaTime;
                yield return null;
            }

            transform.position = dataFrame.HeadPosition;
            head.transform.rotation = dataFrame.HeadRotation;

            leftHand.transform.position = dataFrame.LeftHandPosition;
            leftHand.transform.rotation = dataFrame.LeftHandRotation;

            rightHand.transform.position = dataFrame.RightHandPosition;
            rightHand.transform.rotation = dataFrame.RightHandRotation;

            leftPointer.transform.position = dataFrame.LeftHandPointer;
            rightPointer.transform.position = dataFrame.RightHandPointer;
        }

        #endregion --- Avatar Lerping ---
    }
}
