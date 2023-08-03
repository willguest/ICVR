# JukeboxController Class

The `JukeboxController` class is responsible for controlling a jukebox system. It handles loading and playing audio tracks, as well as managing track information and UI display.

## Public Methods

### `void PlayAudio()`
Plays the audio track, either by loading a new track or resuming playback of the current track.

### `void PauseAudio()`
Pauses or resumes the playback of the audio track.

### `void StopAudio()`
Stops the playback of the audio track.

### `void PreviousTrack()`
Loads and plays the previous track in the track list.

### `void NextTrack()`
Loads and plays the next track in the track list.

## Private Methods

### `void Start()`
Called when the script is initialized. Retrieves the track list from the StreamingAsset component and initializes audio sources.

### `void Update()`
Called once per frame. Checks if a new track is ready to be played and initiates playback.

### `void OnApplicationQuit()`
Called when the application is about to quit. Unloads audio resources.

### `IEnumerator LoadAudioFromUri(string url, Action<AudioClip> onLoadingCompleted)`
Coroutine that downloads and loads an audio clip from the given URL using the obsolete WWW class.

### `IEnumerator LoadAudioFromUriUWR(string url, Action<AudioClip> onLoadingCompleted)`
Coroutine that downloads and loads an audio clip from the given URL using UnityWebRequest and DownloadHandlerAudioClip.

### `IEnumerator LoadAudioInEditor(string url, Action<AudioClip> onLoadingCompleted)`
Coroutine that downloads and loads an audio clip in the Unity editor using the obsolete WWW class. This method is only used in Unity versions earlier than 2018.4.29f1.

### `IEnumerator LoadAudioInEditorUWR(string url, Action<AudioClip> onLoadingCompleted)`
Coroutine that downloads and loads an audio clip in the Unity editor using UnityWebRequest and DownloadHandlerAudioClip.

### `void SaveAudioToIDB(string assetPath, string trackId)`
Saves the audio track to IndexedDB. This method is called from WebGL platform only.

### `void LoadAudioTrack(string audioId)`
Called when an audio track has been fully loaded in IndexedDB. Initiates loading and playing of the audio track.

### `void GetUrlFromWebGL(string url)`
Receives the callback from `GetAudioUrlFromIndexedDB` in the WebGL platform. Updates the current audio URL and triggers audio loading and playing.

### `void UpdateTrackText(string trackId)`
Updates the track information displayed on the UI.

### `AudioSource CreateAudioSource(AudioClip clip, string trackId)`
Creates a new AudioSource component and configures it with the given audio clip and track ID.

### `void UnloadAudioResources()`
Unloads existing audio resources by stopping and destroying audio sources and clips.

### `void LoadClip(string url, Action<AudioClip> onLoadingCompleted)`
Loads an audio clip from the given URL and triggers the given callback when loading is completed.

## Serialized Fields

### `Text TrackDisplayText`
Reference to the UI Text component that displays the track information.

### `Text TrackDisplayNo`
Reference to the UI Text component that displays the track number.

### `AudioClip MachineStartSound`
The sound clip played when the jukebox starts playing a new track.

### `AudioClip MachineChangeRecord`
The sound clip played when the jukebox changes to a different track.

## Private Fields

### `AudioSource myAudioSource`
Reference to the main audio source.

### `AudioSource machineSounds`
Reference to the audio source used for machine sounds.

### `string[] currentTrackList`
Array representing the list of track IDs.
		
### `string streamingAssetUrl`
The URL of the current streaming asset.

### `string currentAudioURL`
The URL of the current audio track.