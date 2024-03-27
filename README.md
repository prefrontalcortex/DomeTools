# Dome Tools

This repository contains
- The **Dome Tools** Unity package, a highly efficient realtime fulldome toolkit.   
  It has two modes: **Dome Warp** – a novel invariant distortion technique, and a traditional **cubemap-based** approach. The toolkit supports **BiRP, URP, and HDRP** render pipelines, and is provided as UPM package.  

- A **Dome Viewer** application compatible with Windows, Quest 2 and more.   
  It can receive **NDI streams**, **Spout sources**, and display local **video and image files**. The viewer is provided as executable for Windows and on Meta AppLab, or can be compiled from source.  

Dome Tools and Dome Viewer are developed and maintained by [prefrontal cortex](https://prefrontalcortex.de).  

## Getting Started

The typical workflow is:

1. **Add Dome Tools to your project.**
   Add the Dome Tools package to your Unity project using the installer below. 

2. **Add Dome Tools prefabs to your scene.**
   Add the `Dome (camera rig)`, `Canvas (DomeMaster)` and `Canvas (Final Output)` prefabs to your scene. Position , rotate, and scale the camera rig where you want the viewer to be.  

1. **Create content.**  
   Create your scene, animate the dome rig or its parameters, animate postprocessing effects, and so on.  

2. **Preview content**  
   By default, your content gets streamed with NDI on your local network for easy viewing. Use the **Dome Viewer** to display the output on a Virtual Dome either on desktop or in VR, or display the NDI stream on a physical dome directly.     

You can use the Dome Tools package in your Unity project, and use the Dome Viewer to display the output on a Virtual Dome or a physical dome.

[Download Dome Tools for Unity package installer]()  

(Coming soon) [Download Dome Viewer for Windows]()  
(Coming soon) [Download Dome Viewer for Quest from App Lab]()  

The viewer supports both desktop usage and VR usage with hands or controllers.  
It has been tested on Windows (with and without VR), Quest 2/3, and Pico 4.  

For VR on Windows, use an OpenXR-compatible headset and runtime (for example, headsets supported by the Oculus App or Steam VR).  

## Dome Setup

> **Under construction 🏗️**. This section will contain information regarding various dome systems and how to display NDI content on them. If you want to provide steps for your dome system, please [open an issue](https://github.com/prefrontalcortex/Dome-Tools/issues/new/choose) or make a pull request.  

NDI is a widely supported protocol for streaming video content. It is supported by many dome systems. Please refer to the docs of your dome system for instructions on how to receive NDI streams.  

## Contributing

### Testing and developing the Dome Tools package

The `package` folder contains the UPM package for Dome Tools.   
To work on the package, open one of the test projects inside `projects/DomeTools*`, for example <kbd>DomeTools-2021.3</kbd>.  
These projects already contain a local reference to the package, so all changes to the package will be shared between the test projects.  

Before making a package release, make sure to rename the `Samples` folder to `Samples~`. This will hide the folder inside Unity, and allows users of the package to install the provided Samples from the Package Manager window. When installing a sample, Unity will copy the files to the project's `Assets` folder, out of the immutable package folder.  

### Testing and developing the Viewer

The `projects/DomeViewer` folder contains the Unity project for the Virtual Dome Viewer. It currently uses Unity 2021.3 LTS.  

Before opening the project, **make sure you have git installed** since the project references a git package – [learn more](https://docs.unity3d.com/Manual/upm-git.html).  To work on the viewer, open the project inside `projects/DomeViewer`. Required packages will be installed automatically.

> **Note**: If you get a warning regarding the klak.ndi package not being installed, please install git first as described above.  

#### Building

When building for Android, make sure to provide a unique application identifier and either your own signing key or a development key. A custom Android manifest is provided that allows network access to receive NDI streams.  

## Further Reading
- [Interactive Domes – Fulldome Festival Presentation 2020](https://www.youtube.com/watch?v=vjvYJ7hgbyo)
- [Fulldome on Wikipedia](https://en.wikipedia.org/wiki/Fulldome)

## Contact Us
- contact@prefrontalcortex.de, refer to Felix Herbst
- [@prefrontlcortex](https://twitter.com/prefrontlcortex) and [@hybridherbst](https://twitter.com/hybridherbst) on Twitter