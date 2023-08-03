# ChatController Documentation

The `ChatController` is a MonoBehavior script that handles the chat functionality. It allows for sending and receiving chat messages.

## Properties
- `HasFocus`: A boolean property indicating whether the chat input field has focus.

## Serialized Fields
- `_input`: A serialized reference to the input field where the user types their chat message.
- `_output`: A serialized reference to the output text component where chat messages are displayed.

## Event
- `OnBroadcastMessage`: An event that is triggered when a chat message is broadcasted.

## Methods

### Update()
- Updates the chat feed by appending any new chat messages to the output text.
- Listens for the Return or Keypad Enter key press and calls `BroadcastChatMessage()` if the chat input field has focus.

### GetFocus()
- Sets the `HasFocus` property to true and activates the input field.

### LoseFocus()
- Sets the `HasFocus` property to false and deactivates the input field.

### BroadcastChatMessage()
- Sends a broadcasted chat message by creating an `AvatarChatData` object with the current draft message.
- Invokes the `OnBroadcastMessage` event with the chat message.
- Resets the current draft and clears the input field.

### UpdateChatFeed(string id, AvatarChatData acd)
- Updates the chat feed by appending the received broadcasted chat message to the output text.
- Sets the `newChatMessageReady` flag to true to indicate that a new chat message is ready to be displayed.

### UpdateChatMessage(string message)
- Updates the chat message based on the user's input.
- If the input message ends with a backtick character ('`'), it sets the input field text without the backtick.
- Otherwise, it sets the current draft message.

## How it Works
1. The script listens for updates each frame.
2. If there is a new chat message ready, it appends the message to the output text.
3. If the Return or Keypad Enter key is pressed and the input field has focus, it calls `BroadcastChatMessage()`.
4. The `BroadcastChatMessage()` method creates a new `AvatarChatData` object with the current draft message and invokes the `OnBroadcastMessage` event.
5. The `UpdateChatFeed()` method updates the chat feed by appending received broadcasted chat messages to the output text.
6. The `UpdateChatMessage()` method updates the chat message based on the user's input, handling special cases for the backtick character.
7. The `GetFocus()` and `LoseFocus()` methods manage the focus state of the input field.
