# RoomManager Documentation

## Description
The `RoomManager` class is responsible for managing rooms in a multiplayer game. It handles room creation, joining, and updating room information.

## Usage
1. Attach the `RoomManager` script to a game object in your scene.
2. Set the required serialized fields in the inspector:
   - `roomName: Text` - The text component to display the current room name.
   - `roomSelector: ControlDynamics` - The control dynamics component used for selecting rooms.
   - `roomParent: Transform` - The parent transform where the room labels will be added.
   - `roomTemplate: Transform` - The template transform used for creating room labels.
   - `CreateButton: Button` - The button component for creating a new room.
   - `JoinButton: Button` - The button component for joining a room.
3. Set the optional serialized fields:
   - `MaxPeers: int` - The maximum number of players allowed in a room (default is 6).
4. Start the scene to initialize the room manager.

## Public Members
- `MaxPeers: int` - The maximum number of players allowed in a room.
   - Getter: Returns the current maximum number of peers.
   - Setter: Sets the maximum number of peers.
- `CheckForRooms(): void` - Checks for available rooms.
- `UpdateRoomName(): void` - Updates the room name display.
- `SetRoomSize(newCapacity: int): void` - Sets the maximum number of players allowed in a room.
   - Parameters:
     - `newCapacity: int` - The new maximum capacity for the room.
- `SetRoomMode(isPublic: bool): void` - Sets the room mode (public or private).
   - Parameters:
     - `isPublic: bool` - `true` for public room, `false` for private room.
- `CreateRoom(): void` - Creates a new room.
- `JoinRoom(): void` - Joins a selected room.
- `ReceiveRoomInfo(message: string): void` - Callback method for receiving room information.
   - Parameters:
     - `message: string` - The JSON string containing the room information.

## Private Members
- `Start(): void` - Initializes the room manager.
- `Update(): void` - Updates the room manager every frame.
- `RoomIsFull(): void` - Callback method for a full room.
- `GetNewRoomName(): string` - Generates a new room name.
- `SetRoomName(newName: string): void` - Sets the new room name.
   - Parameters:
     - `newName: string` - The new room name to set.
- `GetRandomWord(source: byte[], wordLength: int): string` - Gets a random word from a source byte array.
   - Parameters:
     - `source: byte[]` - The source byte array containing words.
     - `wordLength: int` - The length of the word to retrieve.
- `RoomCreated(message: string): void` - Callback method for a successfully created room.
   - Parameters:
     - `message: string` - The room ID of the created room.
- `RoomJoined(message: string): void` - Callback method for a successfully joined room.
   - Parameters:
     - `message: string` - The room ID of the joined room.
- `MakeRoomListItem(room: RoomObject): void` - Creates a new room label and adds it to the room list.
   - Parameters:
     - `room: RoomObject` - The room object containing room information.

