# Character Controller 
This documentation describes the public objects used in the DesktopController class of an interactive WebXR scene.

## Inspector Variables
rotationEnabled (bool): Enable or disable rotation control in the Unity editor.
translationEnabled (bool): Enable or disable translation control in the Unity editor.
mouseSensitivity (float): The sensitivity of the mouse for rotation control.
straffeSpeed (float): The lateral movement speed.
seaLevel (float): The level at which the character is considered swimming.
currentVehicle (GameObject): The object that the player can move around with using the mouse and keyboard.
headObject (GameObject): The object associated with the player's head that moves around with the camera.
Cursor Objects
cursorForScene (Texture2D): The cursor texture to use when hovering over the scene.
cursorForObjects (Texture2D): The cursor texture to use when hovering over objects.
cursorForInteractables (Texture2D): The cursor texture to use when hovering over interactable objects.
crosshair (SimpleCrosshair): The crosshair object used for aiming.

## Public Attributes
- IsSwimming (bool, getter and setter): Indicates whether the character is swimming.
- CurrentObject (GameObject, getter and setter): The currently viewed game object.
- currentDistance (float, readonly): The current distance between the character and the viewed object.
- currentHitPoint (Vector3, readonly): The current point of interaction with the viewed object.

## Cursor Event Handling
- OnNetworkInteraction (delegate event): Event that triggers when interacting with the cursor. It provides an AvatarHandlingData object.


## Private Variables
The private variables are used internally in the class and are not intended for public access.

- xrState: The current state of the WebXR instance.
- isGameMode: Indicates whether the game mode is enabled.
- attachJoint: The fixed joint used for object attachment.
- runFactor: The factor by which the character's movement speed is multiplied when running.
- jumpCool: The cooldown for jumping.
- minimumX, maximumX: The minimum and maximum rotation values around the X-axis.
- minimumY, maximumY: The minimum and maximum rotation values around the Y-axis.
- rotationX, rotationY: The current rotation values around the X and Y axes.
- startRotation, currentHeading: The initial and current rotations of the head object.
- hotspot: The hotspot position of the cursor.
- cMode: The cursor mode.
- isMouseDown: Indicates whether the mouse button is currently pressed.
- isDragging: Indicates whether an object is currently being dragged.
- globalInvertMouse: Global modifier for inverting the mouse movement.
- runOne: Flag used for internal logic.
- currentSharedAsset: The currently handled shared asset.
- currentElevation: The current elevation of the character.
- activeMesh: The currently active mesh.
- prevMeshName: The name of the previously active mesh.
- screenPoint, offset: Variables used for object dragging.
- jumpTick: The timestamp for the last jump.
- triggerTick: The timestamp for the last trigger interaction.
- buttonDown: Indicates whether a button is currently pressed.
- currentButton: The currently viewed game object.
- Unity Functions
- The Unity functions are called automatically by the Unity engine at specific points in the game's lifecycle.

- OnEnable(): Called when the script component is enabled.
- OnDisable(): Called when the script component is disabled.
- Awake(): Called when the script component is initialized.
- Start(): Called before the first frame update.
- Update(): Called once per frame.



## Character Movement
### OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
Handles the change in WebXR state. Updates the xrState variable and sets the cursor parameters.

### GetCameraRotationFromMouse(float sensitivity, float invertMouse)
Calculates the camera rotation based on mouse input. The rotation values (rotationX and rotationY) are updated based on the mouse movement. The sensitivity and invertMouse parameters adjust the rotation speed and direction. The rotation values are clamped within the specified range.

### RelativeQuatFromIncrements(float rotX, float rotY)
Returns a Quaternion representing the camera rotation based on the X and Y increments. The rotationX and rotationY values are used to create Quaternion rotations around the Y and X axes. The resulting Quaternion represents the current heading of the camera.

### MoveBodyWithKeyboard(GameObject referenceObject, float multiplier = 1.0f)
Moves the character body based on keyboard input. The function calculates the movement vectors based on the Horizontal and Vertical input axes. The referenceObject parameter is the GameObject that represents the character body. The multiplier parameter adjusts the movement speed. The character moves in the desired direction by translating the referenceObject.

### SetCameraRotation()
Sets the camera rotation based on mouse input. If game mode is enabled, the camera rotation is updated based on the GetCameraRotationFromMouse function. If not in game mode and the mouse button is pressed, the camera rotation is updated based on the inverted mouse movement. The dragMod and globalInvertMouse variables control the rotation speed and direction.

### JumpSwim()
Makes the character jump or swim based on the current state. If the character is swimming and the jump cooldown has passed, a swim force is applied to the character's GameObject. If the jump cooldown has passed, a jump force is applied to the character's GameObject.

