## SharedAsset Class Documentation

### Introduction
The `SharedAsset` class is a MonoBehaviour script used to manage shared assets in a game or application. It provides functionality to handle asset registration and removal from a shared asset manager.

### Public Properties
- `IsBeingHandled`: A boolean property that gets or sets whether the asset is currently being handled or not.

### Private Properties
- `Id`: A private string property that holds the unique ID of the asset.
- `DefaultLocation`: A private Vector3 property that holds the default location of the asset.
- `DefaultRotation`: A private Quaternion property that holds the default rotation of the asset.
- `DefaultScale`: A private Vector3 property that holds the default scale of the asset.
- `isNetworkAvailable`: A private boolean property that represents the availability of a network connection.
- `_manager`: A private reference to the `SharedAssetManager` instance.

### Methods
- `Awake()`: A method that is called when the script instance is being loaded. It initializes the default location, rotation, and scale of the asset, and checks if the `SharedAssetManager` instance is available.
- `Start()`: A method that is called on the frame when a script is enabled. It gets the `SharedAssetManager` instance and includes the asset in the register if the network is available.
- `OnDestroy()`: A method that is called when the component is destroyed. It removes the asset from the register if the network is available.
- `GetGameObjectPath(GameObject obj)`: A static method that returns the hierarchical path of a game object.

### How it works
The SharedAsset class is a MonoBehaviour that represents a shared asset in the experience. When an instance of this class is created, it initializes its default properties by obtaining the current position, rotation, and scale of its transform.

During the Awake method, the class checks if a SharedAssetManager instance exists, indicating that a network is available. If so, it sets the isNetworkAvailable flag to true.

In the Start method, if a network is available, the class retrieves the SharedAssetManager instance and assigns it to the local _manager variable. It then obtains the unique ID of the asset by calling the GetGameObjectPath method with the GameObject of the asset. Finally, it registers the asset with the _manager by calling the IncludeAssetInRegister method.

If the asset is destroyed, the OnDestroy method is called. If a network is available, it removes the asset from the SharedAssetManager by calling the RemoveAssetFromRegister method.