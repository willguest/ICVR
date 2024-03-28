# Island Collective Virtual Reality 

Welcome to the ICVR repository, a Unity-based toolkit for designing cutting-edge immersive experiences on the Internet Computer.

## 📦 Steps to Create an ICVR Experience

Make sure that [Unity 2020 LTS](https://unity.com/releases/editor/archive#download-archive-2020) is installed on your system.

1) Start a new URP project using [Unity Hub](https://unity.com/download).

<img src="https://github.com/willguest/ICVR/assets/24574013/02397692-9cc0-4172-a282-0f6e9e958d61)" align="right" width="200px"/>

2) Import the ICVR_Core and, if desired, the ICVR_DLC unity packages. \
*Assets → Import Package → Custom Package*

4) Open the ICVR Setup window. \
*Window → WebXR → ICVR Setup*

5) Use the interface to complete the ICVR setup:

   - Switch build target to WebGL
   - Add dependencies: Newtonsoft Json and WebXR Export (as scoped UPM package)
   - Click on *Enable ICVR Settings* to add the `ICVR` Scripting Define Symbol.
   - Apply relevant settings. Note that **this will override your current settings**.

6) Check that WebXR Export is enabled for the WebGL target. \
*Project Settings → XR Plug-in Management*

<img src="https://github.com/willguest/ICVR/assets/24574013/d91ea42b-d38f-4902-98d2-783179d3aad7" align="right" width="200px"/>

6) Open one of the test scenes \
Bare bones: *Assets/ICVR/Scenes* \
Templates: *Assets/ICVR_Scenes*, using [DLC Content](https://github.com/willguest/ICVR/releases)

8) Open the *Build Settings* window, add the current scene and click *Build*

9) For compatibility with the [ICVR-React](https://github.com/willguest/icvr-react) template, place the build in a folder called `unity_build`.

<br clear="right"/>


## 🛠️ Technology Stack
- [Unity 2020 LTS](https://unity.com/releases/programmer-features/2020-lts-tier2-features): Immersive 3D development platform.
- [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline): A multiplatform render pipeline, suitable for WebXR.
- [WebXR Export](https://github.com/De-Panther/unity-webxr-export/): Turns WebGL builds into immersive experiences.
- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/): A modern, object-oriented programming language.


## 📄 Documentation

Each class is documented in a corresponding markdown file, and linked in the class definition summary. The [top level](https://github.com/willguest/ICVR/tree/develop/Documentation) contains instructions on how to add documentation to any contributions.

## 🌐 Self-Hosting

This repository is intended to be used with [icvr-canister](https://github.com/willguest/icvr-canister), which allows the Unity WebGL builds to be self-hosted on the Internet Computer blockchain. See that repository's README for more information.


## 💛 Sponsorship

The framework is open-source and was initially funded by non-dilutive grants from the Internet Computer. I welcome sponsorship in all forms and look forward to scaling this project as more resources become available. Please visit my [sponsorship page](https://github.com/sponsors/willguest) for more information.


## ⚖️ License

Code and documentation copyright 2023 Will Guest. Code released under the [Mozilla Public License 2.0](https://www.mozilla.org/en-US/MPL/2.0/FAQ/).
