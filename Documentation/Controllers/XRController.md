# XRController Documentation

## Description
This script is responsible for controlling the XR controller and handling hand interactions in a virtual reality environment. It includes functions for button events, grabbing and releasing objects, and movement control.

## Public Variables, Functions, and Attributes

- `debugHand`: a boolean indicating whether the hand is in debug mode
- `CharacterRoot`: a reference to the root object of the character
- `BodyController`: a reference to the BodyController script component
- `MaxInteractionDistance`: the maximum distance for interactions with objects
- `GameObjectRenderers`: an array of Renderers to be toggled on and off based on XR state
- `hand`: the controller hand (left or right)
- `PointerLayerMask`: the layer mask for interaction pointers

#### Enumerations:

- `ButtonTypes`: represents the different types of buttons on the controller
- `AxisTypes`: represents the different types of axis on the controller (e.g. trigger, grip)
- `Axis2DTypes`: represents the different types of 2D axes on the controller (e.g. thumbstick, touchpad)

#### Events:

- `OnControllerActive`: invoked when the controller is activated or deactivated
- `OnHandActive`: invoked when the hand is activated or deactivated
- `OnHandUpdate`: invoked when the hand position or rotation is updated
- `AButtonEvent`: event for A button press
- `BButtonEvent`: event for B button press
- `OnHandInteraction`: invoked when hand interaction occurs

#### Methods:

- `SetGripPose(string gripPose)`: sets the grip pose animation trigger
- `ModifyJoint(int jointIndex, Rigidbody connectedBody = null)`: modifies a joint between the controller and a connected rigidbody
- `IsUsingInterface`: a flag indicating whether the hand is bound to an object
- `IsControllingObject`: a flag indicating whether the controller is articulating equipment

---

## Unity Functions

### `OnEnable()`
- Subscribes to events for XR change, controller update, and hand update.
- Sets the controller and hand to inactive.

### `OnDisable()`
- Unsubscribes from events for XR change, controller update, and hand update.
- Sets the controller and hand to inactive.

### `Start()`
- Initializes variables and objects.
- Gets the left VR camera.
- Toggles renderers based on XR state.
- Sets the jump tick time.

### `FixedUpdate()`
- Runs code only in a WebXR session or in debug mode.
- Detects swimming.
- Handles movement and rotation using thumbsticks.
- Handles picking up and dropping objects using triggers and grips.
- Handles jumping or swimming using thumbstick click.
- Handles button events when using the interface.
- Sets pointers and active far mesh.

### `LateUpdate()` 
>(Conditional compilation for UNITY_EDITOR or !UNITY_WEBGL)
- Updates input buttons.

---

## State Functions

### `ToggleRenderers(bool onOrOff)`
- Toggles the renderers of the game objects based on the active state.

### `OnHandUpdateInternal(WebXRHandData handData)`
- Updates the hand position and rotation based on the hand data received.
- Sets the controller and hand active based on the hand data.
- Invokes the hand update event.

### `SetControllerActive(bool active)`
- Sets the controller active state and invokes the controller active event.

### `SetHandActive(bool active)`
- Sets the hand active state and invokes the hand active event.

### `OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)`
- Handles the XR state change event.
- Toggles renderers based on the XR state.

### `DetectSwimming()`
- Detects if the character is swimming based on the elevation.
- Adjusts the character's mass and swimming state accordingly.

### `JumpSwim()`
- Handles jumping or swimming action based on the current state.
- Adds force to the character's rigidbody.

---

## Button Functionality

### `TryUpdateButtons()`
- Description: Update button states based on input device values.
- Parameters: None.
- Return Type: void.

### `OnControllerUpdate(WebXRControllerData controllerData)`
- Description: Update controller position, rotation, and button states based on controller data.
- Parameters: 
  - `controllerData` (WebXRControllerData): Data received from the controller update.
- Return Type: void.

### `GetAxis(AxisTypes action)`
- Description: Get the value of a specific axis based on the input action type.
- Parameters: 
  - `action` (AxisTypes): The type of axis to get the value for.
- Return Type: float.

### `GetButtonDown(ButtonTypes action)`
- Description: Check if a specific button is currently being pressed down.
- Parameters:
  - `action` (ButtonTypes): The type of button to check.
- Return Type: bool.

### `GetButtonUp(ButtonTypes action)`
- Description: Check if a specific button has been released.
- Parameters:
  - `action` (ButtonTypes): The type of button to check.
- Return Type: bool.

---

## Character Movement

