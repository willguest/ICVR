## SharedAssetManager Documentation

### Summary

The SharedAssetManager class is a Unity MonoBehaviour class that provides a centralized storage for shared GameObject assets. It ensures that only one instance of the SharedAssetManager exists in the scene and allows adding, retrieving, and removing assets from a shared asset register.

### Static Properties

**Instance**
A static property that returns the instance of the SharedAssetManager.

### Private Variables

**_instance**
A private static variable to hold the instance of the SharedAssetManager.

### Public Properties

**SharedAssetRegister**
A public property of type Dictionary<string, GameObject> that holds the shared asset references.

### Public Methods

**void Awake()**
This method is called when the script instance is being loaded. It ensures that only one instance of the SharedAssetManager exists in the scene. If another instance is found, it is destroyed. If no other instance exists, the current instance is assigned to the _instance variable. Additionally, it initializes the SharedAssetRegister dictionary.

**GameObject RetrieveAssetFromRegister(string id)**
This method retrieves a shared asset from the SharedAssetRegister dictionary based on the given id. It returns the corresponding GameObject if found, otherwise, it returns null.

**bool IncludeAssetInRegister(string Id, GameObject asset)**
This method includes a new asset in the SharedAssetRegister dictionary. If the asset with the given id does not already exist in the dictionary, it is added and the method returns true. Otherwise, the method returns false.

**bool RemoveAssetFromRegister(string Id)**
This method removes an asset from the SharedAssetRegister dictionary based on the given id. If the asset exists in the dictionary, it is removed and the method returns true. Otherwise, the method returns false.


### How it works
The SharedAssetManager class provides a centralized storage for shared GameObject assets. It uses a dictionary to store the assets, with a unique identifier as the key. This allows for easy retrieval and removal of assets based on their identifier.

When including an asset in the register using the IncludeAssetInRegister method, the method checks if the identifier already exists in the register. If it does not exist, the asset is added to the register. If the identifier already exists, the asset is not added and the method returns false.

When retrieving an asset from the register using the RetrieveAssetFromRegister method, the method simply returns the asset associated with the provided identifier. If the identifier is not found in the register, null is returned.

When removing an asset from the register using the RemoveAssetFromRegister method, the method checks if the identifier exists in the register. If it does, the asset is removed from the register. If the identifier does not exist, nothing is removed and the method returns false.

The Singleton pattern is used to ensure that only one instance of SharedAssetManager exists in the scene. This is achieved by checking if an instance already exists in the Awake method. If an instance exists, any duplicates are destroyed, and if no instance exists, the current instance is set as the instance of SharedAssetManager.

This class can be used in Unity projects to manage shared asset references, enabling easy retrieval and removal of assets based on their unique identifier.