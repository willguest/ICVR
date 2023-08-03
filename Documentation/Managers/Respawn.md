# Respawn Class

This class manages the respawning of game objects. It contains several functions to handle different types of objects and special cases.

## Variables

- `DefaultRespawnPose`: A serialized field of type Transform that represents the default respawn position and rotation.
- `characterCollider`: A serialized field of type GameObject that holds the collider for the character.
- `characterRoot`: A serialized field of type GameObject that represents the root object of the character.
- `specialCases`: A List<string> that stores the names of special cases.

## Functions

### `Start()`

This function is called when the class is initialized. It adds the name of the character collider to the specialCases list.

### `OnTriggerEnter(Collider col)`

This function is called when a collider enters the trigger area. It takes the game object of the collider as input and calls the ManageRespawn() function.

### `ManageRespawn(GameObject respawnObject)`

This function manages the respawn of the given game object. It checks if the game object's name is included in the specialCases list. If it is, it calls the ReplaceSpecial() function. Otherwise, it calls the ReplaceObject() function.

### `ReplaceObject(GameObject obj)`

This function replaces the given game object back to its default respawn position and rotation. It stores the name and scale of the object and retrieves its Rigidbody component and SharedAsset component if they exist. If a SharedAsset component exists, it sets the object's position, rotation, and scale to the default values stored in the SharedAsset component. If not, it sets the object's position to the DefaultRespawnPose position, rotation to the DefaultRespawnPose rotation, and scale to the stored scale value. Finally, it removes any momentum from the object by setting its velocity and angular velocity to zero.

### `ReplaceSpecial(GameObject specialObject)`

This function handles the special case for the character object. It sets the character root object's position to the zero vector plus an upward offset, rotation to the identity rotation, and velocity and angular velocity to zero.