### `MoveBodyWithJoystick(float xax, float yax, float multiplier = 1.0f)`
- Description: Move the character body based on joystick input.
- Parameters:
  - `xax` (float): The value of the joystick x-axis input.
  - `yax` (float): The value of the joystick y-axis input.
  - `multiplier` (float, optional): The multiplier for the movement speed. Default is 1.0f.
- Return Type: void.

### `RotateWithJoystick(float value)`
- Description: Rotate the character body based on joystick input.
- Parameters:
  - `value` (float): The value of the joystick input for rotation.
- Return Type: void.

---

## Interaction Functions

### PickupFar()
This function is used to pick up a far object. It first checks if the user is interacting with a button, and if so, invokes the button's pressed event. If not, it retrieves the rigidbody of the distant object and sets it as the current object. It then moves the rigidbody to the position of the user's hand and attaches it to the hand using a joint. It also checks if the object has a shared asset component, and if so, sets it as the current shared asset and invokes an acquisition event. Finally, it triggers an animation to point at the object.

### DropFar()
This function is used to drop the currently held far object. If the user is interacting with a button, it invokes the button's released event. It then detaches the object from the hand and applies a throw force to it based on its velocity. If the object has a shared asset component, it releases the asset and invokes a release event. Finally, it triggers an animation to relax the hand.

### BeginAttractFar(Rigidbody targetRB)
This function is called when the user tries to attract a far object using a magnetic power. If the object has a grabbable component, it disables its gravity and detaches it from the hand. It then calls the grabber's BeginAttraction function.

### AttractFar(Grabbable sender)
This function is called during the process of attracting a far object. If the user is still gripping the object tightly, it releases the object and triggers the PickupNear function. Otherwise, it drops the far object.

### PickupNear()
This function is used to pick up a near object. It first checks if the user is already holding a near object. If not, it retrieves the rigidbody of the nearest object and sets it as the current object. It then moves the rigidbody to the position of the user's hand and attaches it to the hand using a joint. If the object has a shared asset component, it sets it as the current shared asset and invokes an acquisition event. Finally, it triggers an animation to hold the object.

### DropNear()
This function is used to drop the currently held near object. It clears the list of nearby contact rigidbodies and checks if the current object is a grabbable object. If it is, it disengages the grab and releases the object. It then applies a throw force to the object based on its velocity. If the object is a controlled object, it finishes the interaction with the object. If the object has a shared asset component, it releases the asset and invokes a release event. Finally, it resets the animation trigger.

### InvokeAcquisitionEvent(string target, Transform interactionTransform, ManipulationDistance distance)
This function is used to invoke an acquisition event for a shared asset. It checks if the target id is not empty and if there are other players present. If so, it creates an acquire data object and builds an interaction event frame to be invoked.

### InvokeReleaseEvent(string target, GameObject interactionObject, ManipulationDistance distance, ThrowData throwData)
This function is used to invoke a release event for a shared asset. It checks if the target id is not empty and if there are other players present. If so, it creates a release data object and builds an interaction event frame to be invoked.

### BuildEventFrame(string targetId, ManipulationDistance distance, AvatarInteractionEventType eventType, AcquireData acqDataFrame, ReleaseData relDataFrame)
This function is used to build an interaction event frame. It takes in the target id, manipulation distance, event type, and optional acquire/release data, and returns an avatar handling data object.

---


## Low-level Interaction Functions

This code contains several low-level interaction methods related to object manipulation and button pressing. The methods in this code are as follows:

### SetActiveFarMesh()

This method is responsible for setting the active far mesh when there is a focused object. It performs different actions based on the layer of the object. It also handles the appearance of the pointer for heavy objects and buttons.

### OnTriggerEnter(Collider other)

This method is triggered when a collider enters the trigger zone. It checks the layer of the entered object and performs specific actions based on the layer. It handles button pressing and adds interactable objects or tools to the list of near contact rigid bodies.

### OnTriggerExit(Collider other)

This method is triggered when a collider exits the trigger zone. It checks the layer of the exited object and performs specific actions based on the layer. It handles button releasing and removes interactable objects or tools from the list of near contact rigid bodies.

### GetDistantRigidBody()

This method returns the nearest rigid body from the list of far contact rigid bodies. It calculates the distance between each contact body and the player's position and returns the body with the minimum distance.

### GetNearRigidBody()

This method returns the nearest rigid body from the list of near contact rigid bodies.

### PlacePointer()

This method places the pointer at the position calculated by casting a ray from the player's hand.

### CastControllerRay()

This method casts a ray from the player's hand and detects any objects within the maximum interaction distance. It updates the current object variable and returns the hit point of the raycast.






