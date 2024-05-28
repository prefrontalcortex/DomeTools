# Dome Tools: Dome Creator and Dome Viewer

This repository contains two main components:

- The **Dome Creator** Unity package, a highly efficient realtime fulldome toolkit.   
  It has two modes: **Dome Warp** – a novel single-view rendering technique, and a traditional **cubemap-based** approach. The toolkit supports **BiRP, URP, and HDRP** render pipelines, and is provided as easy-to-use UPM package. It outputs both audio and video as realtime NDI streams ready for display on a dome.  

- A **Dome Viewer** virtual planetarium compatible with Windows, Quest 2, 3 and more. It can receive **NDI streams**, **Spout sources**, and display local **video and image files**. The viewer is provided as executable for Windows as well as VR app on Meta App Lab and SideQuest, or can be modified and compiled from source.  

**Dome Creator** and **Dome Viewer** are developed and maintained by [prefrontal cortex](https://prefrontalcortex.de).  

## Getting Started

Use **Dome Creator** to produce real-time, interactive fulldome content. Stream it over the network with **NewTek NDI** and view your content in **Dome Viewer** and other NDI-enabled applications.

1. **Add Dome Creator to your project.**
   Add the Dome Creator package to your Unity project using the installer below. 

2. **Add the "Dome Tools" prefab to your scene.**  
   You can find the prefab under `Packages > pfc Dome Tools > Runtime > Dome Tools`.  
   Position , rotate, and scale the object to fit your scene.   
   See [Dome Creator – Readme](https://github.com/prefrontalcortex/DomeTools/blob/main/package/README.md) for more information.  

3. **Install the NDI packages**
   Select Dome Tools and click <kbd>Fix</kbd> next to "NDI Package is not installed".  

3. **Create content.**  
   Create your scene, animate the dome rig or its parameters, create and animate postprocessing effects, place and animate audio sources, and so on. Iterate quickly by previewing your content in the **Dome Viewer**.

4. **Preview and showcase your content**  
   By default, your content gets streamed with NDI on your local network for easy viewing. Use the **Dome Viewer** to display the output on a Virtual Dome either on desktop or in VR, or display the NDI stream on a physical dome directly.     

Typically, you will use the **Dome Creator** package in your Unity project, and start the **Dome Viewer** app on Quest 2/3 in the same local network. Once you're ready, the same NDI stream can be received by many planetarium AV systems directly – in realtime. 

## Download and Installation

**Creating content**  
[Download Dome Creator for Unity – Package Installer 📦](https://package-installer.glitch.me/v1/installer/OpenUPM/com.pfc.dome-tools?registry=https://package.openupm.com)  

**Viewing content**  
[Download Dome Viewer for Windows]()  
[Download Dome Viewer for Quest 2/3/Pro from App Lab](https://www.meta.com/en-gb/experiences/4747161018651543/)   
<!-- (Coming soon) [Download Dome Viewer for Quest from SideQuest]()  -->

The viewer supports both **Desktop usage** and **VR usage** with **hands or controllers**.  
It has been tested on **Windows** (with and without VR), **Quest 2/3/Pro**, and **Pico 4**.  

For VR on Windows, use an **OpenXR-compatible headset and runtime** (for example, headsets supported by the Oculus App or Steam VR).  

## Output to a Dome AV System

> **Under construction 🏗️**. This section will contain information regarding various dome systems and how to display NDI content on them. If you want to provide steps for your dome system, please [open an issue](https://github.com/prefrontalcortex/DomeTools/issues/new/choose) or make a pull request.  

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
