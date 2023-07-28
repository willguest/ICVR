# `Grabbable` Class
A MonoBehaviour class that represents a grabbable object.

## Properties
- `WieldState`: A ControllerHand enum representing the current wield state of the grabbable object.

## Fields
- `controlPoseLeft`: A Transform representing the left control pose of the grabbable object.
- `controlPoseRight`: A Transform representing the right control pose of the grabbable object.
- `primaryPoseTrigger`: A string representing the primary pose trigger.
- `secondaryPoseTrigger`: A string representing the secondary pose trigger.

## Events
- `OnSecondHand`: An event triggered when a second hand grabs the grabbable object.
- `OffSecondHand`: An event triggered when a second hand releases the grabbable object.

## Methods
- `BeGrabbed(ControllerHand hand, Transform handTransform)`: Changes the wield state of the grabbable object to the specified hand. Returns a boolean indicating whether the grab action was successful.
- `Disengage(ControllerHand hand, Transform handTransform)`: Disengages the specified hand from the grabbable object. Returns a boolean indicating whether the disengage action was successful.
- `BeginAttraction(ControllerHand hand, Transform handTransform, System.Action<Grabbable> callback)`: Begins the attraction process by orienting the grabbable object towards the specified hand. Calls the specified callback function upon completion.
- `AttractObject(ControllerHand hand, Transform handTransform, System.Action callback)`: Attracts the grabbable object towards the specified hand. Returns the primary pose trigger.
- `_OrientToHand(Quaternion targetRotation, float duration, System.Action<Grabbable> callback)`: Coroutine that smoothly orients the grabbable object to the specified target rotation. Calls the specified callback function upon completion.
- `_SetLayerAfterDelay(float delay, GameObject obj, int newLayer)`: Coroutine that sets the layer of the specified object to the specified layer after the specified delay.
- `_SetLayerRecursively(GameObject obj, int newLayer)`: Recursively sets the layer of the specified object and its children to the specified layer.
- `_LerpToGrabPose(Transform handTransform, Transform controlPose, float duration, System.Action callback)`: Coroutine that lerps the grabbable object to the specified control pose based on the specified hand transformation. Calls the specified callback function upon completion.

## How It Works
The `Grabbable` class is used to implement grabbable objects with control dynamics. It provides methods for grabbing and releasing the object, as well as attraction actions. The class also includes coroutines for smooth orientation and lerping of the object.

When a hand grabs the object using the `BeGrabbed` method, the `WieldState` is updated accordingly. If a second hand grabs the object, the `OnSecondHand` event is triggered. When a hand releases the object using the `Disengage` method, the `WieldState` is updated and the `OffSecondHand` event is triggered if it was a secondary hand release.

The attraction process begins with the `BeginAttraction` method, which orients the object towards the specified hand. After that, the `AttractObject` method is called to gradually move the object towards the hand using lerping. The primary pose trigger is returned to indicate the completion of the attraction process.

The class also includes helper coroutines to set the layer of objects after a delay and recursively set the layer of objects and their children. These coroutines are used for layer management during grabbing and releasing.

The class can be extended by modifying the properties, fields, and events, as well as adding additional methods and coroutines.
