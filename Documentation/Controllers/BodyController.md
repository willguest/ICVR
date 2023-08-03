# BodyController Class

The BodyController class is responsible for managing the body movements and interactions in a virtual reality environment. It contains properties and methods to handle the current user's ID, the number of peers connected to the network, and the body mass.

## Properties

- `CurrentUserId` (string): The current user's ID.
- `CurrentNoPeers` (int): The number of peers connected to the network.
- `BodyMass` (float): The body mass.

## External Dependencies

- `DllImport("__Internal")`: An external function declared in the Unity engine.

## Serialized Fields

- `headObject` (GameObject): The game object representing the head.
- `leftHand` (GameObject): The game object representing the left hand.
- `rightHand` (GameObject): The game object representing the right hand.
- `leftPointer` (Transform): The transform of the left pointer.
- `rightPointer` (Transform): The transform of the right pointer.
- `hudFollower` (GameObject): The game object representing the HUD follower.

## Private Fields

- `IsConnectionReady` (bool): Indicates if the connection is ready.
- `hasInteractionEvent` (bool): Indicates if there is an interaction event.
- `UiStartPos` (Pose): The start position of the UI.
- `currentEventType` (AvatarEventType): The current avatar event type.
- `currentEventData` (string): The current avatar event data.
- `startTime` (float): The start time.
- `notifyingNetwork` (bool): Indicates if the network is being notified.

## Events

- `OnNetworkInteraction` (Action<AvatarHandlingData>): Event triggered when there is a network interaction.
- `OnHandInteraction` (Action<AvatarHandlingData>): Event triggered when there is a hand interaction.
- `OnXRChange` (Action<WebXRState, int, Rect, Rect>): Event triggered when there is a change in XR state.

## Methods

- `OnEnable()`: Called when the object becomes enabled and active.
- `OnDisable()`: Called when the object becomes disabled.
- `OnXRChange(WebXRState, int, Rect, Rect)`: Handles the XR state change event.
- `Start()`: Called before the first frame update.
- `Update()`: Called once per frame.
- `InitialiseDataChannel(string)`: Initializes the data channel for communication.
- `StartAfterDelay(float)`: Starts communication after a delay.
- `playersChanged(int)`: Handles the event when the number of players changes.
- `SetConnectionReady(bool)`: Sets the connection readiness.
- `PackageEventData(AvatarHandlingData)`: Packages the avatar handling data into an event.
- `BuildDataFrame()`: Builds the data frame to be sent.

## How it Works

The class uses serialized fields to reference game objects and transforms used in the body tracking.

It also includes event handlers for network interactions and hand interactions. These events are triggered when there is a change in the XR state.

The class has methods to initialize the data channel for communication, start communication after a delay, handle the event when the number of players changes, and set the connection readiness.

The PackageEventData method packages the avatar handling data into an event, and the BuildDataFrame method builds the data frame to be sent.

In the Update method, the position and rotation of the HUD follower are set based on the head object's position and rotation. If the connection is ready, data frames are sent at regular intervals.

Overall, the BodyController class provides functionality for tracking body movements and interactions in a virtual reality environment and facilitating communication with peers in the network.