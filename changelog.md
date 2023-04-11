# SciXR Changelog

### 1/24/2020

Updating previous networking setup
- Deactivated previous NetworkManager object
- Deprecated files using Unity’s UNet, which will no longer be supported in newer versions of Unity
- Imported PUN 2

### 1/28/2020

Using Mirror
- Removed PUN 2, server connections would not go through
- Imported Mirror

Set up avatars
- Deprecated old Avatars and created new prefabs with Mirror scripts
- Customized HUD location to upper right side of the screen
- Configured avatars to follow HMD location and synchronize position across network
	- AvatarController.cs

Data synchronization
- Started setup of data synchronization from host to clients
	- NetworkDataManager.cs
	- FileLoadObject/FileLoad2DObject calls SyncData in NetworkDataManager instance after loading a dataset in LoadData

### 1/30/2020

Improving avatars
- AvatarController only sets position and name if instance is bound to local player
- AvatarController updates position in Update instead of setting a parent transform
- Avatar spawns for each player connected to the network and synchronizes position across all players
- AvatarController inherits from NetworkBehaviour instead of Monobehaviour
- AvatarController syncs player names with a Command/ClientRPC pair and SyncVar

Script changes
- Removed changes to FileLoadObject and FileLoad2DObject
- NetworkDataManager inherits from NetworkBehaviour instead of MonoBehaviour

### 2/4/2020

Sending data over the network
- NetworkSerialFile datatype stores SerialFile information in a way that can be transmitted over the network
- NetworkDataManager includes methods for converting between NetworkSerialFile and SerialFile
- NetworkDataManager calls a ClientRPC when the host loads data to request that all clients load the same data
- Removed NetworkIdentity from DataInstance prefab
- Created DataManager object with NetworkIdentity to allow it to call RPCs
- NetworkPrefabManager spawns a DataManager instance over the network once it is connected, then disables itself
- Switched order of SetColorMode methods in DataObject
- DataObject methods call the UpdateData method in NetworkDataManager with their arguments converted to strings
- NetworkDataManager calls a ClientRPC when the host updates data to request that all clients update their data

Better avatars
- Changed local position of elements of the AvatarVR prefab in order to match better with the camera location

### 2/6/2020

Improved data synchronization
- Added NetworkSyncPosition script on the DataInstance prefab to call MoveData in the NetworkDataManager and synchronize position across clients

Bug fixes
- Local player disables their own avatar in AvatarController so that it doesn’t cover their camera
- Fixed bug with setting max bound of colorbar

### 2/7/2020

Changed position of avatars of players without VR capabilities to the location of the desktop interface camera

### 2/11/2020

UI changes
- Changed position of the UsernameInput field
- UsernameInput field is deactivated once the user connects to the network

### 2/18/2020

Fixed SceneController so that OpenVR is not initialized when disableVR is set to true
- Added None as main supported VR SDK- change if machine has SteamVR

Changed FileLoad2DMenu to use the number of files in DataLoader for numSlots instead of 10

DataOverlay can be moved up and down on the z axis from UI menu
- Added another button in the UI to open a canvas with slider
- Added SetOverlayZ method in DataObject to change transform of the overlay
- OverlayControls script calls SetOverlayZ in DataObject after scaling z
- Added new case in NetworkDataManager RpcRequestUpdateData

Point cloud size controls
- Added PSIZE variable size in v2f in DataMapPointCloud shader
- vert sets size to a variable _PointSize between 0 and 1
- Changing _PointSize currently has no effect on the actual mesh

### 2/20/2020

Started adding time series capabilities
- Added timeseries field to SerialData.DataType enum
- DataLoader loads timeseries data differently in LoadFiles
	- Gets all subdirectories using Directory.GetDirectories
	- Checks if subdirectories contain the string "_tseries"
	- If true, then manually create a SerialFile
		- type = DataType.timeseries
		- identifier = dirFull up to _tseries
		- path = dirFull
		- Sets fileName and lastModified as the single file-readers do
- DataLoader uses CreateDataObject differently for timeseries SerialFile's
	- Does not set vertexCount, triangleCount, notes, or runtimeName
	- Adds TimeSeriesController component to the game object
- Changed ModelJSReader, ModelPLYReader to allow null dataObject argument

### 2/21/2020

Activate the OverlayButton in GenerateView if applicable

Continued work on time series data
- Implemented StepForward and StepBackward methods in TimeSeriesController
- Activates child object with TimeSeriesController component instead of adding to the DataInstance
- Added a time button which activates for time series data and opens a canvas with buttons bound to forward/backward step
- Sets name of a TextMesh inside TimeSeriesController to display the name of the current file

### 2/25/2020

Continued work on time series data
- Edited DataInstance prefab to have a button for time series data which toggles a menu to step through time

### 2/27/2020

Continued fixing initial loading bug
- In DataObject, ProcessResults:
	- Used mesh.vertices.Length instead of data.vertexCount
- In DataObject, GenerateMeshData:
	- Used serialData.vertexCount instead of data.vertexCount to loop through the vertices
	- Set the DataObject instance's data field to the passed in serialData
