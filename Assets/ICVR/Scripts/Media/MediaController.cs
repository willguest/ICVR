using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;


public class MediaController : MonoBehaviour
{

#if UNITY_WEBGL
    [DllImport("__Internal")]
    public static extern string GetAudioUrlFromIndexedDB(string objectName, string str);

    [DllImport("__Internal")]
    public static extern string SaveAudioInIndexedDB(string objectName, string path, string audioId);
#endif
    public delegate void AudioChanged(string trackInfo, int trackNo);
    public event AudioChanged OnAudioUpdated;

    public delegate void TrackChanged();
    public event TrackChanged OnTrackChange;

    public string CurrentTrackInfo { get; private set; }
    public int CurrentTrackNo { get; private set; }

    private AudioSource myAudioSource;
    private string[] currentTrackList;

    private string streamingAssetUrl = "";
    private string currentAudioURL = "";
    private string currentAudioId = "";

    private bool isLoadingTrack = false;
    private bool ReadyToPlay = false;
    private bool isURLFromWebGLReceived = false;

    private bool continuousPlay = false;

    #region Start, Update, Quit

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
        CurrentTrackNo = 0;

        if (GetComponent<StreamingAsset>())
        {
            currentTrackList = GetComponent<StreamingAsset>().BuildDataStore();
            CurrentTrackInfo = (gameObject.name + "'s data store loaded");
        }
        else
        {
            CurrentTrackInfo = (gameObject.name + " has no data store");
        }
    }

    void Update()
    {
        if (ReadyToPlay)
        {
            Debug.Log("Playing track:\n" + currentAudioId);
            ReadyToPlay = false;
            Play(currentAudioURL, currentAudioId);
        }
    }

    private void OnApplicationQuit()
    {
        UnloadAudioResources();
    }

    #endregion Start, Update, Quit


    #region Media Button Functions

    private void UpdateTrackText(string trackId)
    {
        string[] trackInfo = trackId.Split('_');
        string trackInfoDisplay = trackInfo[1] + "\n-----------\n" + trackInfo[2];
        CurrentTrackInfo = trackInfoDisplay;
        CurrentTrackNo = GetComponent<StreamingAsset>().currDataIndex + 1;
        OnAudioUpdated.Invoke(CurrentTrackInfo, CurrentTrackNo);
    }

    public void UnblockOperation()
    {
        isLoadingTrack = false;
    }

    public void PlayAudio()
    {
        if (myAudioSource == null)
        {
            //Debug.Log("Cannot play track. Audio source is null");
        }
        else if (string.IsNullOrEmpty(currentAudioId) || string.IsNullOrEmpty(currentAudioURL))
        {
            if (currentTrackList.Length > 0)
            {
                if (isLoadingTrack)
                {
                    CurrentTrackInfo = "Jukebox is busy.\n\n Please wait a few seconds and try again.";
                    return;
                }

                isLoadingTrack = true; // hold operation until further instruction
                OnTrackChange.Invoke(); // animate and then resume operation

                myAudioSource.volume = 0.8f;
                streamingAssetUrl = GetComponent<StreamingAsset>().GetFirstFileUrl();
                currentAudioId = System.IO.Path.GetFileNameWithoutExtension(streamingAssetUrl);
                if (currentAudioId != "")
                {
                    continuousPlay = true;
                    SaveAudioToIDB(streamingAssetUrl, currentAudioId);
                }
            }
        }
        else
        {
            continuousPlay = true;
            SaveAudioToIDB(streamingAssetUrl, currentAudioId);
        }
    }

    public void PauseAudio()
    {
        if (myAudioSource != null)
        {
            if (myAudioSource.clip != null && myAudioSource.clip.loadState == AudioDataLoadState.Loaded)
            {
                continuousPlay = false;

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
            continuousPlay = false;

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
            CurrentTrackInfo = "Jukebox is busy.\n\nPlease wait a few seconds and try again.";
            return;
        }

        OnTrackChange.Invoke();
        isLoadingTrack = true;
        CurrentTrackInfo = "Loading...";
        CurrentTrackNo = 0;

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
            CurrentTrackInfo = "Jukebox is busy.\n\nPlease wait a few seconds and try again.";
            Debug.Log("Jukebox is busy. Please wait a few seconds and try again.");
            return;
        }

        isLoadingTrack = true;

        OnTrackChange.Invoke();
        
        CurrentTrackInfo = "Loading...";
        CurrentTrackNo = 0;
    
        UnloadAudioResources();

        streamingAssetUrl = GetComponent<StreamingAsset>().GetNextFileUrl();
        currentAudioId = System.IO.Path.GetFileNameWithoutExtension(streamingAssetUrl);

        if (currentAudioId != "")
        {
            SaveAudioToIDB(streamingAssetUrl, currentAudioId);
        }
    }


    #endregion Media Button Functions


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
            //isLoadingTrack = false;

            // set trigger for next track
            StartCoroutine(WaitForTrackEnd());
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
    /// This is called when the audio has been fully loaded into idb
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
        newAS.volume = 0.4f;
        newAS.spatialBlend = 1.0f;
        newAS.rolloffMode = AudioRolloffMode.Logarithmic;
        newAS.minDistance = 2.0f;
        newAS.maxDistance = 30.0f;

        // set and configure clip
        newAS.clip = clip;
        newAS.clip.name = trackId;

        // assign global variable
        myAudioSource = newAS;
        return newAS;
    }

    private IEnumerator WaitForTrackEnd()
    {
        if (myAudioSource != null)
        {
            yield return new WaitUntil(() => !myAudioSource.isPlaying && myAudioSource.time == 0 && continuousPlay);
        }
        else
        {
            yield return null;
        }

        NextTrack();
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
    /// Receives callback from GetAudioUrlFromIndexedDB
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
            //isLoadingTrack = false;

            // set trigger for next track
            StartCoroutine(WaitForTrackEnd());
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
