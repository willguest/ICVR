# PlatformManager

The `PlatformManager` class is responsible for managing the platform state and VR capabilities. It provides a way to check if VR is supported and start VR mode.

## Public Properties:

- `Instance` - a static property that returns the singleton instance of `PlatformManager`.

- `IsVRSupported` - a boolean property that indicates whether the platform supports VR.

- `XrState` - a property of type `WebXRState` that stores the current state of the XR (extended reality) platform.

## Public Methods

- `StartVR()` - a method that sets the `discoveredVR` flag to `true`. This flag is checked in the `Update()` method to start VR mode.

## Private Methods

### `Awake()` 
This method is a Unity callback that checks if an instance of `PlatformManager` already exists. If it does, it destroys the current instance. Otherwise, it sets the current instance as the singleton instance.

### `Update()` 
This checks if the `discoveredVR` flag is set to `true`. If it is, it toggles VR mode using the `WebXRManager` singleton instance and resets the `discoveredVR` flag.

### `OnEnable()` 
This method subscribes to events triggered by the `WebXRManager` singleton instance. It listens for changes in XR capabilities and updates the `IsVRSupported` property accordingly.

### `OnDisable()` 
This method unsubscribes from the events triggered by the `WebXRManager` singleton instance.

### `CheckCapabilities()` 
This method is called when XR capabilities are updated. It updates the `IsVRSupported` property based on the `canPresentVR` capability.

### `OnXRChange()` 
This method is called when the XR state changes. It updates the `XrState` property with the provided state.

