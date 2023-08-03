# Avatar Controller Documentation

This class is responsible for controlling the avatar in the scene. It contains various functions related to avatar control, audio streaming, and avatar color setting.

---

## Variables

- `StartAudioStream` - function that starts the audio stream using a specified user ID.
- `StopAudioStream` - function that stops the audio stream using a specified user ID.
- `DefaultColour` - property that gets the default color of the avatar.
- `AffectedRenderers` - a list of renderers that are affected by the color change.
- `head`, `body` - game objects representing the head and body parts of the avatar.
- `leftHand`, `rightHand` - instances of the `AvatarHand` class representing the left and right hands of the avatar.
- `leftPointer`, `rightPointer` - game objects representing the left and right hand pointers of the avatar.
- `latencyText` - a `TextMesh` component used to display the latency value.
- `connectionIndicator` - a `Renderer` component used to visualize the connection status.

---

## Public Methods

- `Initialise()` - initializes the avatar, sets the default color and lerp time, and gets the audio source component.
- `EndSession()` - ends the avatar session, stops any playing audio, and unloads audio resources.
- `OpenAudioChannel(string userId)` - opens the audio channel for a specified user ID.
- `CloseAudioChannel(string userId)` - closes the audio channel for a specified user ID.
- `AddAudioStream(string message)` - adds an audio stream based on the provided message.
- `UpdateAvatar(long latency, NodeDataFrame ndf)` - updates the avatar's position, handles interaction events, and updates the latency text.
- `ConnectAudioSource(string userid, string audioUrl)` - connects the audio source for a specified user ID and audio URL.
- `SetDefaultColor(Color col)` - sets the default color of the avatar.

---

## Avatar Interaction Methods

- `UpdateHandInteractions(string avatarHandlingEvent)` - updates the hand interactions based on the provided event data.
- `ReceiveInstruction(AvatarHandlingData instruction)` - receives and processes avatar handling instructions.
- `PickUpObject(GameObject target, AcquireData acquisition)` - picks up an object based on the provided target and acquisition data.
- `DropObject(GameObject target, ReleaseData release)` - drops an object based on the provided target and release data.
- `SetLayerRecursively(GameObject obj, int newLayer)` - sets the layer of a game object and its children recursively.

---

## Avatar Colour Setting Methods

- `GetRandomColour()` - generates a random color.
- `ChangeColour()` - changes the color of the avatar and affected renderers based on the random color.

---

## Audio Methods

- `Play(string fileName, string trackName)` - plays an audio clip based on the provided file name and track name.
- `CreateAudioSource(AudioClip clip, string trackId)` - creates and configures an audio source component based on the provided clip and track ID.
- `UnloadAudioResources()` - unloads and destroys any existing audio sources and clips.
- `LoadClip(string url, Action<AudioClip> onLoadingCompleted)` - loads an audio clip from a URL.
- `LoadStreamFromUri(string _url, Action<AudioClip> onLoadingCompleted)` - loads an audio stream from a URI.

---

## Avatar Lerping

- `LerpToState(GameObject avatar, Transform initialState, Vector3 targetPos, Quaternion targetRot, float duration)` - lerps the avatar's position and rotation to a target state over a specified duration.
- `LerpToDataFrame(NodeDataFrame dataFrame, float duration)` - lerps the avatar's position and rotation to a specified data frame over a duration.


---

## Code Overview
This code defines the `AvatarController` class, which controls the behavior of an avatar in a virtual environment. It includes functions to handle audio streaming, avatar interaction, and avatar color setting. The class also includes functions to update the avatar's position and rotation based on data received.

## How it Works
The `AvatarController` class is a MonoBehaviour that is attached to a game object in the scene. It contains a number of public and private variables and functions that control the behavior of the avatar.

### Start and Update
The `Start` function is called when the object is initialized and retrieves a reference to the `FixedJoint` component attached to the avatar's head. The `Update` function is called every frame and checks if the avatar is ready to play audio. If ready, it calls the `Play` function to start playing the audio.

### Interface
The public functions in the `Interface` region are used to interface with the avatar controller. The `Initialise` function initializes the default color and assigns the AudioSource component from the avatar's head. The `EndSession` function stops the audio and unloads any audio resources. The `OpenAudioChannel` and `CloseAudioChannel` functions start and stop audio streaming for a specific user. The `AddAudioStream` function adds an audio stream to the avatar. The `UpdateAvatar` function updates the avatar's position and rotation based on received data. The `ConnectAudioSource` function connects an audio source to the avatar. The `SetDefaultColor` function sets the default color of the avatar.

### Avatar Interaction
The private functions in the `Avatar Interaction` region handle avatar interaction events. The `UpdateHandInteractions` function processes the interaction events received from the hands. The `ReceiveInstruction` function receives instructions for interacting with objects in the scene. The `PickUpObject` function picks up an object and sets it as a child of the avatar's head. The `DropObject` function drops an object and applies forces to it. The `SetLayerRecursively` function sets the layer of an object and its children recursively.

### Avatar Color Setting
The private functions in the `Avatar Color Setting` region handle setting the color of the avatar. The `GetRandomColour` function generates a random color. The `ChangeColour` function sets the color of the avatar's affected renderers to a random color.

### Audio Functions
The private functions in the `Audio Functions` region handle audio streaming and playback. The `Play` function unloads any existing audio resources, loads an audio clip from a URL, and plays it. The `CreateAudioSource` function creates and configures an AudioSource component for playing the loaded audio clip. The `UnloadAudioResources` function stops any playing audio and unloads audio resources. The `LoadClip` function loads an audio clip from a URL. The `LoadStreamFromUri` coroutine loads an audio stream from a URL asynchronously.

### Avatar Lerping
The private functions in the `Avatar Lerping` region handle lerping the avatar's position and rotation. The `LerpToState` coroutine lerps the position and rotation of an object to a target position and rotation over a duration. The `LerpToDataFrame` coroutine lerps the position and rotation of the avatar's head, hands, and pointers to the target position and rotation in a received data frame.
