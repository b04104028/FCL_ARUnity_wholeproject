# Interactive Data Visualization and Augmented Reality of Urban Photovoltaic
## Introduction
This project was created for the Future Cities Lab Exhibition, aiming to visualize influencial data of photovoltaic power usage using augmented reality. It utilizes 3D printed models of two sites, Zurich and Singapore, and is composed of three key modules: energy demand(cooling and heating), mobility flow (both vehicle and non-vehicle), and social economics(energy trading within these regions). 
By inputing these data in a specific format, the project creates a virtual overlay on the physical 3D models, providing a direct and clear view of the real situations in the designated areas.

## Facts
This project is developed based on [AR Foundation 5.1](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html) and its demonstration example [AR Foundation Sample Project](https://github.com/Unity-Technologies/arfoundation-samples).

It depends on 2 Unity packages:

* [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html)
* [Apple ARKit XR Plug-in](https://docs.unity3d.com/Packages/com.unity.xr.arkit@5.1/manual/index.html) on iOS

## Version

The `ARFoundationAllBackup` branch of this repository uses AR Foundation 5.1 and is compatible with Unity 2021.2 and later.

## How to use 

### Build and run on device

You can build the AR Foundation Samples project directly to device, which can be a helpful introduction to using AR Foundation features for the first time.

To build to device, follow the steps below:

1. Install Unity 2021.2 or later and clone this repository.

2. Open the Unity project at the root of this repository.

3. As with any other Unity project, go to [Build Settings](https://docs.unity3d.com/Manual/BuildSettings.html), select your target platform, and build this project.

### Understand the sample code

All sample scenes in this project can be found in the `Assets/Scenes` folder. To learn more about the AR Foundation components used in each scene, see the [AR Foundation Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html). Each scene is explained in more detail below.

# Table of Contents

| Sample scene(s) | Description |
| :-------------- | :---------- |
| [Simple AR](#simple-ar) | Demonstrates basic Plane detection and Raycasting
| [Camera](#camera) | Scenes that demonstrate Camera features |
| [Plane detection](#plane-detection) | Scenes that demonstrate Plane detection |
| [Image tracking](#image-tracking) | Scenes that demonstrate Image tracking |

## Menu

## Energy Demand Module

The annual cooling demand is measured in kWh, and it's represented by a color bar on the right. This bar uses different shades of blue to indicate the level of demand. Darker shades of blue represent higher cooling demand. The darkest level matches the highest cooling demand in this model data. Each building displays its annual cooling demand on top of the model, and its color corresponds to the color scale bar to show the level of demand.

| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
| `ColorManager.cs` |  `Loadjson` | Read and load the data from energy demand json file, store the data into dictionary with building id as Key and cooresponding coolding load as Value. | 
|                   |`AssignLoad`| Assign the cooling load on the attached building gameobject.  | 
|                   |`AssignColor`| Assign the color according to the level of demand of the attached building gameobject. | 
|                   |`LoadOnText`| Show cooling demand on top of the building gameobject by changing the text of its children gameobject: TextMeshPro  | 

### Cooling Demand
The annaul cooling demand data is visualized on both Zurich and Sinagpore models. 

### Heating Demand
The annual heating demand data is visualized on Zurich model.  

## Mobility Flow Module
The mobility flow of vehicle and non-vehicle are demonstrated on virtual models which includes the terrain and buildings within areas of study. Given the location(latitude, longtitude) and time, each user(a preson/a vehicle) is represented by one dot(a sphere prefab) showing on the models according to time sequence. Based on real time record, the mobility flow is speed up by `timeSpeedupFactor`, which default value is 1000 times faster than real time. 


| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
| `MobilityFlow.cs` |`LoadJsonFlat`| Read and load the data from mobility json file, store the data `uid`, `time`, `lon`, `lat` into the list `dataList`, and sort out by time from the earliest to the oldest time. | 
|                   |IEnumerator `InstantiateSpheresWithTimeDelay`|fetch the data from dataList and instantiate sphere prefabs that represent the users according to time. The waiting time is the difference betweeen the previous time spot and the current time spot.| 
|                   |`MapCoordinatesToUnitySpace`| Map latitude and longitude to their corresponding locations on the model using reference points and their respective coordinates from Google Maps, allowing the calculation of new points on the model. | 
|                   |`ShowTime`| Show time on the progress bar.  | 


## Social Economics (Energy Trade) ModuleThe social economics module illustrates energy trading among buildings in the area over the course of a day. When a building's PV panel generates surplus electricity, it trades it with neighboring buildings. Buildings turn blue when selling excess energy, displaying the amount in kWh above them, while those purchasing electricity turn yellow, showing the electricity they receive. Arrows indicate the trading direction and amount (indicated by arrow width). 

| Scripts | Function | Description |
| :-------------- | :---------- |:---------- |
|`EnergyTrade.cs`|`LoadJson`| Read and load data from energy trade json file. `FromBuilding` sells `transmission` amount of electricity to `ToBuilding` at hour `T`, and these data are stored in a list. | 
|                |IEnumerator `StartLaunchLineRenderers`| Fetch data from dataList and assign coorsponding building on the model, and then pass the data to initiate `TEMPChangeBuildingColor`| 
|                |IEnumerator `TEMPChangeBuildingColor`| Assign "from building" locaiton as the start point of the arrow(line renderer) and "to building" as end point. Change the color to blue for "from building" and yellow for "to building". Arrow width is scaled to fit the model size and indicates the amoudn of transmission electricity. The arrows are destroyed after `prefabStayTime` | 
|                   |`ShowTime`| Display time on progress bar(slider) | 

