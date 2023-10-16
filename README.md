# Interactive Data Visualization and Augmented Reality of Urban Photovoltaic

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

## Annual Energy Demand Module

### Cooling Demand
The annual cooling demand is measured in kWh, and it's represented by a color bar on the right. This bar uses different shades of blue to indicate the level of demand. Darker shades of blue represent higher cooling demand. Each building displays its annual cooling demand on top of the model, and its color corresponds to the color scale bar to show the level of demand.

| Scripts | Function | Description |
| `ColorManager.cs` |  `Loadjson` | Read and load the data from energy demand json file, store the data into dictionary with building id as Key and cooresponding coolding load as Value. | 
|                   |`AssignLoad`| Assign the cooling load on the attached building gameobject.  | 
|                   |`AssignLoad`| Assign the color according to the level of demand of the attached building gameobject. | 
| [Simple AR](#simple-ar) | Demonstrates basic Plane detection and Raycasting
| [Camera](#camera) | Scenes that demonstrate Camera features |
| [Plane detection](#plane-detection) | Scenes that demonstrate Plane detection |
| [Image tracking](#image-tracking) | Scenes that demonstrate Image tracking |


### Heating Demand

There are two samples demonstrating image tracking. The image tracking samples are supported on ARCore and ARKit. To enable image tracking, you must first create an `XRReferenceImageLibrary`. This is the set of images to look for in the environment. [Click here](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/features/image-tracking.html) for instructions on creating one.

You can also add images to the reference image library at runtime. This sample includes a button that adds the images `one.png` and `two.png` to the reference image library. See the script `DynamicLibrary.cs` for example code.

Run the sample on an ARCore or ARKit-capable device and point your device at one of the images in [`Assets/Scenes/ImageTracking/Images`](https://github.com/Unity-Technologies/arfoundation-samples/tree/master/Assets/Scenes/ImageTracking/Images). They can be displayed on a computer monitor; they do not need to be printed out.

## Mobility Flow

At runtime, ARFoundation will generate an `ARTrackedImage` for each detected reference image. This sample uses the [`TrackedImageInfoManager.cs`](https://github.com/Unity-Technologies/arfoundation-samples/blob/master/Assets/Scenes/ImageTracking/BasicImageTracking/TrackedImageInfoManager.cs) script to overlay the original image on top of the detected image, along with some meta data.

## Social Economic Energy Trade

With [`PrefabImagePairManager.cs`](https://github.com/Unity-Technologies/arfoundation-samples/blob/master/Assets/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs/PrefabImagePairManager.cs) script, you can assign different prefabs for each image in the reference image library.

You can also change prefabs at runtime. This sample includes a button that switch between the original and alternative prefab for the first image in the reference image library. See the script [`DynamicPrefab.cs`](https://github.com/Unity-Technologies/arfoundation-samples/blob/master/Assets/Scenes/ImageTracking/ImageTrackingWithMultiplePrefabs/DynamicPrefab.cs) for example code.
