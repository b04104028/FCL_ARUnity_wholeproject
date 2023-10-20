# Interactive Data Visualization and Augmented Reality of Urban Photovoltaic
## Introduction
This project is an iPad mobile app created for the Future Cities Lab Exhibition 2023 at ETH Zurich, aiming to visualize influencial data of photovoltaic power usage using augmented reality. It utilizes 3D printed models of two sites, Zurich and Singapore, and is composed of three key modules: energy demand(cooling and heating), mobility flow (both vehicle and non-vehicle), and social economics(energy trading within these regions). 
By inputing these data in a specific format, the project creates a virtual overlay on the physical 3D models, providing a direct and clear view of the real situations in the designated areas.

## Environment and dependencies
The `ARFoundationAllBackup` branch represents the final version of this repository. It is compatible with [Unity](https://unity.com/download) 2021.2 and later.

This project is developed based on [AR Foundation 5.1](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html) and its demonstration example [AR Foundation Sample Project](https://github.com/Unity-Technologies/arfoundation-samples).

It depends on 2 Unity packages:
* [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html)
* [Apple ARKit XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arkit@5.1/manual/index.html) on iOS

To build on iOS mobile devices, it requires an external development environment:
* [Xcode](https://developer.apple.com/documentation/xcode) available on macOS and developer account


## How to use 

### case1: Directly run on device without modification

This project can be directly built to iOS device providing input data in specific formats.   
In this case, Unity is not required. Follow the steps below:  

[Xcode part]   
(See [tutorial video](https://www.youtube.com/watch?v=Z-gija1aAhw) that demonstrates the following steps.)
1. Clone this repository and go to `1610FinalBuild` folder, open `Unity-iPhone.xcodeproj` using Xcode 14.0 or later.
2. Plug in your iOS device (with [developer mode](https://docs.expo.dev/guides/ios-developer-mode/#) enabled) to the compmuter, and select this device on top of the window in Xcode.
3. In Xcode, click on "Unity-iPhone" on folder hierarchy on the top left, and then "Signing & Capabilities" in the middle.
4. In "Signing & Capabilities" page, check "automatically manages signing"
5. "Add an account" under "Team" selection options, and then choose your account for Team.
6. Run the project by clicking the triangle "run" button on top left. This way Xcode builds the app to your device. 
[Device part]  
7. On your iOS device, sign in to the Apple developer account the same as "Team"
8. "[Verify](https://www.youtube.com/watch?v=vsi2MsEW764)" this app in Settings > General > VPN & Device Management 
9. Transfer the data for visualization to the device under App document folder(Example file formats are described [below](#Data-Format)). There are many ways to do this. For example, using [iMazing](https://imazing.com) or [iExplorer](https://macroplant.com/iexplorer), navigate to ../Apps/AR-Foundation/Developer/Documents and put the files in this folder. For the app document file path, see [Unity Persistant File Path](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html).
10. The app is ready to use. 

### case 2: Modify project before building to device
To modify project on models, scenes, scripts,...,etc, follow the steps below:  
[Unity part]  
1. Install [Unity Hub](https://unity.com/download). In Unity Hub, go to "Install" to install Unity 2021.2 or later, add 2 modules: "Visual Studio for Mac" and "iOS Build Support".  
2. Clone this repository and open the Unity project at the root of this repository.
3. Modify the projects as needed
4. Go to [Build Settings](https://docs.unity3d.com/Manual/BuildSettings.html), select a target platform([iOS](https://docs.unity3d.com/Manual/iphone-BuildProcess.html) in this sample), and build this project in a new folder. In this way, unity automatically build a Xcode project. 
[Xcode part]
5. After finishing building, go to that new folder and open `Unity-iPhone.xcodeproj` using Xcode 14.0 or later.
6. Follow the steps of [case1](#case1:-Directly-run-on-device-without-modification)


## Data Format
### Energy Demand
### Mobility
### Energy Trade


# Table of Contents

| Sample scene(s) | Location in project |
| :-------------- | :---------- |
| [Menu](#Menu) | ../Asset/Scenes/ImageTracking/Menu |
| [Images](#Scenes) | ../Asset/Scenes/ImageTracking/Images |
| [Energy Demand Module](#Energy-Demand-Module) | ../Asset/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs|
| [Mobility Flow Module](#Mobility-Flow-Module) | ../Asset/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs |
| [Social Economics (Energy Trade) Module](#Social-Economics-(Energy-Trade)-Module) | ../Asset/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs |
| [Debug Image Tracking](#Debug-Image-Tracking) | ../Asset/Scenes/ImageTracking/BasicImageTracking |

## Menu
 ../Asset/Scenes/ImageTracking/Menu
The menu scene is the start page of the app. It allows users (1)select the city, then (2)select the data module.

## Image
 ../Asset/Scenes/ImageTracking/Images
`ReferenceImageLibrary.asset` manages the images that can be detected in physical world. Use this to modify or add new images.
After modifying images, add the cooresponding prefabs that will be instantiated when the new image is detected under: (A module's scene, eg., EnergyDemandScene.unity)>>
More detail explanation of the code can be found on [AR Foundation Image Tracking package documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/features/image-tracking.html)

## Data Visualization Modules
Prefabs location: ../Asset/Scenes/ImageTracking/Prefabs
Scenes location: ../Asset/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs
Scripts location: ../Asset/Scenes/ImageTracking/Scripts

### Energy Demand Module

The annual energy demand is measured in kWh, and it's represented by a color bar on the right. This bar uses different shades of blue(for cooling)/red(for heating) to indicate the level of demand. Darker shades of blue/red represent higher demand. The darkest level matches the highest demand within the area of study. Each building displays its annual demand on top of the model, and its color corresponds to the color scale bar to show the level of demand. The annaul cooling demand data is visualized on both Zurich and Sinagpore models; the annual heating demand data is visualized on Zurich model.  

The following summarizes the code for Zurich case, the Sinagpore case shares the same structure. 
| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
| `ColorManager.cs` | `Loadjson` | Read and load the data from energy demand json file, store the data into dictionary with building id as Key and cooresponding coolding load as Value. | 
|                   |`AssignLoad`| Assign the cooling load on the attached building gameobject.  | 
|                   |`AssignColor`| Assign the color according to the level of demand of the attached building gameobject. | 
|                   |`LoadOnText`| Show cooling demand on top of the building gameobject by changing the text of its children gameobject: TextMeshPro  | 
|   `Colorbar.cs `  |`SetColorbar`| Set the color bar that indicates the level of demand. The highest demand value in this area is rounded to the nearest whole number matches the darkest color, and 0 demand value matches the white color, and the color gradient in between shows the interpolation between min and max value of demand. The maximum value is rounded to a whole number instead of an original value. | 
|                   |`LoadOnText`| Display the interpolated figure beside color bar.|
|`House.cs`         |class `Root`| Transferred data from json files using [JSON2CSHARP](https://json2csharp.com) online tool.|

| Unity Assets | Zurich Heating | Zurich Cooling | Singapore Cooling |
| :---------------------- | :------------- |:-------------- |:----------------- |
| Scene |`EnergyDemandScene.unity`|`QCEnergyDemandScene.unity`|`QCSingaporeEnergyDemandScene.unity`| 
| Prefabs |`ZRHmodel2809.prefab`|`ZRHModel_QC.prefab`|`Qc_SingaporeEnergyDemand.prefab`|
| Scripts |`ColorManager.cs`,`Colorbar.cs`|`QCColorManager.cs`,`QCColorbar.cs`|`QCSingColorManager.cs`,`QCSingColorbar.cs`|
| Data Scripts|`House.cs`|`House.cs`|`House.cs`|
|Data Files| | | |


### Mobility Flow Module
The mobility flow of vehicle and non-vehicle are demonstrated on virtual models which includes the terrain and buildings within areas of study. Given the location(latitude, longtitude) and time, each user(a preson/a vehicle) is represented by one dot(a sphere prefab) showing on the models according to time sequence. Based on real time record, the mobility flow is speed up by `timeSpeedupFactor`, which default value is 1000 times faster than real time. 


| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
| `MobilityFlow.cs` |`LoadJsonFlat`| Read and load the data from mobility json file, store the data `uid`, `time`, `lon`, `lat` into the list `dataList`, and sort out by time from the earliest to the oldest time. | 
|                   |IEnumerator `InstantiateSpheresWithTimeDelay`|fetch the data from dataList and instantiate sphere prefabs that represent the users according to time. The waiting time is the difference betweeen the previous time spot and the current time spot.| 
|                   |`MapCoordinatesToUnitySpace`| Map latitude and longitude to their corresponding locations on the model using reference points and their respective coordinates from Google Maps, allowing the calculation of new points on the model. | 
|                   |`ShowTime`| Display time on the progress bar.  | 

| Unity Assets | Zurich | Singapore |
| :---------------------- | :------------- |:-------------- |
| Scene |`MobilityScene.unity`|`SingMobilityScene.unity`| 
| Prefabs |`ZurichModel0210_buildingterrain.prefab`|`SingaporeMobility.prefab`|
| Scripts |`MobilityFlow.cs`|`SingMobilityFlow.cs`|
| Data Scripts |`ZurichMobilityJson.cs`|`ZurichMobilityJson.cs`|

### Social Economics (Energy Trade) Module
The social economics module illustrates energy trading among buildings in the area over the course of a day. When a building's PV panel generates surplus electricity, it trades it with neighboring buildings. Buildings turn blue when selling excess energy, displaying the amount in kWh above them, while those purchasing electricity turn yellow, showing the electricity they receive. Arrows indicate the trading direction and amount (indicated by arrow width). 

| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
|`EnergyTrade.cs`|`LoadJson`| Read and load data from energy trade json file. `FromBuilding` sells `transmission` amount of electricity to `ToBuilding` at hour `T`, and these data are stored in a list. | 
|                |IEnumerator `StartLaunchLineRenderers`| Fetch data from dataList and assign coorsponding building on the model, and then pass the data to initiate `TEMPChangeBuildingColor`| 
|                |IEnumerator `TEMPChangeBuildingColor`| Assign "from building" locaiton as the start point of the arrow(line renderer) and "to building" as end point. Change the color to blue for "from building" and yellow for "to building". Arrow width is scaled to fit the model size and indicates the amoudn of transmission electricity. The arrows are destroyed after `prefabStayTime` | 
|                   |`ShowTime`| Display time on progress bar(slider) | 

| Unity Assets | Zurich | Singapore |
| :---------------------- | :------------- |:-------------- |
| Scene |`EnergyTradeScene.unity`|`EnergyTradeSingaporeScene.unity`| 
| Prefabs |`ZRHmodelEnergyTrade.prefab`|`EnergyTradeSingapore.prefab`|
| Scripts |`EnergyTrade.cs`|`SingEnergyTrade.cs`|
| Data Scripts |`DataEnergyTrade.cs`|`DataEnergyTrade.cs`|

## Debug Image Tracking
| Unity Assets | Content | Description |
| :---------------------- | :------------- | :------------- |
| Scene |`BasicImageTracking.unity`| When an image in the `ReferenceImageLibrary.asset` is detected, the information of this image is displayed in virtual space. |
| Scripts |`TrackedImageInfoManager.cs`||
