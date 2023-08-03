# AvatarManager

This class manages the creation and removal of avatars in the scene. It also processes incoming avatar data and updates the avatars accordingly.

## Properties
- `Instance` (static): Returns the instance of the AvatarManager class.
- `AudioChannelOpen`: Gets or sets whether the audio channel is open.

## Events
- `OnDictionaryChanged`: Event triggered when the dictionary of other players changes. It passes the current number of players as a parameter.

## Methods
- `ResetScene()`: Resets the scene by removing all player avatars and clearing the dictionaries.
- `ProcessAvatarData(NodeInputData nodeFrame)`: Processes the incoming avatar data and updates the corresponding avatar if it exists. If the avatar does not exist, it sets the data as the current data frame to be used when creating a new avatar.
- `CreateNewPlayerAvatar(NodeDataFrame nodeFrame)`: Creates a new player avatar using the provided node frame data. Updates the dictionaries and invokes the OnDictionaryChanged event.
- `RemovePlayerAvatar(string userId)`: Removes the player avatar with the given user ID from the scene. Updates the dictionaries and invokes the OnDictionaryChanged event.

## How it works
- The `Awake()` method ensures that there is only one instance of the AvatarManager class.
- In the `Start()` method, dictionaries for other players and avatar controllers are initialized, and the AudioChannelOpen property is set to false.
- The `Update()` method checks if there is a ready-to-create avatar flag and a current data frame. If so, it creates a new player avatar using the CreateNewPlayerAvatar() method and resets the flag and current data frame.
- The `OnDestroy()` method is called when the instance is destroyed. It removes all player avatars and clears the dictionaries.
- The `ResetScene()` method removes all player avatars, clears the dictionaries, and sets AudioChannelOpen to false.
- The `ProcessAvatarData(NodeInputData nodeFrame)` method processes the incoming avatar data. If the avatar already exists, it updates the avatar. Otherwise, it sets the data as the current data frame to be used later.
- The `CreateNewPlayerAvatar(NodeDataFrame nodeFrame)` method creates a new player avatar using the provided node frame data. It updates the dictionaries and invokes the OnDictionaryChanged event.
- The `RemovePlayerAvatar(string userId)` method removes the player avatar with the given user ID from the scene. It updates the dictionaries and invokes the OnDictionaryChanged event.
