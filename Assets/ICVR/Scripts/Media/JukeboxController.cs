/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ICVR
{
    /// <summary>
    /// <see href="https://github.com/willguest/ICVR/tree/develop/Documentation/Media/JukeboxController.md"/>
    /// </summary>
    public class JukeboxController : MonoBehaviour
    {

#if UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern string GetAudioUrlFromIndexedDB(string objectName, string str);

        [DllImport("__Internal")]
        public static extern string SaveAudioInIndexedDB(string objectName, string path, string audioId);
#endif

        [SerializeField] private Text TrackDisplayText;
        [SerializeField] private Text TrackDisplayNo;
        [SerializeField] private AudioClip MachineStartSound;
        [SerializeField] private AudioClip MachineChangeRecord;


        private AudioSource myAudioSource;
        private AudioSource machineSounds;

        private string[] currentTrackList;

        private string streamingAssetUrl = "";
        private string currentAudioURL = "";
        private string currentAudioId = "";

        private bool isLoadingTrack = false;
        private bool ReadyToPlay = false;
        private bool isURLFromWebGLReceived = false;


        #region Start, Update, Quit

        void Start()
        {
            myAudioSource = GetComponent<AudioSource>();
            machineSounds = GetComponentInChildren<AudioSource>();

            if (GetComponent<StreamingAsset>())
            {
                currentTrackList = GetComponent<StreamingAsset>().BuildDataStore();
            }
            else
            {
                //Debug.LogError(gameObject.name + " has no data store");
            }

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

        private void OnApplicationQuit()
        {
            UnloadAudioResources();
        }

        #endregion Start, Update, Quit


        #region Jukebox Button Functions

        private void UpdateTrackText(string trackId)
        {
            string[] trackInfo = trackId.Split('_');
            string trackInfoDisplay = trackInfo[1] + "\n-----------\n" + trackInfo[2];
            TrackDisplayText.text = trackInfoDisplay;
            TrackDisplayNo.text = (GetComponent<StreamingAsset>().currDataIndex + 1).ToString();
        }

        public void PlayAudio()
        {
            if (myAudioSource == null)
            {
                //Debug.Log("Cannot play track. Audio source is null");
            }
            else if (string.IsNullOrEmpty(currentAudioId) || string.IsNullOrEmpty(currentAudioURL))
            {
                //Debug.Log("No loaded tracks found. Creating new library...");

                if (currentTrackList.Length > 0)
                {
                    if (isLoadingTrack)
                    {
                        TrackDisplayText.text = "Jukebox is busy.\n\n Please wait a few seconds and try again.";
                        return;
                    }

                    isLoadingTrack = true;
                    machineSounds.PlayOneShot(MachineStartSound);

                    streamingAssetUrl = GetComponent<StreamingAsset>().GetRandomFileUrl();
                    currentAudioId = System.IO.Path.GetFileNameWithoutExtension(streamingAssetUrl);
                    if (currentAudioId != "")
                    {
                        SaveAudioToIDB(streamingAssetUrl, currentAudioId);
                    }
                }
            }
            else
            {
                SaveAudioToIDB(streamingAssetUrl, currentAudioId);
            }
        }

        public void PauseAudio()
        {
            if (myAudioSource != null)
            {
                if (myAudioSource.clip != null && myAudioSource.clip.loadState == AudioDataLoadState.Loaded)
                {
                    if (myAudioSource.isPlaying)
                    {
                        myAudioSource.Pause();
                    }
                    else
                    {
                        myAudioSource.Play();
                    }
                }
                else
                {
                    //Debug.Log("No clip to pause.");
                }
            }
            else
            {
                //Debug.Log("Cannot pause, audio source or clip is null");
            }
        }

        public void StopAudio()
        {
            if (myAudioSource != null)
            {
                if (myAudioSource.isPlaying)
                {
                    myAudioSource.Stop();
                }
                else
                {
                    //Debug.Log("Audio source is not playing, stop not performed.");
                }
            }
            else
            {
                //Debug.Log("Cannot stop playing, audio source is null");
            }
        }



        public void PreviousTrack()
        {
            if (isLoadingTrack)
            {
                TrackDisplayText.text = "Jukebox is busy.\n\nPlease wait a few seconds and try again.";
                return;
            }

            isLoadingTrack = true;
            TrackDisplayText.text = "Loading...";
            TrackDisplayNo.text = "";

            //machineSounds.PlayOneShot(MachineChangeRecord);

            UnloadAudioResources();

            streamingAssetUrl = GetComponent<StreamingAsset>().GetPrevFileUrl();
            currentAudioId = System.IO.Path.GetFileNameWithoutExtension(streamingAssetUrl);

            if (currentAudioId != "")
            {
                SaveAudioToIDB(streamingAssetUrl, currentAudioId);
            }
        }

        public void NextTrack()
        {
            if (isLoadingTrack)
            {
                TrackDisplayText.text = "Jukebox is busy.\n\nPlease wait a few seconds and try again.";
                return;
            }

            isLoadingTrack = true;
            TrackDisplayText.text = "Loading...";
            TrackDisplayNo.text = "";

            //machineSounds.PlayOneShot(MachineChangeRecord);

            UnloadAudioResources();

            streamingAssetUrl = GetComponent<StreamingAsset>().GetNextFileUrl();
            currentAudioId = System.IO.Path.GetFileNameWithoutExtension(streamingAssetUrl);

            if (currentAudioId != "")
            {
                SaveAudioToIDB(streamingAssetUrl, currentAudioId);
            }
        }


        #endregion Jukebox Button Functions


        #region StreamingAssets and IndexedDB 

        private void SaveAudioToIDB(string assetPath, string trackId)
        {
#if UNITY_EDITOR
            currentAudioURL = "file://" + assetPath;

            // remove all current AudioSource component, destroy clip and component to force memory unload (hopefully)
            UnloadAudioResources();

            StartCoroutine(LoadAudioInEditor(currentAudioURL, (clip) =>
            {
            // update jukebox track display screens
            UpdateTrackText(trackId);

            // create new AudioSource (so old one can be destroyed)
            AudioSource freshAudioSource = CreateAudioSource(clip, trackId);

            // play new track
            freshAudioSource.Play();

            // notify that loading has finished
            isLoadingTrack = false;
            }));
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor)
        {
            SaveAudioInIndexedDB(this.gameObject.name, assetPath, trackId);
        }
#endif
        }

        /// <summary>
        /// This is the function called from simpleWebXR.jslib, when the audio has been fully loaded 
        /// in to indexeddb.
        /// </summary>
        /// <param name="audioId"></param> The same as the id parameter sent to SaveAudioInIndexedDB.
        public void LoadAudioTrack(string audioId)
        {
            //Debug.Log("Received track ready trigger:" + audioId);
            StartCoroutine(LoadAudioForWebGL(audioId));
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
            myAudioSource = newAS;
            return newAS;
        }

        private IEnumerator LoadAudioForWebGL(string audioId)
        {
            isURLFromWebGLReceived = false;
            GetAudioUrlFromIndexedDB(this.gameObject.name, audioId);

            for (int i = 0; !isURLFromWebGLReceived; i++)
            {
                if (i > 100)
                {
                    //Debug.LogError("Url not received...");
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            if (isURLFromWebGLReceived)
            {
                //Debug.LogError("Audio track ready.");
                isURLFromWebGLReceived = false;
                ReadyToPlay = true;
            }

        }

        /// <summary>
        /// Receives callback from GetAudioUrlFromIndexedDB in simpleWebXR.jslib.
        /// </summary>
        /// <param name="url"></param> path to the indexeddb location of the audio file
        public void GetUrlFromWebGL(string url)
        {
            //Debug.Log("URL received: " + url);
            currentAudioURL = url;
            isURLFromWebGLReceived = true;
            //Play(currentAudioURL, currentAudioId);
        }


        private void Play(string fileName, string trackName)
        {
            // unload any existing audio
            UnloadAudioResources();

            // Load Clip then assign to audio source and play
            LoadClip(fileName, (clip) =>
            {
            // update jukebox track display screens
            UpdateTrackText(trackName);

            // create new AudioSource (so old one can be destroyed)
            AudioSource freshAudioSource = CreateAudioSource(clip, trackName);

            // play new track
            freshAudioSource.Play();

            // notify that loading has finished
            isLoadingTrack = false;
            });
        }

        private void UnloadAudioResources()
        {
            foreach (AudioSource audio in gameObject.GetComponents<AudioSource>())
            {
                audio.Stop();
                if (audio.clip != null)
                {
                    //Debug.Log("Destroying clip:" + audio.clip.name);

                    audio.clip.UnloadAudioData();
                    audio.clip = null;
                    DestroyImmediate(audio.clip, false);
                }

                Destroy(audio);
            }
        }

        private void LoadClip(string url, Action<AudioClip> onLoadingCompleted)
        {
            StartCoroutine(LoadAudioFromUri(url, onLoadingCompleted));
        }

        /*
        IEnumerator LoadAudioFromUri(string _url, Action<AudioClip> onLoadingCompleted)
        {
            // Download of the given URL
            WWW www = new WWW(_url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                //Debug.Log(www.error);
            }
            else
            {
                AudioClip ac = www.GetAudioClipCompressed(false, AudioType.AUDIOQUEUE) as AudioClip;
                if (ac != null)
                {
                    onLoadingCompleted(ac);
                }
                else
                {
                    //Debug.Log("audio clip was null");
                }
            }
        }

        // only > 2018.4.29f1
        // earlier versions will throw a 'Streaming of mpeg on this platform is not supported' error.
        IEnumerator LoadAudioInEditor(string _url, Action<AudioClip> onLoadingCompleted)
        {
            // Download of the given URL
            WWW www = new WWW(_url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                //Debug.Log(www.error);
            }
            else
            {
                AudioClip ac = www.GetAudioClip(false, false, AudioType.MPEG);
                if (ac != null)
                {
                    onLoadingCompleted(ac);
                }
                else
                {
                    //Debug.Log("audio clip was null");
                }
            }
        }

        */

        IEnumerator LoadAudioFromUri(string _url, Action<AudioClip> onLoadingCompleted)
        {
            // Download of the given URL
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_url, AudioType.AUDIOQUEUE))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    // Handle error here
                    //Debug.Log(www.error);
                }
                else
                {
                    AudioClip ac = DownloadHandlerAudioClip.GetContent(www);
                    onLoadingCompleted(ac);
                }
            }
        }


        IEnumerator LoadAudioInEditor(string _url, Action<AudioClip> onLoadingCompleted)
        {
            // Download of the given URL
            using (UnityWebRequest www = UnityWebRequest.Get(_url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    // Handle error here
                    //Debug.Log(www.error);
                }
                else
                {
                    AudioClip ac = DownloadHandlerAudioClip.GetContent(www);
                    onLoadingCompleted(ac);
                }
            }
        }


        #endregion StreamingAssets and IndexedDB 

    }
}