- In DataObject, CreateDataObject:
	- After instantiating the GameObject, activates TimeSeriesController and returns if it is a timeseries data type
	- TimeSeriesController finishes creating the DataObject and loads meshes

Can load initial time series data but still can't step through different meshes

Added data container resizing feature
- Added DataInstanceRatio script to DataInstance prefab which changes the location of the front and back plates based on the ratio of the dataset

### 3/3/2020

Worked on data container resizing feature
- Made Start method of OverLayUI public
- DataInstanceRatio changes the scale of the FilterBox and size of the BoxCollider of FilterBox to correctly set ticks and their locations
- Instantiates new base plates to use as tiles instead of using one base plate

### 3/5/2020

Fixed data container resizing feature
- Changed scale of base plates in order to fit with the dataset

Worked on time series data loading
- Stores a list of SerialMeshes
- Uses SwapView method to load a new SerialMesh to the data instance when the user steps through the data

### 3/6/2020

Merged with changes to multiplayer
- Does not allow clients to move data instances by setting the VRUI VRTK_InteractableObject component's isGrabbable field to false
- Created NetworkManagerHUD script to determine the correct offsetX and offsetY values
- Added height and width fields to the NetworkManagerHUD script

UI changes
- Updated ratios to use for size and placement of the NetworkManagerHUD
- Changed color of text in NetworkManagerHUD to black for better readability

Stress tested time series handling
- Copied ArgoSalinity files so that 9 datasets will be in the directory
	- Having all 9 loaded at once did not decrease performance
- Increased value of LoadBuffer
	- Causes Unity to crash / not be able to load data
- Allowed LoadBuffer to be changed from the editor

Starting adding multiplayer capabilities to time series data
- TimeSeriesController calls RpcRequestUpdateData when StepForward or StepBackward are triggered
- Added new cases to NetworkDataManager for StepForward and StepBackward

### 3/10/2020

Worked on loading meshes for time series data in the background
- Added LoadFiles IEnumerator in TimeSeriesController but do not start it because it is very slow

Runtime bug fixes
- Checked that manager.manager is not null in NetworkSyncPosition
- Fixed NullReferenceExceptions in MeshVRControls
- Fixed NullReferenceException in ThreadManager

3/12/2020

Starting integrating MRTK to switch to AR
- Deleted background scene
- Deleted VRUI
- Deleted SteamVR and VRTK plugins
- Deleted Assets/Scripts/Control/vrtk
- Imported Windows Mixed Reality Toolkit

3/17/2020

Worked on AR interface
- Created PressableButton prefab
- Finished hand menu
	- Added pressable buttons to toggle file loading menu, slicer, and network menu
- Worked on AR file loading menu

3/18/2020

Continued working on AR interface
- Finished AR file loading menu
- Created menu in HoloLens view space to use because hand menu won't work in emulator

3/19/2020

Continued working on AR interface
- Screen menu opens up either file loading, network, or slicer menu depending on which button is hit

3/23/2020

Fixing data loading
- Adjusted file object button distances
- Data loading now functional
- Added MRTK components to buttons on the DataInstance prefab to trigger the popup canvases when pressed with AR hand

Fixed bug where data overlay button does not appear for point clouds

3/24/2020

Fixing data instance prefab
- Created new PressableButton2D prefab for usage on UI canvases
- Switched existing buttons to new prefab type

3/26/2020

Fixing file loading menu
- Swtiched from a scrolling collection of files to paging through the files with arrow buttons
- Created PageObjectCollection script to control the file selection display and bound arrow buttons to it

3/27/2020

Fixed button bindings for hand menu
Changed text wrapping and sizing settings for file names in the menu
Added interactions on data instance prefab
- Added ManipulationHandler on new GameObject in data instance to allow rotation, moving, and scaling
- Added new grabbing object to DataInstanceRatio

3/30/2020

Bug fixes

4/8/2020

Changed locations/bounds of menus to show on Hololens
Disabled manager scripts and other scripts which were interfering with controls in Hololens Emulator

4/9/2020

Fixed main menu visuals and text

4/10/2020

Fixed issues with DataLoader.cs
- No longer able to detect changes to data files

## Notes for next time:
- Fix buttons on popup canvases
- add multiplayer capabilities to time series data and test them
- figure out a way to pre-load data
- Fix dropdown
- matlab?
- data loading menu needs to allow a larger angle in order to click on side arrows

Scripts that depended on VRTK:
- MenuHandVRControls
- BoxClipControls
- FileLoadObject
- MeshVRControls
- PointerHandVRControls
- ColliderDisableInteractable
- HoverHighlight
- ParentAxisSecondary
- RotateGrab
- ScaleGrab
- DataSubMesh
- DataLoader has list of VR pointers but does not seem to use them
- DemoController

## Open issues:
- Inspect data value of one point in a point cloud
- Select individual points from a point cloud
- MATLAB integration
- Overlay multiple coincident datasets as one plot
- Control point size for point clouds
	- Stored in SubMesh object
