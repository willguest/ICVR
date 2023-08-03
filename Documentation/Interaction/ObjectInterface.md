# ObjectInterface Class

## Public Methods

### `ToggleActivation()`
This method toggles the activation of the object. It changes the value of the `IsBeingUsed` variable and performs certain actions based on the current state.

## Private Methods

### `OnTriggerEnter(Collider other)`
This method is called when the object enters a trigger collider. It checks if the object is being used, and if not, it checks if the collider belongs to a controller interaction script (XRControllerInteraction). If the collider does belong to a controller interaction script and it is not currently controlling any object, the object receives control from the collider.

### `OnTriggerExit(Collider other)`
This method is called when the object exits a trigger collider. It checks if the object is being used, and if yes, it checks if the collider belongs to the same controller interaction script (XRControllerInteraction). If it does, the object loses control.

### `ReceiveControl(GameObject manipulator)`
This method is called to receive control from a manipulator (the controller). It assigns the manipulator's model object to the `currentManipulator` variable, disables the hand colliders of the manipulator to prevent interference with object colliders and rigidbodies, sets the manipulator as the parent of the object, determines the active control pose based on the manipulator's name (left or right), and lerps the object's position and rotation to match the control pose. Finally, it sets the `IsBeingUsed` variable to true.

### `LoseControl()`
This method is called to lose control of the object. It removes the manipulator as the parent of the object, resets the object's pose if it has a ControlDynamics component, lerps the object's position and rotation to zero and identity respectively, enables the hand colliders of the manipulator, sets the `currentManipulator` variable to null, and sets the `IsBeingUsed` variable to false.

### `LerpToControlPose(GameObject objToLerp, Vector3 endPosition, Quaternion endRotation, float duration)`
This coroutine lerps the position and rotation of the object (`objToLerp`) to the specified end position and rotation over the specified duration. It uses a while loop to perform the lerp calculation over time and yield a frame until the duration is reached.

---

## How It Works
When the object enters a trigger collider and is not being used, it checks if the collider belongs to a controller interaction script and if the controller is not currently controlling any object. If true, the object receives control from the collider.

When the object exits a trigger collider and is being used, it checks if the collider belongs to the same controller interaction script. If true, the object loses control.

When the object receives control from a manipulator, it assigns the manipulator's model object to the `currentManipulator` variable, disables the hand colliders of the manipulator, sets the manipulator as the parent of the object, determines the active control pose based on the manipulator's name, and lerps the object's position and rotation to match the control pose.

When the object loses control, it removes the manipulator as the parent of the object, resets the object's pose if it has a ControlDynamics component, lerps the object's position and rotation to zero and identity respectively, enables the hand colliders of the manipulator, and sets the `currentManipulator` variable to null.

The `ToggleActivation()` method toggles the activation state of the object by changing the value of the `IsBeingUsed` variable. If the object is currently being manipulated and `currentManipulator` is not null, it calls the `LoseControl()` method.
