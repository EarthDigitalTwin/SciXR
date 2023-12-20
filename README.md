# SciXR
Extended reality toolkit for Earth and planetary sciences.

## About
SciXR is a fully immersive science data visualization application intended for use by Earth and planetary scientists. It was developed to help scientists better understand their data and to shorten the feedback loop of scientific discovery. The extended reality environment allows scientists to explore their data in real time using natural depth perception, tracked hand motions, and specialized tools (e.g., data slicer, object scaling, data-querying laser pointer) all within a room-scale environment unconfined by the limits of a computer monitor. Collaborative features allow multiple scientists to interact with data within the same environment. SciXR also takes advantage of the GPU to enable high performance real time visualizations and on-the-fly computations. These features allow scientists to visualize and interact with data in ways that desktop applications and a traditional mouse, keyboard, and display setup cannot. Integration with Apache SDAP enables users to perform statistical data analysis such as understanding spatial averages and viewing trends in time series information. Support for WMS/WMTS geospatial imagery services including NASA GIBS allows users to add overlays and obtain further context. Although initially built for Earth scientists, SciXR may be extended in the future to other fields such as Planetary science, microbiology, and astrodynamics.

## Background
This software was developed in support of the [Ice Sheet System Model](https://issm.jpl.nasa.gov/) and Fire Alarm projects at the Jet Propulsion Laboratory, California Institute of Technology.

It was originally developed for Hololens and then HTC Vive. In the fall of 2023, it was upgraded by an intern to support Meta Quest Pro.

## Features
These are the features of SciXR that work in the Meta Quest Pro version as of December 2023, when we moved from Unity 2019 and VRTK 3.3 to Unity 2022 and XRI.

- [x] A virtual and augmented reality environment for exploring science data
- [x] Viewing data with depth perception using supported XR devices
- [x] Real time interaction with science data allowing four dimensions (XYZ and Color) over time
- [ ] Playback 3D animations of science data (needs more testing)
- [x] Tracked controller motions that provide intuitive interaction with data
- [ ] Multiple users working within the same virtual room
- [x] Dynamic color scaling (including interpolation method selection) and colormap swapping on the data
- [ ] Model selection, rotation, stretching capability
- [ ] Data inspector with a virtual laser pointer
- [ ] Arbitrary plane slicing using a slicer tool
- [ ] Data mesh extrusion color animations of the data over time
- [ ] Screenshots and videos of the data via OS features
- [x] Customized virtual rooms (e.g., office space replication)
- [ ] Integration with geospatial data formats using GDAL (see Development Information below)
- [ ] Integration with WMS/WMTS geospatial imagery services including NASA GIBS (kind of works)
- [x] Integration with the Apache Science Data Analytics Platform (SDAP) for common statistical data analysis (basic features: catalog and basic data download)
- [ ] Snap pop-oup menus to controller
- [ ] 3D Tiles integration

## Development Information
This software was developed using Unity 2022 and XRI. Here are some important notes as of December 2023:

- There's a bug in OpenXR 1.8 (a plugin that we use) where it will forcibly disable internet permissions in any built Android apps. It is possible to disable it, but the bug is that when you rebuild the application (e.g. when you change code), that switch is toggled again. So you have to toggle it every time. Just keep the window open — see [here](https://forum.unity.com/threads/unity-removes-android-permission-internet-in-the-build-apk-after-build-is-completed.1466654/) for how to get to that window. (Apparently this was fixed in 1.9.1, but the version in my Unity 2022 is still 1.8.)
- Loading files works differently on Android than on PC. Moreover, the previous system for loading files did not conform to Unity best practices. I have attempted to rectify the situation by adding local demo datasets to `StreamingAssets` and making `WebRequest`s to fetch them. However, this means that the previous dataset-parsing logic (contained in `Assets/Scripts/Data`), most of which expects to be able to just read a file at a path, will not work. (In general, everything having to do with i/o needs to be changed for Quest.) I had the time to change `ModelJSReader` to work with Android/Quest. This primarily means adding the `MetadataFromRaw` and `MetadataFromPath` functions (and see their callers for how it's used). Similar things will work with PLY and Matlab, although maybe we don't want to be focusing on loading local datasets anyway.
- I haven't tested whether this is backwards-compatible with Vive. It's possible that you can just add the Vive plugin to XRI and it'll just work.
- For working on 3D Tiles and GLTF support — those plugins, developed by JPL, are just sitting in the Assets folder, not managed as a plugin. This will certainly cause problems.
- There are plenty of UI rough edges and flat-out missing features. Rough edges include the fact that SDAP data appears only to be rendered in one eye, the variable names don't show up on the DataInstance reliably, etc. Missing features include the laser inspector, the slicer, stretching data, and satellite imagery.
- The SDAP user interface needs to be improved.
