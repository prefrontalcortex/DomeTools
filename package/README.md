# Dome Tools
by prefrontal cortex

## Description

This package provides a highly efficient realtime fulldome toolkit for Unity.  
It has two modes: 

1. **Dome Warp, a novel invariant distortion technique**
   Much faster (single view), can be used with postprocessing, can be used with volumetric effects   
   Best suited for real-time applications  
   Best suited for spatial content (content that is around the viewer, not just on the floor plane)  
   View angle is limited to 180°  
   
2. **A traditional cubemap-based approach**
   Much slower (renders 5-6 views)  
   Flexible view angle (up to 360°)  

Both result in a fulldome projection in the common "Dome Master" format (equidistant azimuthal fisheye).    
The toolkit supports BiRP, URP, and HDRP render pipelines, and is provided as UPM package.  

## Installation

Import the package via Unity Package Manger.
- OpenUPM (recommended): 
- git: 

Compatibility:

- Unity 2021 LTS
- Unity 2022 LTS
- Render Pipelines: BiRP / URP / HDRP

## Usage

### Basic

Add the prefabs inside `pfc Fulldome > Runtime > Viewer > Prefabs` to your scene:
- Dome (camera rig)  
- Canvas (Dome Master)  
- Canvas (Final Output)  

Use <kbd>Dome (camera rig)</kbd> prefab as camera to move around.  
The camera is your viewers position.  

<kbd>>Canvas (Dome Master)</kbd> combines the rendered image with UI elements into the final dome master image.

<kbd>>Canvas (Final Output)</kbd> is the final output for your dome. You can use it for testing or for rendering to disk. By default, it contains an <kbd>NDI Sender</kbd> component for streaming the image to a virtual or physical dome.

### Use with NDI (recommended)

Select <kbd>Canvas (Final Output) > NDI Sender</kbd>. Follow the instructions to install the NDI package(s). Now, you can open any NDI-enabled application on your network and receive the Dome Master image as realtime NDI stream.

> **Note:** [NDI](https://newtek.com) is a high-bandwidth protocol. For best results, use a wired network connection. Audio streaming is currently not supported; please open an issue if you need this feature.  

### Use with Spout

Select <kbd>Canvas (Final Output)</kbd>. Add <kbd>Spout Sender</kbd> and follow the instructions to install the Spout package(s). You can now receive the Dome Master image as a Spout stream in any Spout-enabled application on your computer.  

### Producing real-time dome content

We recommend streaming to your physical or virtual dome via NDI in 4k or higher (depending on your media server / machine).

### Producing video content

Depending on your needs, you can either:

- use Unity Recorder and record the DomeMaster render texture into a video or image sequence
- use a hardware capture card to capture the monitor output

## Viewing on a simulated dome

### Dome Viewer for Meta Quest
<p><i>NDI Streaming only</i></p>
Sideload our virtual dome viewer or play from UnityEditor to your oculus quest 2 (and above) and you can choose your NDI stream. You need a fast wifi network.
2k render texture is recommended. 4k will run a little slow and just recommended for visual quality check.

### Dome Viewer for Desktop
Run our build or play in editor. Klak Spout is recommended if your dome source is on the same computer. Otherwise we recommend using NDI.

You can can choose between first person view and virtual reality.

> **Note:** The Dome Viewer application currently supports Windows only. Please open an issue if you need support for other platforms.  

### NDI Studio Monitor
For a quick check for your domemaster output you can use ndi studio monitor.  
https://ndi.video/tools/ndi-studio-monitor/

## Samples
There are several sample scenes for different render pipelines included with the pfc Dome Tools package.  
1. Open Package Manager and select pfc Dome Tools
2. Import the basics sample
3. Import the sample for the render pipeline you want to use.

## Technology Overview – invariant distortion vs cubemap

### Advantages
- highly efficient (rendering only one single camera instead of 5 or 6)
- standard camera cane be used with standard features
- postprocessing is working (no seams or breaks)
- vfx effects are working out of the box
- can be used with built-in, urp and hdrp

### Disadvantages against cubemap rendering
- it's not a mathematically perfect azimuthal equidistant projection (but very narrow), which is not really necessary because the viewer never stands perfectly in the center of a dome
- projections greater 170° need some technical workarounds

## Further Reading
- https://www.youtube.com/watch?v=vjvYJ7hgbyo
- https://en.wikipedia.org/wiki/Fulldome

## Contact Us
- contact@prefrontalcortex.de