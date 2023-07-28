# RigidDynamics Class
A MonoBehaviour class responsible for managing physics-driven interaction.

## Properties
- `Mass`: The mass of the rigidbody.
- `Volume`: The volume of the rigidbody.
- `Throw`: The forces applied to the rigidbody during a throw.
- `UsesGravity`: Indicates whether the rigidbody uses gravity.

## Methods

### `GetVolume()`
Returns the volume of the rigidbody. If the rigidbody has a mesh, it calculates the volume using a volume solver. Otherwise, it calculates the volume based on the rigidbody's mass and density.

### `GetNewMass()`
Calculates and sets the mass of the rigidbody based on the density and volume.

### `FixedUpdate()`
Updates the component's state during each physics step. Calculates the current velocity and angular velocity.

### `GetCurrentVelocity()`
Calculates and returns the current velocity of the rigidbody.

### `GetNormalisedAngVel(Vector3[] angleArr)`
Calculates and returns the normalized angular velocity based on the array of previous rotation angles.

### `ToAngularVelocity(Quaternion q)`
Converts a quaternion to the corresponding angular velocity.

### `GetForcesOnRigidBody()`
Calculates and returns the forces applied to the rigidbody during a throw. Calculates the translational force based on the current velocity, and the rotational force based on the angular velocity.

## How It Works
The `RigidDynamics` class is a MonoBehaviour script that provides functionality for managing the dynamics of a rigidbody in Unity. It includes properties for controlling the mass, volume, and forces applied to the rigidbody during a throw. 

The `GetVolume()` method calculates the volume of the rigidbody by either using a volume solver for mesh-based objects or by using the formula mass/(density * 1000) for non-mesh objects.

The `GetNewMass()` method calculates and sets the mass of the rigidbody based on the density and volume.

The `FixedUpdate()` method is called during each physics update and updates the component's state. It calculates the current velocity and angular velocity of the rigidbody.

The `GetCurrentVelocity()` method calculates the average velocity of the rigidbody based on the previous velocities.

The `GetNormalisedAngVel()` method calculates the average angular velocity and returns the normalized vector.

The `ToAngularVelocity()` method converts a quaternion to the corresponding angular velocity vector.

The `GetForcesOnRigidBody()` method calculates and returns the forces applied to the rigidbody during a throw. It calculates the translational force based on the current velocity and the rotational force based on the angular velocity.

Overall, the `RigidDynamics` class provides a way to dynamically calculate the forces acting on a rigidbody in a physics simulation.