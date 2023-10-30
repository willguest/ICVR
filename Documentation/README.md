#  ICVR Documentation

> This is the entry point for the AI-supported documentation. The files and folders should reflect the code structure, to aid automatic updates and change monitoring, until an autonomous system is in place.

## Getting Started

For setup instructions, see the README file in the root folder of the codebase. The following instructions assume you have Unity 2020 LTS, running a Universal Render Pipeline (URP) project.


### Interaction

`RigidDynamics` is used to make object throwable. It uses the movement of the controller (mouse or hand) to give realistic release velocities and, if given a readable mesh, will calculate the volume of the object and, using the "density" parameter, will update the mass accordingly. Collisions are not affected.

When using `Grabbable` as a component, you can specify the relative position and orientation of the object when it is picked up with the Jedi-style mechanic (trigger -> grip). See Grabbable.md for instructions on how to set this position and rotation. Note than, even with this component, near interaction is unaffected.

The `ObjectInterface` is similar, but orients the hand to suit the object. This can identify interaction potential or remove ambiguity from interfaces. 

`ControlDynamics` allows objects to send event triggers on interaction with other objects, based on collision data. This is useful for levers, user interfaces or anywhere where second order effects are needed with user interaction.



