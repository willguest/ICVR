# A Guide to Designing Interactions Using the ICVR Framework


## Overview

Both near (within reach) and far (line of sight) interactions are considered, as well as the transition from far to near. 

*Grip* button is used for near pickup and throwing. 

*Trigger* is used for distant manipulation and flinging objects. 

Pressing *Grip* while holding an object with *Trigger* will first cause it to rotate then, after a short delay, with attract it to the hand, into a particular grip. Releasing *Grip* during the rotation phase will cancel the interaction.


## Interaction Design 

### Collisions and Layers

This framework relies on object collisions and layers for primary and chained effects. Maintaining all collision causes jitter and unexpected behaviour as the forces involved approach infinity. From a design perspective, three layers are important:

+ `Furniture` (layer 9) collides with Objects, Tools, amd other items of Furniture, giving you functional surfaces that do not collide with the character, thus not restricting the workspace.

+ `Objects` (layer 10) are non-static, movable entities. Anything on this layer will be accessible by the controllers and should most likely have a `Rigidbody` component. Object collide with everything except the Body and any Wearables.

+ `Scene` (layer 11) is for fixed objects in the environment. These are not expected to move and will collide with everything, apart from Buttons (layer

+ `Buttons` (layer 12) are interface objects that are used as triggers. Buttons only interact with Tools amd Objects, making them less likely to be triggered accidentally.

+ `Body` (layer 13) is the character collider, which remains on/in the Scene, but otherwise does not trigger collisions.

+ `Wearable` (layer 14) are body-locked functional objects that only interact with Tools.

+ `Tools` (layer 15) are the functional interaction endpoints. To begin with, the Hands/Controllers are Tools. When `Grabbable` objects are being used, they become Tools and the hands become part of the Body layer. This allows interactions to be stacked - you can pick up a Tool that picks up a Tool (etc.), that picks up an Object.



### Useful Modules

`RigidDynamics` is used to make object throwable. It uses the movement of the controller (mouse or hand) to give realistic release velocities and, if given a readable mesh, will calculate the volume of the object and, using the "density" parameter, will update the mass accordingly. Collisions are not affected.

When using `Grabbable` as a component, you can specify the relative position and orientation of the object when it is picked up with the Jedi-style mechanic (trigger -> grip). See Grabbable.md for instructions on how to set this position and rotation. Note than, even with this component, near interaction is unaffected.

The `ObjectInterface` is similar, but orients the hand to suit the object. This can identify interaction potential or remove ambiguity from interfaces. 

`ControlDynamics` allows objects to send event triggers on interaction with other objects, based on collision data. This is useful for levers, user interfaces or anywhere where second order effects are needed with user interaction.