### ClampAngle(float angle, float min, float max)
Clamps the angle value within the specified range. The angle value is wrapped around 360 degrees and then clamped between the minimum and maximum angles.

### RotateCamera(Quaternion targetRot, float speed)
Rotates the camera smoothly towards the target rotation. The target rotation is the Quaternion representing the desired camera rotation. The speed parameter controls the rotation speed. The rotationTimer variable is used to smoothly interpolate between the current and target rotations over time.

## Object interaction
### ToggleGameMode()
Handles the toggle between game mode and normal mode. Updates the isGameMode variable and calls the SetCrosshairVisibility() and SetCursorParameters() functions.

### SetCrosshairVisibility()
Toggles the visibility of the crosshair. Changes the alpha value of the crosshair color based on the isGameMode variable.

### SetCursorParameters()
Sets the cursor parameters based on the current WebXR state and game mode. Hides or shows the cursor and locks or unlocks the cursor based on the WebXR state and game mode.

### SetCursorImage()
Sets the cursor image based on the type of object the cursor is currently over. Changes the cursor image and adjusts the crosshair gap based on the layer of the current object.

### PickUpObject(GameObject ooi)
Picks up an object and makes it draggable. Sets the activeMesh variable to the object being picked up and enables the interaction with the object. If the object has a Rigidbody component, it is made non-kinematic and connected to the attachJoint. If the object is a shared asset, an acquisition event is invoked. Updates the isDragging variable to true.

### ReleaseObject()
Releases the currently picked up object. Resets the isDragging variable to false. If the object has a RigidDynamics component, it is released by disconnecting it from the attachJoint and applying a linear and angular force to it. If the object is a shared asset, a release event is invoked. If the object has a ControlDynamics component, it finishes the interaction with the object. Resets the activeMesh variable to null.

### InvokeAcquisitionEvent(string target, Transform interactionTransform)
Invokes an acquisition event with the specified target and interaction transform. Creates a new AcquireData object with the current time, position, and rotation of the interaction transform. Builds an AvatarHandlingData object with the acquisition event and returns it.

### InvokeReleaseEvent(string target, GameObject interactionObject, ThrowData throwData)
Invokes a release event with the specified target, interaction object, and throw data. Creates a new ReleaseData object with the current time, position, rotation, and throw data of the interaction object. Builds an AvatarHandlingData object with the release event and returns it.

### BuildEventFrame(string targetId, AvatarInteractionEventType eventType, AcquireData acqDataFrame = null, ReleaseData relDataFrame = null)
Builds an AvatarHandlingData object with the specified target ID, event type, and optional acquisition or release data. Returns the built AvatarHandlingData object.

### GetActiveMesh(GameObject ooi)
Gets the active mesh for the specified object. Checks if the object has a Rigidbody component and returns the object if it does. Otherwise, checks if the object has a parent with a Rigidbody component and returns the parent object. Finally, checks if the object has any child objects with a Rigidbody component and returns the first child object found. If no Rigidbody component is found in the object or its parent or child objects, returns null.

### ActivateObjectTrigger(GameObject currObj)
Activates the trigger of the specified object. If the object has a PressableButton component and the time since the last trigger tick is greater than 0.5 seconds, sets the buttonDown variable to true, sets the currentButton variable to the specified object, and invokes the ButtonPressed event of the PressableButton component.

### ReleaseObjectTrigger(GameObject currObj)
Releases the trigger of the specified object. If the object has a PressableButton component, sets the buttonDown variable to false, sets the currentButton variable to null, and invokes the ButtonReleased event of the PressableButton component.

### ViewObject(GameObject viewObject)
Updates the view of the current object. Returns the name of the current object if it is not null and its name is different from the previous mesh name. Otherwise, returns an empty string.

### DoubleClick()
This function handles the double click event. If the current object is not null, it sends a message called "OnDoubleClick" to the current object. This function provides an easy way to make things interactable through code. Any scripts attached to the GameObject that have a function called 'OnDoubleClick' will get called here.

### ScreenRaycast(bool fromTouch = false)
This function performs a raycast from the screen to determine the object under the cursor. It takes an optional parameter called "fromTouch" which is set to false by default. If the function is called while dragging an object, it simply returns the current object.

The function casts a ray from the camera either to the center of the screen if in game mode or to the mouse position if not in game mode. It then checks if the ray intersects with any objects on the default raycast layers within a maximum distance of 15 units.

If an intersection is found, the function updates the current hit point and distance variables and returns the game object of the intersected object. If no intersection is found, the function returns null.