# AvatarHand Class

Handles interaction events for the avatar's virtual hand.

## Fields

- `attachJoint`: Array of FixedJoint components used for attaching objects.
- `currentNearRigidBody`: Rigidbody component of the currently picked up object in near distance.
- `currentFarRigidBody`: Rigidbody component of the currently picked up object in far distance.
- `prevLayer`: Stores the previous layer of the attached object.

## Methods

- `Start()`: Initializes the `attachJoint` array.
- `ReceiveInstruction(AvatarHandlingData instruction)`: Receives an instruction for avatar interaction and performs the corresponding action.
- `PickupNear(GameObject target, AcquireData acquisition)`: Picks up the `target` object in the near distance.
- `DropNear(GameObject target, ReleaseData release)`: Drops the currently held object in the near distance.
- `PickupFar(GameObject target, AcquireData acquisition)`: Picks up the `target` object in the far distance.
- `DropFar(GameObject target, ReleaseData release)`: Drops the currently held object in the far distance.
- `SetLayerRecursively(GameObject obj, int newLayer)`: Sets the layer of `obj` and its child objects recursively to `newLayer`.

### How it works

- The `Start()` method initializes the `attachJoint` array with the first two `FixedJoint` components attached to the `AvatarHand` object.
- The `ReceiveInstruction(AvatarHandlingData instruction)` method handles the instructions for acquiring or releasing data from the avatar interaction. It checks if the `target` object exists and performs the corresponding action based on the `instruction.EventType` and `instruction.Distance`.
- The `PickupNear(GameObject target, AcquireData acquisition)` method picks up the `target` object in the near distance by accessing its `Rigidbody` component. It sets the object's position and rotation to the values provided in the `acquisition` parameter. It connects the `currentNearRigidBody` to the first `attachJoint`. It also changes the object's layer to "Tools" and remembers the previous layer.
- The `DropNear(GameObject target, ReleaseData release)` method drops the currently held object in the near distance. It sets the object's position and rotation to the values provided in the `release` parameter. It disconnects the `attachJoint` connection and applies forces to the `currentNearRigidbody` based on the `release` parameter. It resets the object's layer to the previous layer and forgets the previous layer and `currentNearRigidBody`.
- The `PickupFar(GameObject target, AcquireData acquisition)` method performs a similar action as `PickupNear`, but for the far distance. It connects the `currentFarRigidBody` to the second `attachJoint`.
- The `DropFar(GameObject target, ReleaseData release)` method performs a similar action as `DropNear`, but for the far distance. It disconnects the `attachJoint` connection and applies forces to the `currentFarRigidbody`.
- The `SetLayerRecursively(GameObject obj, int newLayer)` method sets the layer of the `obj` and its child objects recursively to `newLayer`.
