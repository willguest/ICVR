# MediaController Documentation

The `MediaController` class is a script that controls the playback of audio tracks in a Unity game. It allows the user to play, pause, stop, and navigate between audio tracks. It also provides events for when the audio track changes or is updated.

## Usage

To use the `MediaController` script, attach it to a GameObject in your scene. Make sure that the GameObject also has an AudioSource component attached to it. The AudioSource is used to play the audio tracks.

## Serialisable Properties

- `continuousPlay`: Sets whether the next track should automatically play when the one before finishes.

## Public Properties

- `CurrentTrackInfo`: Gets the current track information as a string.
- `CurrentTrackNo`: Gets the current track number.

## Events

- `OnAudioUpdated`: Event that is triggered when the audio track is updated. It provides the current track information and track number as parameters.
- `OnTrackChange`: Event that is triggered when the track changes.

## Public Methods

- `PlayAudio()`: Plays the audio track. If no audio track is currently loaded, it loads the first track.
- `PauseAudio()`: Pauses or resumes the playback of the audio track.
- `StopAudio()`: Stops the playback of the audio track.
- `PreviousTrack()`: Loads and plays the previous audio track.
- `NextTrack()`: Loads and plays the next audio track.

## How it Works

1. The `Start()` method initializes the `AudioSource` component and loads the track list from a `StreamingAsset` component attached to the same GameObject.

2. The `Update()` method plays the audio track when the `ReadyToPlay` flag is set to true.

3. When the user calls the `PlayAudio()` method, it checks if an audio track is already loaded. If not, it loads the first track from the track list. If an audio track is already loaded, it continues playing the current track.

4. The `Play()` method unloads any currently loaded audio, loads the requested audio track, creates a new `AudioSource` component, and plays the audio track.

5. The `PauseAudio()` method pauses or resumes the playback of the audio track.

6. The `StopAudio()` method stops the playback of the audio track.

7. The `PreviousTrack()` method loads and plays the previous audio track from the track list.

8. The `NextTrack()` method loads and plays the next audio track from the track list.

9. The `UnloadAudioResources()` method stops the playback of any currently playing audio and unloads the audio clip.

10. The `LoadClip()` method loads an audio clip from a URL using a UnityWebRequest.

11. The `LoadAudioFromUri()` and `LoadAudioInEditor()` methods handle the loading of audio clips for WebGL and Editor platforms respectively.

12. The `WaitForTrackEnd()` method waits for the current track to finish playing before loading and playing the next track.
