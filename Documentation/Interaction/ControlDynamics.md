# `ControlDynamics` Class
A MonoBehaviour class responsible for managing control dynamics.

## Properties
- `State`: A string representing the current state of the control dynamics.

## Fields
- `resetReference`: A Transform representing the reference position and rotation for resetting the control dynamics.
- `stickySensors`: A boolean indicating whether the sensors should stick to the target.
- `controlEvents`: A list of ControlEvent objects representing the available control events.

## Events
- `onControlAction`: An event triggered when a control event occurs.

## Methods
- `AddControlEvent(ControlEvent newEvent)`: Adds a new control event to the list of control events. Returns the total number of control events.
- `StartInteraction(GameObject target)`: Starts an interaction with the specified target object.
- `FinishInteraction()`: Finishes the current interaction.
- `ResetPose()`: Resets the pose of the control dynamics object.
- `StickToTarget(Transform target)`: Sticks the control dynamics to the specified target.
- `Awake()`: Called when the script instance is being loaded.
- `OnDestroy()`: Called when the script instance is being destroyed.
- `InitialiseControlEvents()`: Initializes the control events.
- `OnTriggerEnter(Collider other)`: Called when another collider enters the trigger.
- `OnTriggerExit(Collider other)`: Called when another collider exits the trigger.
- `ProcessControlEvent(GameObject go)`: Processes a control event triggered by the specified game object.
- `ResetGripOrigin()`: Resets the grip origin of the control dynamics.
- `_onControlAction(ControlEvent cEvent)`: Event handler for when a control action is triggered.
- `DebugControlEvent(ControlEvent c)`: Logs debug information about a control event.

---

## How It Works

The `ControlDynamics` class manages the control dynamics of an object. It allows the user to define control events, which consist of a state, a sensor collider, and a control effect. The control dynamics can be triggered by entering the sensor collider. When a control event is triggered, the control effect associated with the event is invoked.

The class also provides methods for starting and finishing interactions with target objects, as well as resetting the pose of the control dynamics. The `StickToTarget` method allows the control dynamics to stick to a specified target.

The class ensures the integrity of the control events by checking if the control effects are assigned. If a control effect is not assigned, a warning message is logged.

The control dynamics can be customized by modifying the properties and fields of the class, as well as adding and removing control events.
