# SciXR
Extended reality toolkit for Earth and planetary sciences.

## About
SciXR is a fully immersive science data visualization application intended for use by Earth and planetary scientists. It was developed to help scientists better understand their data and to shorten the feedback loop of scientific discovery. The extended reality environment allows scientists to explore their data in real time using natural depth perception, tracked hand motions, and specialized tools (e.g., data slicer, object scaling, data-querying laser pointer) all within a room-scale environment unconfined by the limits of a computer monitor. Collaborative features allow multiple scientists to interact with data within the same environment. SciXR also takes advantage of the GPU to enable high performance real time visualizations and on-the-fly computations. These features allow scientists to visualize and interact with data in ways that desktop applications and a traditional mouse, keyboard, and display setup cannot. Integration with Apache SDAP enables users to perform statistical data analysis such as understanding spatial averages and viewing trends in time series information. Support for WMS/WMTS geospatial imagery services including NASA GIBS allows users to add overlays and obtain further context. Although initially built for Earth scientists, SciXR may be extended in the future to other fields such as Planetary science, microbiology, and astrodynamics.

## Background
This software was developed in support of the [Ice Sheet System Model](https://issm.jpl.nasa.gov/) project at the Jet Propulsion Laboratory, California Institute of Technology.

## Features
* A virtual and augmented reality environment for exploring science data
* Viewing data with depth perception using supported XR devices
* Real time interaction with science data allowing four dimensions (XYZ and Color) over time
* Playback 3D animations of science data
* Uses GPU to enable highly responsive visualizations and interactions
* Tracked hand motions that provide intuitive interaction with data
* Multiple users working within the same virtual room
* Dynamic color scaling (including interpolation method selection) and colormap swapping on the data
* Intuitive object resizing using hand gestures
* Data inspector with a virtual laser pointer
* Arbitrary plane slicing using a slicer tool
* Data mesh extrusion color animations of the data over time
* Screenshots and videos of the data via a device's default export tools
* Screenshots of the data via a virtual camera as an arbitrarily large image (via vector graphics)
* Customized virtual rooms (e.g., office space replication)
* Integration with geospatial data formats using GDAL
* Integration with WMS/WMTS geospatial imagery services including NASA GIBS
* Integration with the Apache Science Data Analytics Platform (SDAP) for common statistical data analysis

## Development Information
This software was developed using Unity.
Execute `RUN_BUILD.bat` to generate a Windows executable.

### Modified Plugin Files
* Assets\TextMesh Pro\Resources\Shaders\TMP_SDF-Mobile.shader
* Assets\TextMesh Pro\Resources\Shaders\TMP_SDF.shader
* Assets\Plugins\VRTK\Source\Scripts\Internal\VRTK_UIGraphicRaycaster.cs
* Assets\Plugins\VRTK\Source\Scripts\UI\VRTK_UIPointer.cs
