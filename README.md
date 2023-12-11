# Island Collective Virtual Reality 

Welcome to the ICVR repository, a Unity-based toolkit for designing cutting-edge immersive experiences on the Internet Computer.

## 📦 Create a New Project

Make sure that [Unity 2020 LTS](unityhub://2020.3.48f1/b805b124c6b7) is installed on your system.

Start a new URP project using [Unity Hub](https://unity.com/download).

Import the ICVR_Core and, if desired, the ICVR_DLC unity packages. \
*(Assets → Import Package → Custom Package)*

Open the ICVR Setup window. \
*(Windows → WebXR → ICVR Setup)*

<img src="https://github.com/willguest/ICVR/assets/24574013/219f7977-eb3b-4367-84ef-452acff0bb33" align="right" width="200px"/>
Use the relevant buttons to complete the ICVR setup:

   - Switch build target to WebGL
   - Add dependencies: Newtonsoft Json and WebXR Export (as scoped UPM package)
   - Click on *Enable ICVR Settings* to add the `ICVR` Scripting Define Symbol.
   - Import relevant settings. Note that this will **override your current settings**.

<img src="https://github.com/willguest/ICVR/assets/24574013/d91ea42b-d38f-4902-98d2-783179d3aad7" align="right" width="200px"/>
Make sure that WebXR Export is enabled for the WebGL target.

*(Project Settings → XR Plugin Management)*.

Open one of the scenes inside *Assets/ICVR/Scenes* to begin with an empty scene.

If also using the DLC (see [Releases](https://github.com/willguest/ICVR/releases)), open one of the scenes in *Assets/ICVR_Scenes* for some design examples.

### 👓 Things to Remember When Building

In the *Build Settings* window, don't forget to add the current scene.

For compatibility with the [ICVR-React](https://github.com/willguest/icvr-react) template, place the build in a folder called `unity_build`.

<br clear="right"/>


## 🛠️ Technology Stack
- [Unity 2020 LTS](https://unity.com/releases/programmer-features/2020-lts-tier2-features): Immersive 3D development platform.
- [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline): A multiplatform render pipeline, suitable for WebXR.
- [WebXR Export](https://github.com/De-Panther/unity-webxr-export/): Turns WebGL builds into immersive experiences.
- [C#](https://learn.microsoft.com/en-us/dotnet/csharp/): A modern, object-oriented programming language.


## 📄 Documentation

Each class is documented in a corresponding markdown file, and linked in the class definition summary. The top level can be found [here](https://github.com/willguest/ICVR/tree/develop/Documentation), along with instructions on how to contribute.


## ⚖️ License

Code and documentation copyright 2023 Will Guest. Code released under the [Mozilla Public License 2.0](https://www.mozilla.org/en-US/MPL/2.0/FAQ/).
