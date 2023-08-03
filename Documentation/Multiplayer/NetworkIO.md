# NetworkIO

The `NetworkIO` class is the main entry point for P2P network communication in a Unity project. It handles the creation of network connections, sending and receiving data, and managing the state of the network.

## Properties

- `NetworkUpdateFrequency` (float): The frequency at which network updates are sent.
- `ReadyFlag` (bool): A flag indicating whether the network is ready for sending and receiving data.
- `CurrentUserId` (string): The ID of the current user.
- `IsConnected` (bool): Indicates whether the current user is connected to the network.

## Events

- `OnNetworkChanged`: Triggered when the network status changes.
- `OnConnectionChanged`: Triggered when the connection status changes.
- `OnJoinedRoom`: Triggered when the user joins a network room.

## Methods

- `OpenJoin()`: Opens a new network connection for joining a room.
- `StartRtcConnection()`: Starts the RTC connection.
- `StopRtcConnection()`: Stops the RTC connection.
- `SignalReadiness()`: Signals that the network is ready for sending and receiving data.

## How it works

The NetworkIO class provides functionality for managing network connections and handling various network events.

The class includes several methods that perform specific tasks. The `Awake()` method initializes the singleton instance of the NetworkIO class, ensuring that only one instance exists throughout the application.

The `Start()` method initializes the necessary variables and starts the connection indicator animation, visually showing the network connection status.

The `Update()` method handles network updates and invokes the OnNetworkChanged event, allowing other parts of the application to respond to changes in the network.

The `OpenJoin()` method creates a new connection to join a room, facilitating communication with other users in the network.

The `OnDisable()` method ceases the network connection when the script is disabled, ensuring that the connection is properly closed and resources are released.

The `StartRtcConnection()` method starts the Real-Time Communication (RTC) connection, enabling real-time data transmission between users.

The `StopRtcConnection()` method stops the RTC connection and closes the network connection, terminating the communication between users.

The `RoomIsFull()` method increments the room ID and retries joining the room, handling situations where a room has reached its maximum capacity.

The `CloseConnection()` method closes the network connection and resets the state, ensuring that all resources are properly released and the class is ready for subsequent connections.

The `OnFinishedLoadingRTC()` method handles the completion of loading the RTC connection, allowing the application to proceed with further operations once the RTC connection is fully established.

The `OnConnectionStarted()` method handles the successful connection to the network, providing a callback for performing actions upon successful network connection.

The `OnUserOnline()` method handles when a user comes online in the network, allowing the application to respond to user presence changes.

The `OnDestroy()` method handles the destruction of the network session, cleaning up any remaining resources before the network connection is fully closed.

The `ReceivePoseData()` method receives and processes pose data from other users, allowing the application to synchronize user actions across the network.

The `HandlePoseData()` method handles the processing of pose data, performing any required computations or actions based on the received data.

The `OnConnectedToNetwork()` method handles the event when connected to the network, providing a callback for performing actions upon successful network connection.

The `OnDisconnectedFromNetwork()` method handles the event when disconnected from the network, allowing the application to respond to network disconnection.

The `DeleteAvatar()` method removes an avatar from the scene, facilitating avatar management in a networked environment.

The `FadeToColour()` method animates the connection indicator color change, visually indicating changes in the network connection status.